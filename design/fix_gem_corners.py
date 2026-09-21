# The gem button source art (design/gem_buttons_trimmed/*.png) is RGB with
# solid black filling the four corner triangles outside the emerald-cut
# gem shape - invisible on the black overlay panels every button used to
# sit on, but visible as ugly black triangles now that the store rows put
# a Buy button on a light stone slab. Flood-fills from each corner (only
# pixels connected to a corner and near-black turn transparent, so a gem's
# own dark shadowed facets - not connected to the outer corner - are safe)
# and re-saves as RGBA.
from PIL import Image, ImageDraw
import os

SRC_DIR = r"C:\Dev\Baraja\design\gem_buttons_trimmed"
COLORS = ["ruby", "gold", "blue", "purple", "green", "silver", "rainbow"]
THRESH = 36


def punch_corners(path):
    im = Image.open(path).convert("RGBA")
    w, h = im.size
    corners = [(0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)]
    for cx, cy in corners:
        if im.getpixel((cx, cy))[:3] == (0, 0, 0) or sum(im.getpixel((cx, cy))[:3]) < THRESH:
            ImageDraw.floodfill(im, (cx, cy), (0, 0, 0, 0), thresh=THRESH)
    return im


for color in COLORS:
    path = os.path.join(SRC_DIR, f"gem_button_{color}.png")
    fixed = punch_corners(path)
    fixed.save(path)
    n_transparent = sum(1 for px in fixed.getdata() if px[3] == 0)
    print(f"{color}: {n_transparent} transparent px")
