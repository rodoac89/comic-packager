# Comic Packager

[![CI](https://github.com/rodoac89/comic-packager/actions/workflows/ci.yml/badge.svg)](https://github.com/rodoac89/comic-packager/actions/workflows/ci.yml)
[![Release](https://github.com/rodoac89/comic-packager/actions/workflows/release.yml/badge.svg)](https://github.com/rodoac89/comic-packager/actions/workflows/release.yml)
[![GitHub release](https://img.shields.io/github/v/release/rodoac89/comic-packager)](https://github.com/rodoac89/comic-packager/releases/latest)

<img src="./src/ComicPackager.App/Assets/ComicPackager-light.png" alt="Comic Packager Logo" width="150" height="150" />

Cross-platform desktop app for packaging comic and manga images into **CBZ** or **CBR**, with a visual page editor and `ComicInfo.xml` metadata compatible with modern readers.

## Usage

1. Add the images to package by selecting individual files, folder or drag & drop.
2. Reorder pages (if needed) in the grid using drag & drop.
3. Mark the cover.
4. Fill the metadata. If it's Manga, you can enable right-to-left reading.
5. Choose the output format (CBZ or CBR).
6. Click **Package** and pick a destination.
7. During the process, a `ComicInfo.xml` file is generated at the archive root with the metadata.

Shortcuts: `Ctrl+O` add, `Delete` remove selection, `Ctrl+Enter` package.

## Supported formats

| Format | What it is | When to use it |
| --- | --- | --- |
| **CBZ** | ZIP with a `.cbz` extension | Default. No extra software required. |
| **CBR** | Real RAR with a `.cbr` extension | Only if the `rar` binary is available. Otherwise CBR is disabled and the reason is explained. A ZIP renamed to `.cbr` is **never** created. |

## Requirements for contributing

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- (Optional) WinRAR / RARLAB `rar` if you want to use CBR
- Linux: `libicu` and the native Skia dependencies that Avalonia includes when publishing

```bash
git clone https://github.com/rodoac89/comic-packager.git
cd comic-packager
dotnet restore
dotnet test
dotnet run --project src/ComicPackager.App
```

## Structure

```
comic-packager/
├── ComicPackager.slnx
├── src/
│   ├── ComicPackager.Core/          # domain with no UI
│   │   ├── Models/                  # PageItem, ComicMetadata, formats
│   │   ├── Import/                  # natural sort, import, reordering
│   │   ├── Metadata/                # ComicInfo.xml + file name
│   │   ├── Packing/                 # CBZ (ZIP), CBR (rar), validation
│   │   └── Thumbnails/              # on-disk cache, bounded decode
│   └── ComicPackager.App/           # Avalonia UI (MVVM)
├── tests/ComicPackager.Tests/
├── examples/                        # sample ComicInfo RTL / LTR
└── packaging/                       # installer notes
```

## Technical details

Each file generated contains:

- **Image files**: `0001.jpg`, `0002.png`, … (4-digit padding; the original extension will kept)
- `ComicInfo.xml` (UTF-8, Anansi / ComicInfo v2.0–v2.1 schema) with the metadata
- ZIP compression: **STORE** for images (they are already compressed) and fast DEFLATE for the XML

## Dependencies and licenses

| Component | License | Notes |
| --- | --- | --- |
| Avalonia UI | MIT | UI |
| CommunityToolkit.Mvvm | MIT | MVVM |
| SkiaSharp | MIT | Thumbnails and corrupt-file detection |
| .NET / System.IO.Compression | MIT | CBZ |
| `rar` (RARLAB / WinRAR) | Proprietary | **Not bundled.** The user need to install it if they want CBR. |

## How to build installers

Details in `packaging/`. Summary:

```bash
# Windows (portable folder; the .msi is built with WiX or Inno Setup)
dotnet publish src/ComicPackager.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist/win

# Linux
dotnet publish src/ComicPackager.App -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o dist/linux

# macOS
dotnet publish src/ComicPackager.App -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o dist/osx

```

- Linux: AppImage (`packaging/linux`) or `.deb`.
- Windows: Inno Setup / WiX for `.exe` or `.msi` (`packaging/windows`).
- macOS: `.app` + `.dmg` (`packaging/macos`).

## Known issues

See [docs/KNOWN_ISSUES.md](docs/KNOWN_ISSUES.md).

Made in Chile 🇨🇱
