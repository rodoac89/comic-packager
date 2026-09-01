using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ComicPackager.App.Services;

public sealed class LocalizationService : INotifyPropertyChanged
{
    private string _language = "es";

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Language
    {
        get => _language;
        set
        {
            var normalized = value is "en" ? "en" : "es";
            if (_language == normalized)
                return;
            _language = normalized;
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged("Item[]");
        }
    }

    public string this[string key] => Get(key);

    public string Get(string key)
    {
        if (Tables.TryGetValue(_language, out var table) && table.TryGetValue(key, out var value))
            return value;
        if (Tables["es"].TryGetValue(key, out var fallback))
            return fallback;
        return key;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private static readonly Dictionary<string, Dictionary<string, string>> Tables = new()
    {
        ["es"] = new()
        {
            ["AppTitle"] = "PanelPack — comic-packager",
            ["Metadata"] = "Metadatos",
            ["Title"] = "Título",
            ["Series"] = "Serie",
            ["Number"] = "Número",
            ["Volume"] = "Volumen",
            ["Writer"] = "Autor / escritor",
            ["Artist"] = "Artista",
            ["Publisher"] = "Editorial",
            ["Year"] = "Año",
            ["Month"] = "Mes",
            ["Day"] = "Día",
            ["Genre"] = "Género",
            ["LanguageIso"] = "Idioma (ISO)",
            ["Summary"] = "Resumen / sinopsis",
            ["BookType"] = "Tipo",
            ["TypeComic"] = "Cómic",
            ["TypeManga"] = "Manga",
            ["TypeManhwa"] = "Manhwa / Webtoon",
            ["Rtl"] = "Lectura inversa (derecha a izquierda)",
            ["RtlHint"] = "Esto solo escribe metadatos (Manga = YesAndRightToLeft). No reordena las imágenes.",
            ["MangaHint"] = "Manga: se marca RTL por defecto. Los lectores respetarán ComicInfo.xml.",
            ["ManhwaHint"] = "Manhwa/Webtoon: lectura izquierda-derecha o vertical. RTL no se marca.",
            ["BlackAndWhite"] = "Blanco y negro",
            ["OutputFormat"] = "Formato de salida",
            ["CbzRecommended"] = "CBZ (recomendado)",
            ["Cbr"] = "CBR",
            ["CbrDisabled"] = "CBR deshabilitado: no hay binario `rar` en el sistema. No se finge un CBR. Usa CBZ.",
            ["OutputFileName"] = "Nombre del archivo",
            ["Destination"] = "Carpeta de destino",
            ["Browse"] = "Examinar…",
            ["Pages"] = "Páginas",
            ["AddFiles"] = "Añadir archivos…",
            ["AddFolder"] = "Añadir carpeta…",
            ["Recursive"] = "Incluir subcarpetas",
            ["MoveUp"] = "Subir",
            ["MoveDown"] = "Bajar",
            ["ReverseAll"] = "Invertir todas",
            ["ReverseSelected"] = "Invertir selección",
            ["ReverseNow"] = "Invertir orden de páginas ahora",
            ["MakeCover"] = "Marcar como portada",
            ["Remove"] = "Eliminar",
            ["Clear"] = "Limpiar",
            ["Pack"] = "Empaquetar",
            ["Zoom"] = "Zoom",
            ["Theme"] = "Tema",
            ["ThemeDark"] = "Oscuro",
            ["ThemeLight"] = "Claro",
            ["ThemeSystem"] = "Sistema",
            ["LanguageUi"] = "Idioma",
            ["Spanish"] = "Español",
            ["English"] = "English",
            ["DropHint"] = "Arrastra una carpeta o imágenes aquí",
            ["NoPages"] = "Sin páginas. Añade imágenes o suelta una carpeta.",
            ["Cover"] = "Portada",
            ["PageType"] = "Tipo de página",
            ["Confirm"] = "Confirmar",
            ["Cancel"] = "Cancelar",
            ["Ok"] = "Aceptar",
            ["Yes"] = "Sí",
            ["No"] = "No",
            ["OverwriteTitle"] = "El archivo ya existe",
            ["OverwriteMessage"] = "¿Quieres sobrescribirlo?",
            ["DeleteMany"] = "Vas a eliminar {0} páginas. ¿Continuar?",
            ["ClearConfirm"] = "¿Quitar todas las páginas de la lista? Los archivos originales no se borran.",
            ["PackSuccess"] = "Archivo creado",
            ["PackSuccessBody"] = "Se creó {0} ({1}).",
            ["OpenFolder"] = "Abrir carpeta",
            ["ImportWarnings"] = "Algunos archivos no se añadieron",
            ["Corrupt"] = "Corruptos / no decodificables",
            ["Skipped"] = "Ignorados (no son imagen)",
            ["Duplicates"] = "Duplicados (misma ruta)",
            ["Error"] = "Error",
            ["Packing"] = "Empaquetando…",
            ["Ready"] = "Listo",
            ["PagesStatus"] = "{0} páginas · {1}",
            ["PickImages"] = "Añadir imágenes",
            ["PickFolder"] = "Añadir carpeta",
            ["PickDestination"] = "Carpeta de destino",
            ["Lightbox"] = "Vista previa",
        },
        ["en"] = new()
        {
            ["AppTitle"] = "PanelPack — comic-packager",
            ["Metadata"] = "Metadata",
            ["Title"] = "Title",
            ["Series"] = "Series",
            ["Number"] = "Number",
            ["Volume"] = "Volume",
            ["Writer"] = "Writer",
            ["Artist"] = "Artist",
            ["Publisher"] = "Publisher",
            ["Year"] = "Year",
            ["Month"] = "Month",
            ["Day"] = "Day",
            ["Genre"] = "Genre",
            ["LanguageIso"] = "Language (ISO)",
            ["Summary"] = "Summary",
            ["BookType"] = "Type",
            ["TypeComic"] = "Comic",
            ["TypeManga"] = "Manga",
            ["TypeManhwa"] = "Manhwa / Webtoon",
            ["Rtl"] = "Right-to-left reading",
            ["RtlHint"] = "This only writes metadata (Manga = YesAndRightToLeft). It does not reorder images.",
            ["MangaHint"] = "Manga: RTL is checked by default. Readers will honor ComicInfo.xml.",
            ["ManhwaHint"] = "Manhwa/Webtoon: left-to-right or vertical reading. RTL is not set.",
            ["BlackAndWhite"] = "Black and white",
            ["OutputFormat"] = "Output format",
            ["CbzRecommended"] = "CBZ (recommended)",
            ["Cbr"] = "CBR",
            ["CbrDisabled"] = "CBR disabled: no `rar` binary on this system. A CBR will not be faked. Use CBZ.",
            ["OutputFileName"] = "File name",
            ["Destination"] = "Destination folder",
            ["Browse"] = "Browse…",
            ["Pages"] = "Pages",
            ["AddFiles"] = "Add files…",
            ["AddFolder"] = "Add folder…",
            ["Recursive"] = "Include subfolders",
            ["MoveUp"] = "Move up",
            ["MoveDown"] = "Move down",
            ["ReverseAll"] = "Reverse all",
            ["ReverseSelected"] = "Reverse selection",
            ["ReverseNow"] = "Reverse page order now",
            ["MakeCover"] = "Set as cover",
            ["Remove"] = "Remove",
            ["Clear"] = "Clear",
            ["Pack"] = "Package",
            ["Zoom"] = "Zoom",
            ["Theme"] = "Theme",
            ["ThemeDark"] = "Dark",
            ["ThemeLight"] = "Light",
            ["ThemeSystem"] = "System",
            ["LanguageUi"] = "Language",
            ["Spanish"] = "Español",
            ["English"] = "English",
            ["DropHint"] = "Drop a folder or images here",
            ["NoPages"] = "No pages. Add images or drop a folder.",
            ["Cover"] = "Cover",
            ["PageType"] = "Page type",
            ["Confirm"] = "Confirm",
            ["Cancel"] = "Cancel",
            ["Ok"] = "OK",
            ["Yes"] = "Yes",
            ["No"] = "No",
            ["OverwriteTitle"] = "File already exists",
            ["OverwriteMessage"] = "Do you want to overwrite it?",
            ["DeleteMany"] = "You are about to remove {0} pages. Continue?",
            ["ClearConfirm"] = "Remove all pages from the list? Original files are not deleted.",
            ["PackSuccess"] = "File created",
            ["PackSuccessBody"] = "Created {0} ({1}).",
            ["OpenFolder"] = "Open folder",
            ["ImportWarnings"] = "Some files were not added",
            ["Corrupt"] = "Corrupt / undecodable",
            ["Skipped"] = "Skipped (not an image)",
            ["Duplicates"] = "Duplicates (same path)",
            ["Error"] = "Error",
            ["Packing"] = "Packaging…",
            ["Ready"] = "Ready",
            ["PagesStatus"] = "{0} pages · {1}",
            ["PickImages"] = "Add images",
            ["PickFolder"] = "Add folder",
            ["PickDestination"] = "Destination folder",
            ["Lightbox"] = "Preview",
        },
    };
}
