"""Generate a smooth radial-gradient felt for the match board (no woven texture).
Matches unified_board.html: ellipse 80%x70% at (50%,44%), #2B5C3A center -> felt #21492E -> #173420 edge,
falling into a near-black surround so screen edges read as deep shadow. Output is stretched full-screen
by the board Image, so a square source is fine for a smooth gradient (nothing to distort)."""
from PIL import Image
import math

W = H = 1024
cx, cy = 0.50, 0.44          # focus slightly above center (chandelier light)
rx, ry = 0.80, 0.70          # ellipse radii (fraction of half-extent), per mockup 80%/70%

def lerp(a, b, t): return tuple(int(round(a[i] + (b[i]-a[i])*t)) for i in range(3))

CENTER = (0x2B, 0x5C, 0x3A)  # #2B5C3A
FELT   = (0x21, 0x49, 0x2E)  # #21492E  at ~48%
EDGE   = (0x17, 0x34, 0x20)  # #173420  at 100% of ellipse
SHADOW = (0x10, 0x0D, 0x0A)  # near-black surround beyond the ellipse

img = Image.new("RGB", (W, H))
px = img.load()
for y in range(H):
    for x in range(W):
        # normalized elliptical distance from focus, 0 at center -> 1 at ellipse edge
        dx = (x / W - cx) / rx
        dy = (y / H - cy) / ry
        d = math.sqrt(dx*dx + dy*dy)
        if d <= 0.48:
            c = lerp(CENTER, FELT, d / 0.48)
        elif d <= 1.0:
            c = lerp(FELT, EDGE, (d - 0.48) / 0.52)
        else:
            # fade felt edge into deep shadow over the corners
            t = min((d - 1.0) / 0.6, 1.0)
            c = lerp(EDGE, SHADOW, t)
        px[x, y] = c
img.save("Textures/felt_smooth.png")
print("wrote Assets/Art/Textures/felt_smooth.png", img.size)
