using ComicPackager.Core.Metadata;
using ComicPackager.Core.Models;

namespace ComicPackager.Core.Packing;

public sealed class PackValidator
{
    public ValidationResult Validate(PackRequest request, bool rarAvailable)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Metadata);
        var issues = new List<ValidationIssue>();
        var metadata = request.Metadata;

        if (request.Pages is null || request.Pages.Count == 0)
        {
            issues.Add(new ValidationIssue("NoPages", "Añade al menos una imagen para empaquetar."));
        }
        else if (request.Pages.Count > 9999)
        {
            issues.Add(new ValidationIssue("TooManyPages", "El máximo es 9999 páginas (padding de 4 dígitos)."));
        }

        if (string.IsNullOrWhiteSpace(metadata.OutputFileName))
        {
            issues.Add(new ValidationIssue("NoFileName", "Indica un nombre de archivo de salida."));
        }
        else
        {
            var name = OutputFileNameBuilder.SanitizeFileName(
                Path.GetFileNameWithoutExtension(metadata.OutputFileName));
            if (string.IsNullOrWhiteSpace(name))
                issues.Add(new ValidationIssue("NoFileName", "El nombre de archivo de salida no es válido."));
        }

        if (string.IsNullOrWhiteSpace(metadata.DestinationFolder))
        {
            issues.Add(new ValidationIssue("NoDestination", "Elige una carpeta de destino."));
        }
        else if (!IsWritableDirectory(metadata.DestinationFolder, out var reason))
        {
            issues.Add(new ValidationIssue("DestinationNotWritable", reason));
        }

        if (metadata.OutputFormat == OutputFormat.Cbr && !rarAvailable)
        {
            issues.Add(new ValidationIssue("CbrUnavailable", RarBinaryDetector.UnavailableMessageEs));
        }

        if (metadata.Month is int month && (month < 1 || month > 12))
            issues.Add(new ValidationIssue("InvalidMonth", "El mes debe estar entre 1 y 12."));
        if (metadata.Day is int day && (day < 1 || day > 31))
            issues.Add(new ValidationIssue("InvalidDay", "El día debe estar entre 1 y 31."));

        return issues.Count == 0 ? ValidationResult.Ok() : new ValidationResult(issues);
    }

    public static bool IsWritableDirectory(string folder, out string reason)
    {
        reason = string.Empty;
        try
        {
            var full = Path.GetFullPath(folder);
            if (!Directory.Exists(full))
            {
                Directory.CreateDirectory(full);
            }

            var probe = Path.Combine(full, ".comicpackager-write-test-" + Guid.NewGuid().ToString("N"));
            File.WriteAllText(probe, "ok");
            File.Delete(probe);
            return true;
        }
        catch (Exception ex)
        {
            reason = $"No se puede escribir en la carpeta de destino: {ex.Message}";
            return false;
        }
    }
}
