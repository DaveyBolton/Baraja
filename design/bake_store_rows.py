# Bakes each store item's Name/Description/Price directly onto its own copy
# of the stone slab, one PNG per item per language (fixed catalog in
# StoreDatabase.cs - same "static content set, bake once" reasoning already
# used for enemy/player cards and the gem buttons). Sized generously per
# "hard to read on a phone" feedback on the old runtime-Text sizes - err
# larger, since a screenshot on a monitor reads bigger than the same pixels
# on a small handheld screen.
from PIL import Image, ImageDraw, ImageFont
import os

STONE_PATH = r"C:\Dev\Baraja\App\Assets\Art\Panels\panel_stone.png"
TITLE_FONT_PATH = r"C:\Dev\Baraja\App\Assets\Fonts\CinzelDecorative-Bold.ttf"
BODY_FONT_PATH = r"C:\Dev\Baraja\App\Assets\Fonts\CrimsonText-SemiBold.ttf"
OUT_DIR = r"C:\Dev\Baraja\design\store_rows_baked"

# Baked at 2x the on-screen display size (960x229) so the text stays crisp
# on a high-DPI phone screen instead of upscaling a 1:1-resolution bake.
SCALE = 2
CANVAS_W, CANVAS_H = 960 * SCALE, 229 * SCALE
DARK_TEXT = (30, 28, 28)
DESC_TEXT = (60, 56, 56)
PRICE_TEXT = (140, 97, 13)

# Right column (x >= 680) is reserved for the runtime Buy button, which sits
# on top of this baked image at runtime, vertically centered right side -
# Price bakes into the strip above it instead of colliding with it.
LEFT_MARGIN = 50 * SCALE
LEFT_MAX_W = 610 * SCALE   # name/desc column width
PRICE_RIGHT = 910 * SCALE  # matches the Buy button's own right edge (anchoredPosition -50)
PRICE_TOP = 20 * SCALE     # clear of the stone's carved top border (measured: $ ascender clipped at 8)
PRICE_MAX_H = 38 * SCALE   # still clears the Buy button's top edge (~63px down) with margin

ITEMS = [
    dict(id="remove_ads", price_es="$2.99", price_en="$2.99",
         name_es="Quitar Anuncios", name_en="Remove Ads",
         desc_es="Elimina todos los anuncios para siempre.",
         desc_en="Turns off all ads, forever."),
    dict(id="petals_small", price_es="$0.99", price_en="$0.99",
         name_es="Bolsa de Pétalos Pequeña", name_en="Small Petal Pouch",
         desc_es="100 Pétalos de Cempasúchil.", desc_en="100 Marigold Petals."),
    dict(id="petals_medium", price_es="$4.99", price_en="$4.99",
         name_es="Bolsa de Pétalos Mediana", name_en="Medium Petal Pouch",
         desc_es="600 Pétalos de Cempasúchil.", desc_en="600 Marigold Petals."),
    dict(id="petals_large", price_es="$9.99", price_en="$9.99",
         name_es="Bolsa de Pétalos Grande", name_en="Large Petal Pouch",
         desc_es="1400 Pétalos de Cempasúchil.", desc_en="1400 Marigold Petals."),
    dict(id="revive_token", price_es="$1.99", price_en="$1.99",
         name_es="Bendición de Catrina", name_en="Catrina's Blessing",
         desc_es="Continúa una vez tras caer en combate.",
         desc_en="Continue once after falling in combat."),
    dict(id="cardback_rainbow", price_es="150 Pétalos", price_en="150 Petals",
         name_es="Reverso Catrina Arcoíris", name_en="Rainbow Catrina Back",
         desc_es="Reverso de carta cosmético.", desc_en="Cosmetic card back."),
    dict(id="cardback_gold", price_es="150 Pétalos", price_en="150 Petals",
         name_es="Reverso Reliquia Dorada", name_en="Golden Relic Back",
         desc_es="Reverso de carta cosmético.", desc_en="Cosmetic card back."),
    dict(id="cardback_silver", price_es="150 Pétalos", price_en="150 Petals",
         name_es="Reverso Plata Mortal", name_en="Silver Death Back",
         desc_es="Reverso de carta cosmético.", desc_en="Cosmetic card back."),
]


def text_wh(draw, text, font):
    bbox = draw.textbbox((0, 0), text, font=font)
    return bbox[2] - bbox[0], bbox[3] - bbox[1], bbox


def fit_size(draw, texts, font_path, max_w, max_h, start=60):
    size = start
    while size > 10:
        font = ImageFont.truetype(font_path, size)
        if all(text_wh(draw, t, font)[0] <= max_w and text_wh(draw, t, font)[1] <= max_h for t in texts):
            return size, font
        size -= 2
    return size, ImageFont.truetype(font_path, size)


def main():
    probe = Image.new("RGB", (10, 10))
    pd = ImageDraw.Draw(probe)

    all_names = [it[f"name_{lang}"] for it in ITEMS for lang in ("es", "en")]
    all_descs = [it[f"desc_{lang}"] for it in ITEMS for lang in ("es", "en")]
    all_prices = [it[f"price_{lang}"] for it in ITEMS for lang in ("es", "en")]

    name_size, name_font = fit_size(pd, all_names, TITLE_FONT_PATH, LEFT_MAX_W, 50 * SCALE, start=46 * SCALE)
    desc_size, desc_font = fit_size(pd, all_descs, BODY_FONT_PATH, LEFT_MAX_W, 40 * SCALE, start=38 * SCALE)
    price_size, price_font = fit_size(pd, all_prices, TITLE_FONT_PATH, 260 * SCALE, PRICE_MAX_H, start=40 * SCALE)
    print(f"name={name_size} desc={desc_size} price={price_size}")

    os.makedirs(OUT_DIR, exist_ok=True)
    for item in ITEMS:
        for lang in ("es", "en"):
            bg = Image.open(STONE_PATH).convert("RGB").resize((CANVAS_W, CANVAS_H), Image.LANCZOS)
            d = ImageDraw.Draw(bg)

            name = item[f"name_{lang}"]
            desc = item[f"desc_{lang}"]
            price = item[f"price_{lang}"]

            # Name+Desc are vertically centered as a block on the row's own
            # center (matching the Buy button's centering) instead of
            # pinned near the top - pinning left ~40% of the slab as dead
            # empty space below the text, which read as unbalanced.
            gap = 18 * SCALE
            nw, nh, nbbox = text_wh(d, name, name_font)
            dw, dh, dbbox = text_wh(d, desc, desc_font)
            block_h = nh + gap + dh
            name_y = (CANVAS_H - block_h) / 2
            desc_y = name_y + nh + gap
            d.text((LEFT_MARGIN - nbbox[0], name_y - nbbox[1]), name, font=name_font, fill=DARK_TEXT)
            d.text((LEFT_MARGIN - dbbox[0], desc_y - dbbox[1]), desc, font=desc_font, fill=DESC_TEXT)

            pw, ph, pbbox = text_wh(d, price, price_font)
            d.text((PRICE_RIGHT - pw - pbbox[0], PRICE_TOP - pbbox[1]), price, font=price_font, fill=PRICE_TEXT)

            out_path = os.path.join(OUT_DIR, f"{item['id']}_{lang}.png")
            bg.save(out_path)
    print(f"Baked {len(ITEMS) * 2} store row images to {OUT_DIR}")


if __name__ == "__main__":
    main()
