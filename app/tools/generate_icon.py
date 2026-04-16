"""Generate strict multi-resolution ICO from icon-app.png.

Uses Pillow to produce PNG frames, then writes the ICO container manually
to guarantee Windows-compatible directory entries for 16/32/48/256 sizes.
"""

from __future__ import annotations

import io
import struct
from pathlib import Path

from PIL import Image

SIZES: list[tuple[int, int]] = [(16, 16), (32, 32), (48, 48), (256, 256)]


def _resolve_paths() -> tuple[Path, Path]:
    repo_root = Path(__file__).resolve().parent.parent
    source = repo_root / "assets" / "branding" / "icon-app.png"
    output = repo_root / "assets" / "branding" / "icon.ico"
    return source, output


def _make_png_frame(img: Image.Image, size: tuple[int, int]) -> bytes:
    resized = img.resize(size, Image.Resampling.LANCZOS)
    buf = io.BytesIO()
    resized.save(buf, format="PNG")
    return buf.getvalue()


def _write_ico(frames: list[bytes], sizes: list[tuple[int, int]], output: Path) -> None:
    num_images = len(frames)
    header = struct.pack("<HHH", 0, 1, num_images)

    dir_size = num_images * 16
    data_offset = 6 + dir_size

    offsets: list[int] = []
    running = data_offset
    for frame in frames:
        offsets.append(running)
        running += len(frame)

    directory = b""
    for size, frame, offset in zip(sizes, frames, offsets):
        w = size[0] if size[0] < 256 else 0
        h = size[1] if size[1] < 256 else 0
        directory += struct.pack("<BBBBHHII", w, h, 0, 0, 1, 32, len(frame), offset)

    with output.open("wb") as fp:
        fp.write(header)
        fp.write(directory)
        for frame in frames:
            fp.write(frame)


def _read_reported_sizes(ico_path: Path) -> list[tuple[int, int]]:
    with ico_path.open("rb") as fp:
        header = fp.read(6)
        _, ico_type, count = struct.unpack("<HHH", header)
        if ico_type != 1:
            raise ValueError(f"Not an ICO file (type={ico_type})")

        directory = fp.read(count * 16)

    reported: list[tuple[int, int]] = []
    for i in range(count):
        entry = directory[i * 16 : (i + 1) * 16]
        w, h = struct.unpack_from("<BB", entry, 0)
        reported.append((w or 256, h or 256))

    return reported


def main() -> int:
    source, output = _resolve_paths()

    print(f"Opening source: {source}")
    if not source.exists():
        raise FileNotFoundError(f"Source PNG not found: {source}")

    img = Image.open(source).convert("RGBA")
    print(f"Source size: {img.size}")

    print(f"Building ICO with sizes: {SIZES}")
    frames = [_make_png_frame(img, size) for size in SIZES]

    _write_ico(frames, SIZES, output)
    print(f"Saved: {output}")

    reported_sizes = _read_reported_sizes(output)
    print(f"ICO frames: {reported_sizes}")

    if reported_sizes != SIZES:
        raise AssertionError(f"Unexpected ICO sizes. expected={SIZES}, got={reported_sizes}")

    print("OK ICO verification passed")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
