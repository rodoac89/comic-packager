# PanelPack (comic-packager)

Aplicación de escritorio multiplataforma para empaquetar imágenes de cómics y mangas en **CBZ** (recomendado) o **CBR**, con editor visual de páginas y `ComicInfo.xml` compatible con lectores modernos (Komga, Kavita, CDisplayEx, YACReader, etc.).

El producto se llama **PanelPack**. El `ComicInfo.xml` incluye `Notes = Created with PanelPack`.

## Por qué Avalonia y no GTK# / MonoDevelop

El stack pedido era desarrollo .NET tipo MonoDevelop. GTK# y MonoDevelop están en mantenimiento. **.NET 10 + Avalonia UI 12** es el equivalente profesional actual:

- El mismo lenguaje (C#) y el mismo patrón MVVM/XAML.
- Linux, Windows y macOS con un solo proyecto.
- Temas claro/oscuro, drag & drop y publicación autocontenida.
- Se abre en Visual Studio, VS Code, Rider o Cursor.

No hay cuentas, red ni APIs externas.

## Requisitos para desarrollar

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- (Opcional) WinRAR / `rar` de RARLAB si quieres crear CBR
- Linux: `libicu` y las dependencias nativas de Skia que trae Avalonia al publicar

```bash
git clone <repo>
cd comic-packager
dotnet restore
dotnet test
dotnet run --project src/ComicPackager.App
```

## Estructura

```
comic-packager/
├── ComicPackager.slnx
├── src/
│   ├── ComicPackager.Core/          # dominio sin UI
│   │   ├── Models/                  # PageItem, ComicMetadata, formatos
│   │   ├── Import/                  # natural sort, importación, reordenado
│   │   ├── Metadata/                # ComicInfo.xml + nombre de archivo
│   │   ├── Packing/                 # CBZ (ZIP), CBR (rar), validación
│   │   └── Thumbnails/              # caché en disco, decode acotado
│   └── ComicPackager.App/           # UI Avalonia (MVVM)
├── tests/ComicPackager.Tests/
├── examples/                        # ComicInfo de muestra RTL / LTR
└── packaging/                       # notas de instaladores
```

La **lista interna de páginas** es la fuente de verdad del orden en el archivo (`0001.ext`, `0002.ext`, …). Marcar «lectura inversa» **no** reordena las imágenes: escribe `<Manga>YesAndRightToLeft</Manga>`. Para invertir archivos hay un botón explícito.

## Flujo (captura textual)

```
┌──────────────────────────────────────────────────────────────────┐
│ PanelPack — comic-packager              [Español] [Oscuro]       │
├────────────────────┬─────────────────────────────────────────────┤
│ METADATOS          │  Miniaturas (cuadrícula)                    │
│ Título / Serie     │  ┌──────┐ ┌──────┐ ┌──────┐                 │
│ Nº  Volumen        │  │0001  │ │0002  │ │0003  │                 │
│ Autor / Artista    │  │ ★Por.│ │      │ │      │                 │
│ Editorial / Fecha  │  │cover │ │p02   │ │p03   │                 │
│ Género / Idioma    │  └──────┘ └──────┘ └──────┘                 │
│ Tipo: Cómic|Manga  │  Arrastra una carpeta o imágenes aquí       │
│ ☑ Lectura inversa  │                                             │
│ Formato: ● CBZ ○CBR│                                             │
│ Destino […]        │                                             │
├────────────────────┴─────────────────────────────────────────────┤
│ Añadir archivos…  Añadir carpeta…  ☑ Subcarpetas                 │
│ Subir  Bajar  Invertir ahora  Portada  Eliminar  Limpiar         │
│ Zoom ──●──   24 páginas · 15.2 MB                 [Empaquetar]   │
└──────────────────────────────────────────────────────────────────┘
```

1. Añadir imágenes (archivos, carpeta o drag & drop). Orden natural: página 2 antes que 10.
2. Reordenar en la cuadrícula (drag & drop, subir/bajar, invertir). Marcar portada (pasa a ser 0001 / FrontCover).
3. Rellenar metadatos. Si el tipo es Manga, RTL se marca solo; eso no mueve páginas.
4. Empaquetar. Barra de progreso, sin bloquear la UI. Al terminar, «Abrir carpeta».

Atajos: `Ctrl+O` añadir, `Supr` quitar selección, `Ctrl+Enter` empaquetar.

## Formatos

| Formato | Qué es | Cuándo |
| --- | --- | --- |
| **CBZ** | ZIP con extensión `.cbz` | Por defecto. No requiere software extra. |
| **CBR** | RAR real con extensión `.cbr` | Solo si existe el binario `rar`. Si no, CBR se deshabilita y se explica por qué. **Nunca** se crea un ZIP renombrado a `.cbr`. |

Dentro del archivo, en la **raíz** (sin carpetas):

- `0001.jpg`, `0002.png`, … (padding de 4 dígitos; se conserva la extensión original)
- `ComicInfo.xml` (UTF-8, esquema Anansi / ComicInfo v2.0–v2.1)

Compresión ZIP: **STORE** para imágenes (ya vienen comprimidas) y DEFLATE rápido para el XML.

## ComicInfo.xml y lectura inversa

| Tipo | Checkbox RTL | `<Manga>` |
| --- | --- | --- |
| Cómic occidental | no | `No` |
| Manga | sí (por defecto) | `YesAndRightToLeft` |
| Manga | no | `Yes` |
| Manhwa / Webtoon | no (LTR o vertical) | `No` (`Format=Web`) |

Ejemplos en `examples/ComicInfo.manga-rtl.xml` y `examples/ComicInfo.comic-ltr.xml`.

## Dependencias y licencias

| Pieza | Licencia | Notas |
| --- | --- | --- |
| Avalonia UI | MIT | UI |
| CommunityToolkit.Mvvm | MIT | MVVM |
| SkiaSharp | MIT | Miniaturas y detección de corruptos |
| .NET / System.IO.Compression | MIT | CBZ |
| `rar` (RARLAB / WinRAR) | Propietario | **No se incluye.** El usuario lo instala si quiere CBR. |

## Cómo construir instaladores

Detalles en `packaging/`. Resumen:

```bash
# Windows (carpeta portable; el .msi se arma con WiX o Inno Setup)
dotnet publish src/ComicPackager.App -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist/win

# Linux
dotnet publish src/ComicPackager.App -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -o dist/linux

# macOS
dotnet publish src/ComicPackager.App -c Release -r osx-arm64 --self-contained true \
  -p:PublishSingleFile=true -o dist/osx
```

- Linux: AppImage (`packaging/linux`) o `.deb`.
- Windows: Inno Setup / WiX para `.exe` o `.msi` (`packaging/windows`).
- macOS: `.app` + `.dmg` (`packaging/macos`).

## Issues conocidos

Ver [docs/KNOWN_ISSUES.md](docs/KNOWN_ISSUES.md). Los más importantes:

- CBR exige el binario `rar`. `unrar` no sirve (solo extrae).
- No se convierte a PDF a propósito.
- TIFF depende de los códecs de Skia en cada plataforma.
- Máximo 9999 páginas (nombres de 4 dígitos).
