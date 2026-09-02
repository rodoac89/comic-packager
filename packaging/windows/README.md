# Windows: portable, .exe (Inno Setup) y .msi (WiX)

## Publicar

```powershell
dotnet publish src/ComicPackager.App -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:PublishTrimmed=false -o dist/win-x64
```

El ejecutable es `ComicPackager.exe`. Esta carpeta ya es una build portable.

Para un release en GitHub no hace falta Inno/WiX: el workflow `.github/workflows/release.yml` genera `ComicPackager-win-x64.zip` al publicar un tag `vX.Y.Z`.

## Inno Setup (.exe)

Plantilla mínima (`packaging/windows/comic-packager.iss` como punto de partida):

- `AppName=Comic Packager`
- `AppVersion=0.1.0`
- `DefaultDirName={autopf}\Comic Packager`
- `PrivilegesRequired=lowest` si se instala por usuario
- Source: `dist\win-x64\*`
- Acceso directo en menú inicio y escritorio opcional
- Asociación de archivos: no necesaria (Comic Packager no es un lector)

Compilar con Inno Setup 6: `iscc comic-packager.iss`

## WiX (.msi)

1. Instalar [WiX v5](https://wixtoolset.org/).
2. `wix build comic-packager.wxs -o ComicPackager.msi`

`Product` + `Package` + `Component` que instale `ComicPackager.exe` en `ProgramFiles64Folder\Comic Packager` y un atajo.

## CBR en Windows

Instalar [WinRAR](https://www.winrar.com/). Comic Packager busca `Rar.exe` en `C:\Program Files\WinRAR\` y en el PATH. Sin eso, CBR aparece deshabilitado.
