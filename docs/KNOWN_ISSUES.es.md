# Issues conocidos

## CBR / binario `rar`

- Crear un CBR **válido** requiere el compresor `rar` de RARLAB (WinRAR en Windows, el paquete comercial `rar` en Linux/macOS).
- `unrar`, `unar` y 7-Zip **no** se usan: no generan un RAR original, o solo extraen.
- Si `rar` no está en el PATH ni en las rutas habituales (`C:\Program Files\WinRAR\Rar.exe`, `/usr/bin/rar`, `/opt/homebrew/bin/rar`), la UI **deshabilita CBR**, muestra el motivo y deja solo CBZ.
- **Nunca** se escribe un ZIP y se le cambia la extensión a `.cbr`. Eso rompe lectores estrictos y es exactamente lo que este programa evita.
- Tras crear un CBR no se puede verificar el índice interno sin `unrar`/`rar l`; solo se comprueba que el archivo existe y no está vacío.
- WinRAR no se redistribuye (licencia propietaria). El usuario debe instalarlo por su cuenta.

## Formato y lectores

- Algunos lectores antiguos ignoran `ComicInfo.xml` y no respetan `YesAndRightToLeft`. El checkbox RTL no reordena páginas a propósito, para no confundir a los que sí leen metadatos. Si las imágenes se cargaron al revés, usar «Invertir orden de páginas ahora».
- Komga/Kavita leen `Manga = YesAndRightToLeft`. Otros pueden tratar cualquier `Yes*` como manga.
- El campo `Format` se escribe `Digital` (cómic/manga) o `Web` (manhwa/webtoon). No es el formato de archivo CBZ/CBR.

## Imágenes

- `.tiff`/`.tif` es opcional: Skia puede no decodificar todos los TIFF (especialmente comprimidos con LZW/JPEG). Esos archivos se marcan como corruptos/no decodificables.
- GIF animados: se empaqueta el archivo original; la miniatura suele ser el primer fotograma.
- JPEG progresivos o CMYK raros pueden fallar al generar miniatura y reportarse como no decodificables aunque otro visor los abra.
- No se recodifican las páginas al empaquetar: se copian tal cual (STORE en ZIP). Si el original está corrupto, el CBZ también lo estará.

## Empaquetado

- Máximo 9999 páginas por el padding `0001`–`9999`.
- Nombres de archivo de salida se sanitizan (`: / \ * ?` → `_`) para que el mismo nombre sirva en Windows, Linux y macOS.
- Si el destino ya existe, se pregunta antes de sobrescribir. Un empaquetado a medias se intenta borrar.
- Las miniaturas viven en `%TEMP%/ComicPackager/thumbs` (o `/tmp/ComicPackager/thumbs`). No se limpian al salir, para reutilizar caché; se pueden borrar a mano.

## UI

- El icono actual es el de la plantilla Avalonia (no hay branding propio).
- En Wayland, el drag & drop desde el gestor de archivos depende del compositor.
- La vista previa grande limita el decode a 2048 px de lado para no cargar escaneos de 6000 px en RAM.
- Cambiar Serie/Número/Volumen regenera el nombre de archivo y puede pisar una edición manual previa.

## Lo que este programa no hace (a propósito)

- No convierte a PDF.
- No requiere cuenta, red ni API.
- No reordena páginas en silencio al marcar manga/RTL.
- No pone las imágenes en subcarpetas del zip.
- No incluye el binario `rar`.
