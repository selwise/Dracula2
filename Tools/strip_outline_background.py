#!/usr/bin/env python3
"""Strip exterior white/grey pixels from outlined sprite sheets.

The cleanup is intentionally conservative: it only removes transparent or
light-neutral pixels reachable from the image border. Interior whites and greys
inside a dark outline are preserved.
"""

from __future__ import annotations

import argparse
from collections import deque
from pathlib import Path
from typing import Iterable

from PIL import Image


Pixel = tuple[int, int, int, int]


def is_light_neutral(
    pixel: Pixel,
    *,
    min_lightness: int = 140,
    neutral_tolerance: int = 28,
    alpha_cutoff: int = 0,
) -> bool:
    r, g, b, a = pixel
    if a <= alpha_cutoff:
        return True

    high = max(r, g, b)
    low = min(r, g, b)
    return high >= min_lightness and high - low <= neutral_tolerance


def border_points(width: int, height: int) -> Iterable[tuple[int, int]]:
    for x in range(width):
        yield x, 0
        if height > 1:
            yield x, height - 1

    for y in range(1, max(1, height - 1)):
        yield 0, y
        if width > 1:
            yield width - 1, y


def neighbor_offsets(connect_diagonals: bool) -> tuple[tuple[int, int], ...]:
    if connect_diagonals:
        return (
            (-1, -1),
            (0, -1),
            (1, -1),
            (-1, 0),
            (1, 0),
            (-1, 1),
            (0, 1),
            (1, 1),
        )

    return ((0, -1), (-1, 0), (1, 0), (0, 1))


def strip_border_connected_light_neutral_pixels(
    image: Image.Image,
    *,
    min_lightness: int = 140,
    neutral_tolerance: int = 28,
    alpha_cutoff: int = 0,
    connect_diagonals: bool = False,
    transparent_rgb: tuple[int, int, int] = (0, 0, 0),
) -> Image.Image:
    source = image.convert("RGBA")
    width, height = source.size
    pixels = source.load()
    visited = [[False for _ in range(width)] for _ in range(height)]
    removable = [[False for _ in range(width)] for _ in range(height)]
    queue: deque[tuple[int, int]] = deque()

    def can_remove(x: int, y: int) -> bool:
        return is_light_neutral(
            pixels[x, y],
            min_lightness=min_lightness,
            neutral_tolerance=neutral_tolerance,
            alpha_cutoff=alpha_cutoff,
        )

    for x, y in border_points(width, height):
        if not visited[y][x] and can_remove(x, y):
            visited[y][x] = True
            queue.append((x, y))

    offsets = neighbor_offsets(connect_diagonals)
    while queue:
        x, y = queue.popleft()
        removable[y][x] = True

        for dx, dy in offsets:
            nx = x + dx
            ny = y + dy
            if nx < 0 or nx >= width or ny < 0 or ny >= height:
                continue
            if visited[ny][nx] or not can_remove(nx, ny):
                continue

            visited[ny][nx] = True
            queue.append((nx, ny))

    cleaned = source.copy()
    out = cleaned.load()
    clear_pixel = (*transparent_rgb, 0)
    for y in range(height):
        for x in range(width):
            if removable[y][x]:
                out[x, y] = clear_pixel

    return cleaned


def positive_byte(value: str) -> int:
    parsed = int(value)
    if parsed < 0 or parsed > 255:
        raise argparse.ArgumentTypeError("expected a value from 0 to 255")
    return parsed


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description=(
            "Make border-connected transparent, white, and grey pixels transparent "
            "without touching light pixels protected by a dark outline."
        )
    )
    parser.add_argument("source", type=Path, help="Source PNG path")
    parser.add_argument("destination", type=Path, help="Cleaned PNG output path")
    parser.add_argument(
        "--min-lightness",
        type=positive_byte,
        default=140,
        help="Minimum channel value for removable grey/white pixels. Default: 140",
    )
    parser.add_argument(
        "--neutral-tolerance",
        type=positive_byte,
        default=28,
        help="Maximum RGB channel spread for removable neutral pixels. Default: 28",
    )
    parser.add_argument(
        "--alpha-cutoff",
        type=positive_byte,
        default=0,
        help="Pixels at or below this alpha are treated as traversable. Default: 0",
    )
    parser.add_argument(
        "--connect-diagonals",
        action="store_true",
        help="Use 8-way flood fill instead of safer 4-way fill.",
    )
    parser.add_argument(
        "--transparent-rgb",
        choices=("black", "white"),
        default="black",
        help="RGB stored under alpha 0. Default: black, to avoid white fringes.",
    )
    return parser


def transparent_rgb_from_option(option: str) -> tuple[int, int, int]:
    if option == "black":
        return (0, 0, 0)
    return (255, 255, 255)


def main(argv: list[str] | None = None) -> int:
    args = build_parser().parse_args(argv)
    with Image.open(args.source) as image:
        cleaned = strip_border_connected_light_neutral_pixels(
            image,
            min_lightness=args.min_lightness,
            neutral_tolerance=args.neutral_tolerance,
            alpha_cutoff=args.alpha_cutoff,
            connect_diagonals=args.connect_diagonals,
            transparent_rgb=transparent_rgb_from_option(args.transparent_rgb),
        )

    args.destination.parent.mkdir(parents=True, exist_ok=True)
    cleaned.save(args.destination)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
