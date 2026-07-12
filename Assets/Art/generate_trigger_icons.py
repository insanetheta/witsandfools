#!/usr/bin/env python3
"""Generate trigger-timing badge icons (Set A 'Armory' from docs/design/trigger_timing_icons.html).

White-on-transparent glyphs rendered at 128px for the card ability badge:
  trigger_attack.png  - sword     (ability fires when played as an attack)
  trigger_defend.png  - heater shield (fires when played as a defense)
  trigger_passive.png - sunburst  (always active)

Geometry mirrors the SVG symbols in the design doc (24-unit viewbox).
"""
import os
from PIL import Image, ImageDraw

OUT_DIR = os.path.join(os.path.dirname(__file__), "Icons")
SIZE = 128          # output size
SS = 8              # supersample factor
VIEW = 24.0         # design viewbox
WHITE = (255, 255, 255, 255)


def canvas():
    return Image.new("RGBA", (SIZE * SS, SIZE * SS), (0, 0, 0, 0))


def px(v):
    """viewbox units -> supersampled pixels"""
    return v / VIEW * SIZE * SS


def save(img, name):
    img = img.resize((SIZE, SIZE), Image.LANCZOS)
    path = os.path.join(OUT_DIR, name)
    img.save(path)
    print("wrote", path)


def rounded_rect(draw, x0, y0, x1, y1, r):
    draw.rounded_rectangle([px(x0), px(y0), px(x1), px(y1)], radius=px(r), fill=WHITE)


def circle(draw, cx, cy, r, width=None):
    box = [px(cx - r), px(cy - r), px(cx + r), px(cy + r)]
    if width is None:
        draw.ellipse(box, fill=WHITE)
    else:
        draw.ellipse(box, outline=WHITE, width=int(px(width)))


def line_round(draw, x0, y0, x1, y1, w):
    draw.line([px(x0), px(y0), px(x1), px(y1)], fill=WHITE, width=int(px(w)))
    for (x, y) in ((x0, y0), (x1, y1)):
        circle(draw, x, y, w / 2)


def cubic(p0, p1, p2, p3, n=28):
    pts = []
    for i in range(n + 1):
        t = i / n
        mt = 1 - t
        x = mt**3 * p0[0] + 3 * mt**2 * t * p1[0] + 3 * mt * t**2 * p2[0] + t**3 * p3[0]
        y = mt**3 * p0[1] + 3 * mt**2 * t * p1[1] + 3 * mt * t**2 * p2[1] + t**3 * p3[1]
        pts.append((x, y))
    return pts


def sword():
    img = canvas()
    d = ImageDraw.Draw(img)
    # blade with tip
    d.polygon([(px(12), px(1.8)), (px(14.1), px(6)), (px(14.1), px(13)),
               (px(9.9), px(13)), (px(9.9), px(6))], fill=WHITE)
    # crossguard
    rounded_rect(d, 6.8, 13.0, 17.2, 15.3, 1.1)
    # grip
    d.rectangle([px(10.9), px(15.5), px(13.1), px(18.6)], fill=WHITE)
    # pommel
    circle(d, 12, 20.2, 1.6)
    save(img, "trigger_attack.png")


def shield():
    img = canvas()
    d = ImageDraw.Draw(img)
    pts = []
    pts += cubic((12, 2.4), (14.8, 4.1), (17.9, 4.8), (19.6, 4.9))
    pts += cubic((19.6, 4.9), (19.6, 11.8), (17.3, 17.7), (12, 21.6))
    pts += cubic((12, 21.6), (6.7, 17.7), (4.4, 11.8), (4.4, 4.9))
    pts += cubic((4.4, 4.9), (6.1, 4.8), (9.2, 4.1), (12, 2.4))
    d.polygon([(px(x), px(y)) for x, y in pts], fill=WHITE)
    save(img, "trigger_defend.png")


def sunburst():
    img = canvas()
    d = ImageDraw.Draw(img)
    circle(d, 12, 12, 3.4)
    import math
    for k in range(8):
        a = math.radians(k * 45)
        x0 = 12 + 5.4 * math.cos(a); y0 = 12 + 5.4 * math.sin(a)
        x1 = 12 + 8.0 * math.cos(a); y1 = 12 + 8.0 * math.sin(a)
        line_round(d, x0, y0, x1, y1, 2.0)
    save(img, "trigger_passive.png")


if __name__ == "__main__":
    os.makedirs(OUT_DIR, exist_ok=True)
    sword()
    shield()
    sunburst()
