#!/usr/bin/env python3
"""Crop generated grid sheets into named Unity-ready PNG assets."""

from __future__ import annotations

import argparse
from collections import deque
from pathlib import Path

from PIL import Image


PRESETS = {
    "food": {
        "cols": 5,
        "rows": 2,
        "padding": 18,
        "tolerance": 42,
        "assets": [
            ("Assets/Art/FoodIcons/nasi_galau_level_5.png", 0, 0),
            ("Assets/Art/FoodIcons/mie_patah_hati.png", 1, 0),
            ("Assets/Art/FoodIcons/tempe_bucin_original.png", 2, 0),
            ("Assets/Art/FoodIcons/cap_cay_overthinking.png", 3, 0),
            ("Assets/Art/FoodIcons/soto_eksistensial.png", 4, 0),
            ("Assets/Art/FoodIcons/bakso_quarter_life_crisis.png", 0, 1),
            ("Assets/Art/FoodIcons/gado_gado_red_flag.png", 1, 1),
            ("Assets/Art/FoodIcons/rendang_privilege.png", 2, 1),
            ("Assets/Art/FoodIcons/sop_buntut_nostalgia.png", 3, 1),
            ("Assets/Art/FoodIcons/nasi_padang_world_edition.png", 4, 1),
        ],
    },
    "customer": {
        "cols": 4,
        "rows": 2,
        "padding": 24,
        "tolerance": 34,
        "assets": [
            ("Assets/Art/Customers/customer_office_worker_alt.png", 0, 0),
            ("Assets/Art/Customers/customer_student_alt.png", 1, 0),
            ("Assets/Art/Customers/customer_ojek_driver_alt.png", 2, 0),
            ("Assets/Art/Customers/customer_ibu_komplek.png", 3, 0),
            ("Assets/Art/Customers/customer_food_reviewer.png", 0, 1),
            ("Assets/Art/Customers/customer_tourist.png", 1, 1),
            ("Assets/Art/Customers/customer_elder_regular.png", 2, 1),
            ("Assets/Art/Customers/customer_couple_table.png", 3, 1),
        ],
    },
}


def color_distance(left: tuple[int, int, int, int], right: tuple[int, int, int, int]) -> int:
    return abs(left[0] - right[0]) + abs(left[1] - right[1]) + abs(left[2] - right[2])


def edge_palette(image: Image.Image) -> list[tuple[int, int, int, int]]:
    width, height = image.size
    points = [
        (1, 1),
        (width - 2, 1),
        (1, height - 2),
        (width - 2, height - 2),
        (width // 2, 1),
        (width // 2, height - 2),
        (1, height // 2),
        (width - 2, height // 2),
    ]
    return [image.getpixel(point) for point in points]


def looks_like_background(pixel: tuple[int, int, int, int], palette: list[tuple[int, int, int, int]], tolerance: int) -> bool:
    return any(color_distance(pixel, color) <= tolerance for color in palette)


def flood_clear_background(image: Image.Image, tolerance: int) -> Image.Image:
    image = image.convert("RGBA")
    width, height = image.size
    pixels = image.load()
    palette = edge_palette(image)
    seen: set[tuple[int, int]] = set()
    queue: deque[tuple[int, int]] = deque()

    for x in range(width):
        queue.append((x, 0))
        queue.append((x, height - 1))
    for y in range(height):
        queue.append((0, y))
        queue.append((width - 1, y))

    while queue:
        x, y = queue.popleft()
        if (x, y) in seen or x < 0 or y < 0 or x >= width or y >= height:
            continue
        seen.add((x, y))

        pixel = pixels[x, y]
        if not looks_like_background(pixel, palette, tolerance):
            continue

        pixels[x, y] = (pixel[0], pixel[1], pixel[2], 0)
        queue.append((x + 1, y))
        queue.append((x - 1, y))
        queue.append((x, y + 1))
        queue.append((x, y - 1))

    return image


def crop_to_content(image: Image.Image, padding: int) -> Image.Image:
    bbox = image.getchannel("A").getbbox()
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
    parser.add_argument("preset", choices=sorted(PRESETS))
    parser.add_argument("sheet", type=Path)
    parser.add_argument(
        "--project-root",
        type=Path,
        default=Path(__file__).resolve().parents[1],
        help="Unity project root. Defaults to this script's parent project.",
    )
    parser.add_argument("--tolerance", type=int, default=None)
    parser.add_argument("--padding", type=int, default=None)
    args = parser.parse_args()

    preset = PRESETS[args.preset]
    cols = preset["cols"]
    rows = preset["rows"]
    tolerance = args.tolerance if args.tolerance is not None else preset["tolerance"]
    padding = args.padding if args.padding is not None else preset["padding"]

    sheet = Image.open(args.sheet).convert("RGBA")
    cell_width = sheet.width // cols
    cell_height = sheet.height // rows

    for relative_path, col, row in preset["assets"]:
        cell = sheet.crop(
            (
                col * cell_width,
                row * cell_height,
                (col + 1) * cell_width,
                (row + 1) * cell_height,
            )
        )
        sprite = crop_to_content(flood_clear_background(cell, tolerance), padding)
        output_path = args.project_root / relative_path
        output_path.parent.mkdir(parents=True, exist_ok=True)
        sprite.save(output_path)
        print(output_path)


if __name__ == "__main__":
    main()
