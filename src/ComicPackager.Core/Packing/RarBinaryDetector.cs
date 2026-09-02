namespace ComicPackager.Core.Packing;

/// <summary>
/// Localiza el binario `rar` de RARLAB / WinRAR. No usamos `unrar` (solo extrae)
/// ni fingimos un CBR renombrando un ZIP.
/// </summary>
public static class RarBinaryDetector
{
    public const string UnavailableMessageEs =
        "No se encontró el binario `rar` (WinRAR o RAR de RARLAB) en el sistema. " +
        "El formato CBR está deshabilitado porque no se puede crear un RAR válido. " +
        "Usa CBZ (ZIP), el formato recomendado. " +
        "Para habilitar CBR, instala WinRAR (Windows) o el paquete `rar` de RARLAB (Linux/macOS) y asegúrate de que `rar` esté en el PATH.";

    public const string UnavailableMessageEn =
        "The `rar` binary (WinRAR or RARLAB rar) was not found. " +
        "CBR is disabled because a valid RAR archive cannot be created. " +
        "Use CBZ (ZIP), the recommended format. " +
        "To enable CBR, install WinRAR (Windows) or RARLAB's `rar` (Linux/macOS) and make sure `rar` is on PATH.";

    public static string? Find()
    {
        foreach (var candidate in Candidates())
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;
            if (File.Exists(candidate))
                return candidate;
        }

        return FindOnPath();
    }

    public static bool IsAvailable() => Find() is not null;

    private static IEnumerable<string> Candidates()
    {
        if (OperatingSystem.IsWindows())
        {
            var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            yield return Path.Combine(pf, "WinRAR", "Rar.exe");
            yield return Path.Combine(pf86, "WinRAR", "Rar.exe");
            yield return Path.Combine(pf, "WinRAR", "rar.exe");
        }
        else
        {
            yield return "/usr/bin/rar";
            yield return "/usr/local/bin/rar";
            yield return "/opt/homebrew/bin/rar";
            yield return "/opt/rar/rar";
        }
    }

    private static string? FindOnPath()
    {
        var names = OperatingSystem.IsWindows()
            ? new[] { "rar.exe", "Rar.exe", "rar" }
            : new[] { "rar" };

        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var parts = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        foreach (var dir in parts)
        {
            foreach (var name in names)
            {
                try
                {
                    var full = Path.Combine(dir.Trim('"'), name);
                    if (File.Exists(full))
                        return full;
                }
                catch
                {
                    // Ignorar entradas de PATH ilegales.
                }
            }
        }

        return null;
    }
}
