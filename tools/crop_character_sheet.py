#!/usr/bin/env python3
"""Crop the 2x3 InClicker character sheet into named Unity-ready PNG sprites."""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image


SPRITES = [
    ("Assets/Art/Characters/chef_male.png", 0, 0),
    ("Assets/Art/Characters/chef_female.png", 1, 0),
    ("Assets/Art/Characters/grandma_cook.png", 2, 0),
    ("Assets/Art/Customers/customer_office_worker.png", 0, 1),
    ("Assets/Art/Customers/customer_student.png", 1, 1),
    ("Assets/Art/Customers/customer_ojek_driver.png", 2, 1),
]


def background_color(image: Image.Image) -> tuple[int, int, int]:
    width, height = image.size
    samples = [
        image.getpixel((8, 8)),
        image.getpixel((width - 9, 8)),
        image.getpixel((8, height - 9)),
        image.getpixel((width - 9, height - 9)),
    ]
    return tuple(sum(pixel[index] for pixel in samples) // len(samples) for index in range(3))


def remove_background(image: Image.Image, tolerance: int) -> Image.Image:
    image = image.convert("RGBA")
    bg = background_color(image.convert("RGB"))
    pixels = image.load()
    width, height = image.size

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            if abs(r - bg[0]) + abs(g - bg[1]) + abs(b - bg[2]) <= tolerance:
                pixels[x, y] = (r, g, b, 0)
    return image


def crop_to_content(image: Image.Image, padding: int) -> Image.Image:
    alpha = image.getchannel("A")
    bbox = alpha.getbbox()
    if bbox is None:
        return image

    left, top, right, bottom = bbox
    left = max(0, left - padding)
    top = max(0, top - padding)
    right = min(image.width, right + padding)
    bottom = min(image.height, bottom + padding)
    return image.crop((left, top, right, bottom))


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("sheet", type=Path, help="Path to the 3-column by 2-row character sheet PNG.")
    parser.add_argument(
        "--project-root",
        type=Path,
        default=Path(__file__).resolve().parents[1],
        help="Unity project root. Defaults to this script's parent project.",
    )
    parser.add_argument("--tolerance", type=int, default=34, help="Background removal tolerance.")
    parser.add_argument("--padding", type=int, default=24, help="Transparent padding around each sprite.")
    args = parser.parse_args()

    sheet = Image.open(args.sheet).convert("RGBA")
    cell_width = sheet.width // 3
    cell_height = sheet.height // 2

    for relative_path, col, row in SPRITES:
        cell = sheet.crop(
            (
                col * cell_width,
                row * cell_height,
                (col + 1) * cell_width,
                (row + 1) * cell_height,
            )
        )
        sprite = crop_to_content(remove_background(cell, args.tolerance), args.padding)
        output_path = args.project_root / relative_path
        output_path.parent.mkdir(parents=True, exist_ok=True)
        sprite.save(output_path)
        print(output_path)


if __name__ == "__main__":
    main()
