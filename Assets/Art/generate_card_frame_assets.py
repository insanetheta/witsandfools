"""Generate UI sprites for the Design-B card frame: top/bottom scrims (vertical alpha
gradients, black) and a subtle gold engraved cartouche to sit behind the rank."""
from PIL import Image, ImageDraw
import math, os
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Textures")
os.makedirs(OUT, exist_ok=True)

def scrim(name, w, h, top_a, bot_a, gamma=1.0):
    img = Image.new("RGBA", (w, h), (0,0,0,0)); px = img.load()
    for y in range(h):
        t = (y/(h-1)) ** gamma
        a = int(round((top_a + (bot_a-top_a)*t) * 255))
        for x in range(w): px[x,y] = (4,3,7,a)
    img.save(os.path.join(OUT, name)); print("wrote", name, img.size)

# top scrim: opaque at top -> transparent toward middle
scrim("card_scrim_top.png", 16, 160, 0.78, 0.0, gamma=1.4)
# bottom drawer scrim: transparent at top -> near-opaque at bottom (darkens art as drawer grows)
scrim("card_scrim_bottom.png", 16, 320, 0.0, 0.98, gamma=0.8)

# gold engraved cartouche (full-opacity strokes on transparent; Unity tints alpha down ~0.42)
W,H = 132, 156
img = Image.new("RGBA",(W,H),(0,0,0,0)); d = ImageDraw.Draw(img)
GOLD=(232,199,102,255); GOLD2=(232,199,102,180)
def shield(cx, top, bot, halfw, col, wdt):
    # a vertical pointed-oval cartouche outline via many short segments
    pts=[]
    for i in range(0,181,6):
        a=math.radians(i)
        x=cx - halfw*math.sin(a)
        y=top + (bot-top)*(i/180.0)
        pts.append((x,y))
    for i in range(180,361,6):
        a=math.radians(i)
        x=cx - halfw*math.sin(a)
        y=top + (bot-top)*((360-i)/180.0)
        pts.append((x,y))
    d.line(pts+[pts[0]], fill=col, width=wdt, joint="curve")
shield(W/2, 8,  H-8,  W/2-10, GOLD, 3)      # outer
shield(W/2, 20, H-20, W/2-26, GOLD2, 2)     # inner echo
# little top & bottom flourishes
d.arc([W/2-22,2,W/2+22,26], 200, 340, fill=GOLD, width=3)
d.arc([W/2-22,H-26,W/2+22,H-2], 20, 160, fill=GOLD, width=3)
img.save(os.path.join(OUT,"card_engrave.png")); print("wrote card_engrave.png", img.size)
