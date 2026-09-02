namespace ComicPackager.Core.Models;

/// <summary>
/// Tipo editorial del libro. Afecta el campo Manga de ComicInfo.xml
/// y el valor por defecto de lectura inversa, nunca el orden de las imágenes.
/// </summary>
public enum BookType
{
    Comic = 0,
    Manga = 1,
    ManhwaWebtoon = 2,
}
