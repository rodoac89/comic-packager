# Known issues

## CBR / `rar` binary

- Creating a **valid** CBR requires RARLAB’s `rar` compressor (WinRAR on Windows, the commercial `rar` package on Linux/macOS).
- `unrar`, `unar`, and 7-Zip are **not** used: they do not produce an original RAR, or they only extract.
- If `rar` is not on the PATH or in the usual locations (`C:\Program Files\WinRAR\Rar.exe`, `/usr/bin/rar`, `/opt/homebrew/bin/rar`), the UI **disables CBR**, shows the reason, and leaves only CBZ.
- A ZIP is **never** written and then renamed to `.cbr`. That breaks strict readers and is exactly what this program avoids.
- After creating a CBR, the internal index cannot be verified without `unrar`/`rar l`; the app only checks that the file exists and is not empty.
- WinRAR is not redistributed (proprietary license). The user must install it themselves.

## Format and readers

- Some older readers ignore `ComicInfo.xml` and do not honor `YesAndRightToLeft`. The RTL checkbox does not reorder pages on purpose, so it does not confuse readers that do respect metadata. If the images were loaded in reverse, use **Reverse page order now**.
- Komga/Kavita read `Manga = YesAndRightToLeft`. Others may treat any `Yes*` value as manga.
- The `Format` field is written as `Digital` (comic/manga) or `Web` (manhwa/webtoon). It is not the CBZ/CBR file format.

## Images

- `.tiff`/`.tif` is optional: Skia may not decode every TIFF (especially LZW/JPEG-compressed ones). Those files are marked as corrupt/undecodable.
- Animated GIFs: the original file is packed; the thumbnail is usually the first frame.
- Progressive JPEGs or unusual CMYK files may fail thumbnail generation and be reported as undecodable even if another viewer opens them.
- Pages are not re-encoded when packing: they are copied as-is (STORE in ZIP). If the original is corrupt, the CBZ will be too.

## Packing

- Maximum of 9999 pages because of the `0001`–`9999` padding.
- Output file names are sanitized (`: / \ * ?` → `_`) so the same name works on Windows, Linux, and macOS.
- If the destination already exists, the app asks before overwriting. A half-finished pack is deleted when possible.
- Thumbnails live in `%TEMP%/ComicPackager/thumbs` (or `/tmp/ComicPackager/thumbs`). They are not cleared on exit so the cache can be reused; they can be deleted manually.

## UI

- The current icon is the Avalonia template icon (there is no custom branding yet).
- On Wayland, drag & drop from the file manager depends on the compositor.
- The large preview limits decoding to 2048 px on the long side so 6000 px scans are not loaded into RAM.
- Changing Series/Number/Volume regenerates the file name and may overwrite a previous manual edit.

## What this program does not do (on purpose)

- It does not convert to PDF.
- It does not require an account, network, or API.
- It does not silently reorder pages when manga/RTL is checked.
- It does not put images in subfolders inside the zip.
- It does not bundle the `rar` binary.
