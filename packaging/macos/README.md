# macOS: .app y .dmg

## Publicar

```bash
# Apple Silicon
dotnet publish src/ComicPackager.App -c Release -r osx-arm64 --self-contained true \
  -p:PublishSingleFile=true -o dist/osx-arm64

# Intel
dotnet publish src/ComicPackager.App -c Release -r osx-x64 --self-contained true \
  -p:PublishSingleFile=true -o dist/osx-x64
```

## Armar el .app

```
PanelPack.app/
  Contents/
    Info.plist
    MacOS/PanelPack      ← binario publicado
    Resources/AppIcon.icns
```

`Info.plist` mínimo: `CFBundleName=PanelPack`, `CFBundleExecutable=PanelPack`, `CFBundleIdentifier=dev.panelpack.app`, `LSMinimumSystemVersion=12.0`.

Notarización (Apple Developer): `codesign --deep --force --sign "Developer ID" PanelPack.app` y `notarytool submit`.

## .dmg

```bash
hdiutil create -volname PanelPack -srcfolder PanelPack.app -ov -format UDZO PanelPack.dmg
```

O [create-dmg](https://github.com/create-dmg/create-dmg) para ventana con icono y atajo a `/Applications`.

## CBR en macOS

Homebrew no empaqueta el `rar` propietario de RARLAB. Hay que instalarlo desde rarlab.com (`/usr/local/bin/rar` o `/opt/homebrew/bin/rar`). Sin él, solo CBZ.
