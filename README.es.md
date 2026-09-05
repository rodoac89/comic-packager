# Comic Packager

[![CI](https://github.com/rodoac89/comic-packager/actions/workflows/ci.yml/badge.svg)](https://github.com/rodoac89/comic-packager/actions/workflows/ci.yml)
[![Release](https://github.com/rodoac89/comic-packager/actions/workflows/release.yml/badge.svg)](https://github.com/rodoac89/comic-packager/actions/workflows/release.yml)
[![GitHub release](https://img.shields.io/github/v/release/rodoac89/comic-packager)](https://github.com/rodoac89/comic-packager/releases/latest)

<img src="./src/ComicPackager.App/Assets/ComicPackager-light.png" alt="Comic Packager Logo" width="150" height="150" />

Aplicación de escritorio multiplataforma para empaquetar imágenes de cómics y mangas en **CBZ** o **CBR**, con editor visual de páginas y metadatos `ComicInfo.xml` compatible con lectores modernos.

## Uso

1. Añadir las imágenes a empaquetar ya sea mediante la carga de archivos individuales, carpeta o drag & drop.
2. Reordenar (si se requiere) en la cuadrícula usando drag&drop.
3. Marcar portada.
4. Rellenar metadatos. Si el tipo es Manga, se puede seleccionar la opción de lectura inversa.
5. Seleccionar el formato de salida (CBZ o CBR)
6. Hacer clic en «Empaquetar» y elegir destino.
7. Durante el proceso se genera `ComicInfo.xml` en la raíz del archivo con los metadatos.

Atajos: `Ctrl+O` añadir, `Supr` quitar selección, `Ctrl+Enter` empaquetar.

## Formatos soportados

| Formato | Qué es | Cuándo |
| --- | --- | --- |
| **CBZ** | ZIP con extensión `.cbz` | Por defecto. No requiere software extra. |
| **CBR** | RAR real con extensión `.cbr` | Solo si existe el binario `rar`. Si no, CBR se deshabilita y se explica por qué. **Nunca** se crea un ZIP renombrado a `.cbr`. |

## Requisitos para contribuir

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- (Opcional) WinRAR / `rar` de RARLAB si quiere utilizar CBR
- Linux: `libicu` y las dependencias nativas de Skia que trae Avalonia al publicar

```bash
git clone https://github.com/rodoac89/comic-packager.git
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

## Detalles técnicos
Dentro de cada archivo generado se crea lo siguiente:

- **Archivos de imagenes**: `0001.jpg`, `0002.png`, … (padding de 4 dígitos; se conserva la extensión original)
- `ComicInfo.xml` (UTF-8, esquema Anansi / ComicInfo v2.0–v2.1) que contiene los metados del archivo
- Compresión ZIP: **STORE** para imágenes (ya vienen comprimidas) y DEFLATE rápido para el XML.

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
dotnet publish src/ComicPackager.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist/win

# Linux
dotnet publish src/ComicPackager.App -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o dist/linux

# macOS
dotnet publish src/ComicPackager.App -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o dist/osx

```

- Linux: AppImage (`packaging/linux`) o `.deb`.
- Windows: Inno Setup / WiX para `.exe` o `.msi` (`packaging/windows`).
- macOS: `.app` + `.dmg` (`packaging/macos`).

## Issues conocidos

Ver [docs/KNOWN_ISSUES.md](docs/KNOWN_ISSUES.es.md). 


Made in Chile 🇨🇱
