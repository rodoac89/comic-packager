using System.Diagnostics;
using ComicPackager.Core.Models;

namespace ComicPackager.Core.Packing;

/// <summary>
/// CBR = RAR con extensión .cbr. Requiere el binario `rar`. Nunca se finge un CBR.
/// </summary>
public sealed class RarCbrPacker : IComicPacker
{
    private readonly string? _rarPath;

    public RarCbrPacker(string? rarPath = null)
    {
        _rarPath = rarPath ?? RarBinaryDetector.Find();
    }

    public OutputFormat Format => OutputFormat.Cbr;

    public bool IsAvailable => !string.IsNullOrWhiteSpace(_rarPath) && File.Exists(_rarPath);

    public string? UnavailableReason => IsAvailable ? null : RarBinaryDetector.UnavailableMessageEs;

    public async Task PackAsync(
        string destinationPath,
        IReadOnlyList<PageItem> pages,
        string comicInfoXmlPath,
        IProgress<PackProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (!IsAvailable)
            throw new InvalidOperationException(UnavailableReason);

        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        ArgumentNullException.ThrowIfNull(pages);

        var tempDir = Path.Combine(Path.GetTempPath(), "ComicPackager", "rar-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var total = pages.Count + 2;
            for (var i = 0; i < pages.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var entryName = pages[i].ArchiveEntryName(i + 1);
                progress?.Report(new PackProgress
                {
                    Current = i + 1,
                    Total = total,
                    Message = $"Preparando {entryName}…",
                });
                File.Copy(pages[i].SourcePath, Path.Combine(tempDir, entryName), overwrite: true);
            }

            File.Copy(comicInfoXmlPath, Path.Combine(tempDir, "ComicInfo.xml"), overwrite: true);

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destinationPath))!);
            if (File.Exists(destinationPath))
                File.Delete(destinationPath);

            progress?.Report(new PackProgress
            {
                Current = total - 1,
                Total = total,
                Message = "Creando archivo RAR…",
            });

            // -ep  : nombres en la raíz, sin carpetas
            // -m0  : store (las imágenes ya están comprimidas)
            // -y   : sí a todo
            // -idq : silencioso
            var psi = new ProcessStartInfo
            {
                FileName = _rarPath!,
                WorkingDirectory = tempDir,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add("a");
            psi.ArgumentList.Add("-ep");
            psi.ArgumentList.Add("-m0");
            psi.ArgumentList.Add("-y");
            psi.ArgumentList.Add("-idq");
            psi.ArgumentList.Add(destinationPath);
            psi.ArgumentList.Add("*.*");

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException("No se pudo iniciar el proceso `rar`.");

            var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var err = await stderr.ConfigureAwait(false);
            _ = await stdout.ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"`rar` terminó con código {process.ExitCode}. {err}".Trim());
            }

            progress?.Report(new PackProgress
            {
                Current = total,
                Total = total,
                Message = "CBR creado.",
            });
        }
        finally
        {
            TryDeleteDirectory(tempDir);
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Best-effort: el SO limpiará el temporal.
        }
    }
}
