# Linux: AppImage y .deb

## Publicar

```bash
dotnet publish src/ComicPackager.App -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -o dist/linux-x64
```

También `linux-arm64` si hace falta.

El binario sale como `ComicPackager`. Dependencias nativas de Skia van incluidas en self-contained.

## AppImage (recomendado para distros varias)

1. Instala [appimagetool](https://github.com/AppImage/AppImageKit).
2. Crea `ComicPackager.AppDir`:

```
ComicPackager.AppDir/
  AppRun                       → script que ejecuta usr/bin/ComicPackager
  comic-packager.desktop
  comic-packager.png
  usr/bin/ComicPackager        → binario publicado
```

`packaging/linux/comic-packager.desktop` es la plantilla. `AppRun`:

```bash
#!/bin/sh
HERE="$(dirname "$(readlink -f "$0")")"
exec "$HERE/usr/bin/ComicPackager" "$@"
```

3. `appimagetool ComicPackager.AppDir ComicPackager-x86_64.AppImage`

## .deb

Estructura mínima:

```
debian-pkg/
  DEBIAN/control
  usr/bin/comic-packager                 → wrapper o el binario
  usr/share/applications/comic-packager.desktop
  usr/lib/comic-packager/                → publicación
```

`control`:

```
Package: comic-packager
Version: 0.1.0
Section: graphics
Priority: optional
Architecture: amd64
Maintainer: Comic Packager
Description: Empaquetador de cómics CBZ/CBR con ComicInfo.xml
```

```bash
dpkg-deb --build debian-pkg comic-packager_0.1.0_amd64.deb
```

El CBR seguirá deshabilitado salvo que el usuario instale `rar` de RARLAB (no está en los repos de Debian de forma legal/libre).
