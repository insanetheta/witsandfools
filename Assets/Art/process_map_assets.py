#!/usr/bin/env python3
"""Process generated map assets: resize, crop circles, add alpha."""
from PIL import Image, ImageDraw
import os

SRC = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Map")
DST = SRC  # overwrite in place

ICON_SIZE = 96
BG_SIZE = (1920, 1080)
DOT_SIZE = 16


def make_circular(img, size):
    """Resize to size, make circular with transparent bg."""
    img = img.resize((size, size), Image.LANCZOS)
    img = img.convert("RGBA")

    # Create circular mask
    mask = Image.new("L", (size, size), 0)
    draw = ImageDraw.Draw(mask)
    # Slightly inset circle to avoid edge artifacts
    margin = 1
    draw.ellipse([margin, margin, size - margin - 1, size - margin - 1], fill=255)

    # Also fade out any dark bg pixels near the edge
    pixels = img.load()
    mask_pixels = mask.load()
    for y in range(size):
        for x in range(size):
            r, g, b, a = pixels[x, y]
            m = mask_pixels[x, y]
            if m == 0:
                # Outside circle: fully transparent
                pixels[x, y] = (0, 0, 0, 0)
            else:
                # Inside circle: keep as-is but ensure alpha
                pixels[x, y] = (r, g, b, m)

    return img


def process_icon(name):
    path = os.path.join(SRC, f"{name}.png")
    if not os.path.exists(path):
        print(f"  SKIP {name} (not found)")
        return
    img = Image.open(path)
    result = make_circular(img, ICON_SIZE)
    out = os.path.join(DST, f"{name}.png")
    result.save(out, "PNG")
    print(f"  OK   {name}.png -> {ICON_SIZE}x{ICON_SIZE} circular RGBA")


def process_background():
    path = os.path.join(SRC, "map_parchment_bg.png")
    if not os.path.exists(path):
        print("  SKIP map_parchment_bg (not found)")
        return
    img = Image.open(path)
    img = img.resize(BG_SIZE, Image.LANCZOS)
    out = os.path.join(DST, "map_parchment_bg.png")
    img.save(out, "PNG")
    print(f"  OK   map_parchment_bg.png -> {BG_SIZE[0]}x{BG_SIZE[1]}")


def process_dot():
    path = os.path.join(SRC, "map_path_dot.png")
    if not os.path.exists(path):
        print("  SKIP map_path_dot (not found)")
        return
    img = Image.open(path)
    result = make_circular(img, DOT_SIZE)
    out = os.path.join(DST, "map_path_dot.png")
    result.save(out, "PNG")
    print(f"  OK   map_path_dot.png -> {DOT_SIZE}x{DOT_SIZE} circular RGBA")


if __name__ == "__main__":
    print(f"Processing map assets in {SRC}\n")

    process_background()

    for icon in ["map_node_match", "map_node_elite", "map_node_boss",
                 "map_node_shop", "map_node_rumor", "map_node_rest"]:
        process_icon(icon)

    process_dot()

    # Verify final sizes
    print("\nFinal check:")
    for f in sorted(os.listdir(DST)):
        if f.endswith(".png"):
            img = Image.open(os.path.join(DST, f))
            print(f"  {f:30s} {img.size[0]:5d}x{img.size[1]:<5d} {img.mode}")
