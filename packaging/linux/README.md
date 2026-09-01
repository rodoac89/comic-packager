# Linux: AppImage y .deb

## Publicar

```bash
dotnet publish src/ComicPackager.App -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -o dist/linux-x64
```

También `linux-arm64` si hace falta.

El binario sale como `PanelPack`. Dependencias nativas de Skia van incluidas en self-contained.

## AppImage (recomendado para distros varias)

1. Instala [appimagetool](https://github.com/AppImage/AppImageKit).
2. Crea `PanelPack.AppDir`:

```
PanelPack.AppDir/
  AppRun                  → script que ejecuta usr/bin/PanelPack
  panelpack.desktop
  panelpack.png
  usr/bin/PanelPack       → binario publicado
```

`packaging/linux/panelpack.desktop` es la plantilla. `AppRun`:

```bash
#!/bin/sh
HERE="$(dirname "$(readlink -f "$0")")"
exec "$HERE/usr/bin/PanelPack" "$@"
```

3. `appimagetool PanelPack.AppDir PanelPack-x86_64.AppImage`

## .deb

Estructura mínima:

```
debian-pkg/
  DEBIAN/control
  usr/bin/panelpack            → wrapper o el binario
  usr/share/applications/panelpack.desktop
  usr/lib/panelpack/           → publicación
```

`control`:

```
Package: panelpack
Version: 0.1.0
Section: graphics
Priority: optional
Architecture: amd64
Maintainer: PanelPack
Description: Empaquetador de cómics CBZ/CBR con ComicInfo.xml
```

```bash
dpkg-deb --build debian-pkg panelpack_0.1.0_amd64.deb
```

El CBR seguirá deshabilitado salvo que el usuario instale `rar` de RARLAB (no está en los repos de Debian de forma legal/libre).
