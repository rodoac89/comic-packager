# Windows: portable, .exe (Inno Setup) y .msi (WiX)

## Publicar

```powershell
dotnet publish src/ComicPackager.App -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:PublishTrimmed=false -o dist/win-x64
```

El ejecutable es `PanelPack.exe`. Esta carpeta ya es una build portable.

## Inno Setup (.exe)

Plantilla mínima (`packaging/windows/panelpack.iss` como punto de partida):

- `AppName=PanelPack`
- `AppVersion=0.1.0`
- `DefaultDirName={autopf}\PanelPack`
- `PrivilegesRequired=lowest` si se instala por usuario
- Source: `dist\win-x64\*`
- Acceso directo en menú inicio y escritorio opcional
- Asociación de archivos: no necesaria (PanelPack no es un lector)

Compilar con Inno Setup 6: `iscc panelpack.iss`

## WiX (.msi)

1. Instalar [WiX v5](https://wixtoolset.org/).
2. `wix build panelpack.wxs -o PanelPack.msi`

`Product` + `Package` + `Component` que instale `PanelPack.exe` en `ProgramFiles64Folder\PanelPack` y un atajo.

## CBR en Windows

Instalar [WinRAR](https://www.winrar.com/). PanelPack busca `Rar.exe` en `C:\Program Files\WinRAR\` y en el PATH. Sin eso, CBR aparece deshabilitado.
