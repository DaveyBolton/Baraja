# Bakes each button's label directly onto its gem texture (one PNG per
# unique button), so the label scales as part of the same image instead of
# being a separate runtime Text/Outline layered on top. On small screens the
# whole button is then one proportional unit - no separate anchor math for
# text-vs-gem that can drift apart at different sizes, which is what caused
# the Buy/price overlap and font-vs-button-size bugs this project kept
# hitting with the old layered approach.
from PIL import Image, ImageDraw, ImageFont
import os

GEM_DIR = r"C:\Dev\Baraja\design\gem_buttons_trimmed"
FONT_PATH = r"C:\Dev\Baraja\App\Assets\Fonts\CinzelDecorative-Bold.ttf"
OUT_DIR = r"C:\Dev\Baraja\design\gem_buttons_baked"

# (output name, gem color, label)
BUTTONS = [
    ("play_gold", "gold", "Play"),
    ("howtoplay_blue", "blue", "How to Play"),
    ("options_purple", "purple", "Options"),
    ("store_green", "green", "Store"),
    ("espanol_rainbow", "rainbow", "Español"),
    ("english_rainbow", "rainbow", "English"),
    ("close_silver", "silver", "Close"),
    ("back_silver", "silver", "< Back"),
    ("next_blue", "blue", "Next >"),
    ("buy_silver", "silver", "Buy"),
    ("comprar_silver", "silver", "Comprar"),
    ("owned_silver", "silver", "Owned"),
    ("comprado_silver", "silver", "Comprado"),
    ("endturn_ruby", "ruby", "End Turn"),
    ("gotit_ruby", "ruby", "Got it"),
]

CANVAS_W, CANVAS_H = 700, 315
SAFE_W = int(CANVAS_W * 0.80)   # clear of the faceted bevel edges
SAFE_H = int(CANVAS_H * 0.55)
FILL = (255, 255, 255)
STROKE_FILL = (0, 0, 0)


def text_size(draw, text, font, stroke_width):
    bbox = draw.textbbox((0, 0), text, font=font, stroke_width=stroke_width)
    return bbox[2] - bbox[0], bbox[3] - bbox[1], bbox


def fit_font_size(draw, labels):
    """Largest single size that fits every label within the safe box,
    same size everywhere so every gem button reads consistently."""
    size = 120
    while size > 10:
        font = ImageFont.truetype(FONT_PATH, size)
        stroke_width = max(2, size // 16)
        widest_ok = all(
            text_size(draw, label, font, stroke_width)[0] <= SAFE_W
            and text_size(draw, label, font, stroke_width)[1] <= SAFE_H
            for label in labels
        )
        if widest_ok:
            return size, stroke_width
        size -= 2
    return size, max(2, size // 16)


def bake(name, color, label, font, stroke_width):
    src = Image.open(os.path.join(GEM_DIR, f"gem_button_{color}.png")).convert("RGB")
    assert src.size == (CANVAS_W, CANVAS_H), f"{name}: unexpected gem size {src.size}"
    draw = ImageDraw.Draw(src)
    w, h, bbox = text_size(draw, label, font, stroke_width)
    cx, cy = CANVAS_W / 2, CANVAS_H / 2
    pos = (cx - bbox[0] - w / 2, cy - bbox[1] - h / 2)
    draw.text(pos, label, font=font, fill=FILL, stroke_width=stroke_width, stroke_fill=STROKE_FILL)
    os.makedirs(OUT_DIR, exist_ok=True)
    out_path = os.path.join(OUT_DIR, f"{name}.png")
    src.save(out_path)
    return out_path, w, h


def main():
    probe = Image.new("RGB", (10, 10))
    draw = ImageDraw.Draw(probe)
    labels = [label for _, _, label in BUTTONS]
    size, stroke_width = fit_font_size(draw, labels)
    print(f"Using font size {size}, stroke width {stroke_width} (fits all {len(labels)} labels)")
    for name, color, label in BUTTONS:
        font = ImageFont.truetype(FONT_PATH, size)
        out_path, w, h = bake(name, color, label, font, stroke_width)
        print(f"{name:20s} '{label}': {w}x{h}px text on {CANVAS_W}x{CANVAS_H} -> {out_path}")


if __name__ == "__main__":
    main()
