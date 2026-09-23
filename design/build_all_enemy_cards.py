from PIL import Image, ImageDraw, ImageFont, ImageFilter
import os

# Same frame family as build_player_cards.py's committed player frame, glossy
# black instead of gold/silver (same FLUX seed 1979140122, same layout prompt,
# material swapped to glossy jet-black lacquer - see
# ArtEngine/jobs/baraja_card_frame_v16_18_batch.txt, card_frame_v18_gloss_black).
# Enemies always use this black frame now, per Dave - replaces the old v14
# silver. Do not regenerate or otherwise alter this file.
FRAME_PATH = r"C:\Dev\Stephen-AI-Studio\ArtEngine\out\baraja_card_frame_v18_gloss_black\card_frame_v18_gloss_black_zero_margin.png"
ART_DIR = r"C:\Dev\Baraja\design\cards_source_clean"  # bust portraits, black background
SUIT_DIR = r"C:\Dev\Calaverita\App\Assets\Resources\Art\Skulls\approved"  # unused for medallions now, see BADGE_DIR
BADGE_DIR = r"C:\Dev\Baraja\design\medallion_badges"
OUT_DIR = r"C:\Dev\Baraja\design\cards"  # Spanish (default language) - shares the folder with the player deck
OUT_DIR_EN = r"C:\Dev\Baraja\design\cards_en"

FONT_PATH = r"C:\Dev\Baraja\App\Assets\Fonts\CinzelDecorative-Bold.ttf"
DARK_PLATE = (0, 0, 0)

CARD_W, CARD_H = 736, 1040

# ART_TOP/ART_BOTTOM_MAX/TITLE_CENTER_Y carried over from the old v14 silver
# geometry rather than re-measured - this frame's ring top measures at
# y~897 (see MEDALLION_CX/CY below), matching v14's ~897 closely enough that
# the same art/title box still fits the space up to the medallion ring.
# COST_CX/CY and MEDALLION_CX/CY do NOT carry over - each generation places
# its gem/ring a bit differently even at identical scale (the same mistake
# already made and fixed once this session for the silver/red player decks),
# so those are measured fresh on THIS frame file.
ART_TOP = 68
ART_BOTTOM_MAX = 713
ART_MAX_W = 560
ART_OVERSIZE = 1.0

TITLE_CX = 368
TITLE_CENTER_Y = 810
TITLE_SIZE = 50  # fixed - long names wrap to a second line instead of shrinking
TITLE_MAX_W = 520
TITLE_LINE_GAP = 4
TITLE_STROKE = 2
TITLE_FILL = (225, 225, 230, 255)  # light, reads well against the black plate background
TITLE_STROKE_FILL = (10, 10, 12, 255)

# Cost/number gem socket, top-left corner (measured directly on FRAME_PATH).
COST_CX, COST_CY, COST_R = 93, 73, 56
COST_FONT_SIZE = 80  # text pushed bigger than the circle alone would suggest

# Suit-medallion socket, bottom-center (measured directly on FRAME_PATH).
MEDALLION_CX, MEDALLION_CY, SOCKET_R = 366, 963, 50

CARDS = [
    dict(art="calaca_menor.png", name_es="Calaca Menor", name_en="Lesser Calaca", num="1",
         rules="HP 18. Attacks 6 dmg, twice per cycle.",
         suit="silver_death_transparent.png"),
    dict(art="alma_en_pena.png", name_es="Alma en Pena", name_en="Wandering Soul", num="2",
         rules="HP 20. Weakens you (-1 dmg dealt), then attacks 7.",
         suit="rainbow_special_transparent.png"),
    dict(art="perro_xolo.png", name_es="Perro Xolo", name_en="Xolo Dog", num="4",
         rules="HP 22. Attacks 5 twice, then heals self 5.",
         suit="blue_ice_transparent.png"),
    dict(art="guardian_de_ofrenda.png", name_es="Guardian de Ofrenda", name_en="Offering Guardian", num="5",
         rules="HP 30. Blocks 8, then attacks 9 dmg twice.",
         suit="gold_gem_transparent.png"),
    dict(art="doble_calavera.png", name_es="Doble Calavera", name_en="Twin Skulls", num="8",
         rules="HP 15+15. One attacks 5; the other blocks 6 or attacks 5.",
         suit="red_heart_transparent.png"),
    dict(art="la_catrina.png", name_es="La Catrina", name_en="La Catrina", num="B",
         rules="HP 80 (Boss). Strike 12, then Weaken+6, then +4 dmg permanent.",
         suit="special_marigold_transparent.png"),
    dict(art="catrina_menor.png", name_es="Catrina Menor", name_en="Lesser Catrina", num="7",
         rules="HP 28. Attack 6, then self-buff +2 dmg (permanent), then attacks (scales).",
         suit="purple_royalty_transparent.png"),
]


def load_font(size):
    return ImageFont.truetype(FONT_PATH, size)


def wrap_to_width(draw, text, font, max_w, stroke_width=0):
    # Text never shrinks below its assigned size - a line too wide for the
    # budget wraps to another line instead.
    words, lines, cur = text.split(" "), [], ""
    for word in words:
        trial = (cur + " " + word).strip()
        w = draw.textbbox((0, 0), trial, font=font, stroke_width=stroke_width)[2]
        if w > max_w and cur:
            lines.append(cur)
            cur = word
        else:
            cur = trial
    if cur:
        lines.append(cur)
    return lines


def render_text_layer(text, font, fill, stroke_width=0, stroke_fill=None):
    canvas = Image.new("RGBA", (CARD_W, 200), (0, 0, 0, 0))
    d = ImageDraw.Draw(canvas)
    bbox = d.textbbox((10, 10), text, font=font, stroke_width=stroke_width)
    d.text((10, 10), text, font=font, fill=fill, stroke_width=stroke_width, stroke_fill=stroke_fill)
    return canvas.crop(bbox)


def swap_medallion_skull(frame_alpha, suit_filename):
    frame_alpha = frame_alpha.copy()
    d = ImageDraw.Draw(frame_alpha)
    d.ellipse([MEDALLION_CX - SOCKET_R, MEDALLION_CY - SOCKET_R,
               MEDALLION_CX + SOCKET_R, MEDALLION_CY + SOCKET_R],
              fill=(20, 15, 10, 255))
    badge_path = os.path.join(BADGE_DIR, suit_filename.replace("_transparent.png", ".png"))
    badge = Image.open(badge_path).convert("RGBA")
    # already a domed, lit, gradient-shaded render - no synthetic drop
    # shadow needed the way the old flat sprite paste required one.
    socket_d = int(SOCKET_R * 2 * 1.05)
    badge.thumbnail((socket_d, socket_d), Image.LANCZOS)
    sx = MEDALLION_CX - badge.width // 2
    sy = MEDALLION_CY - badge.height // 2
    frame_alpha.alpha_composite(badge, (sx, sy))
    return frame_alpha


def build_card(card, frame_alpha, lang="es"):
    frame_alpha = swap_medallion_skull(frame_alpha, card["suit"])

    bg = Image.new("RGBA", (CARD_W, CARD_H), DARK_PLATE + (255,))

    # --- enemy portrait: square bust art, own black background, fit into
    # the open box above the title, centered both ways within it.
    art = Image.open(os.path.join(ART_DIR, card["art"])).convert("RGB")
    box_w, box_h = ART_MAX_W, ART_BOTTOM_MAX - ART_TOP
    fit_scale = min(box_w / art.width, box_h / art.height) * ART_OVERSIZE
    target_w, target_h = int(art.width * fit_scale), int(art.height * fit_scale)
    art_resized = art.resize((target_w, target_h), Image.LANCZOS)
    paste_x = TITLE_CX - target_w // 2
    paste_y = ART_TOP + (box_h - target_h) // 2
    bg.paste(art_resized, (paste_x, paste_y))

    mock = Image.alpha_composite(bg, frame_alpha)
    draw = ImageDraw.Draw(mock)

    # --- title: fixed size always, wraps to a second line instead of
    # shrinking; each line independently centered on x=368, the whole
    # block (1 or 2 lines) centered on the fixed TITLE_CENTER_Y anchor.
    name = card["name_es"] if lang == "es" else card["name_en"]
    title_font = load_font(TITLE_SIZE)
    title_lines = wrap_to_width(draw, name, title_font, TITLE_MAX_W, stroke_width=TITLE_STROKE)
    title_layers = [render_text_layer(line, title_font, TITLE_FILL,
                                       stroke_width=TITLE_STROKE, stroke_fill=TITLE_STROKE_FILL)
                    for line in title_lines]
    block_h = sum(l.height for l in title_layers) + TITLE_LINE_GAP * (len(title_layers) - 1)
    ly = TITLE_CENTER_Y - block_h // 2
    for layer in title_layers:
        mock.alpha_composite(layer, (TITLE_CX - layer.width // 2, ly))
        ly += layer.height + TITLE_LINE_GAP

    # --- rules text is intentionally NOT baked for enemies: HP, Block, and
    # Intent are all live/dynamic (they change every turn), and CombatUI
    # already draws them at runtime into this exact band, below the title
    # (see BarajaCombatSceneBuilder.MakeEnemyPanelPrefab - TextTop/MedallionTop).
    # Baking the "rules" flavor text here would duplicate and visually
    # collide with that live text, which is what happened before this fix.

    # --- number: solid plate over the frame's baked gem (same "erase then
    # draw the real value" pattern as the suit medallion).
    d = ImageDraw.Draw(mock)
    d.ellipse([COST_CX - COST_R, COST_CY - COST_R, COST_CX + COST_R, COST_CY + COST_R],
              fill=(230, 230, 235, 255), outline=(140, 145, 150, 255), width=4)
    font_cost = load_font(COST_FONT_SIZE)
    cost_text = card["num"]
    bbox = d.textbbox((0, 0), cost_text, font=font_cost)
    cw, ch = bbox[2] - bbox[0], bbox[3] - bbox[1]
    d.text((COST_CX - cw // 2, COST_CY - ch // 2 - bbox[1]), cost_text, font=font_cost, fill=(20, 20, 25, 255))

    out_dir = OUT_DIR if lang == "es" else OUT_DIR_EN
    os.makedirs(out_dir, exist_ok=True)
    out_path = os.path.join(out_dir, card["art"])
    mock.convert("RGB").save(out_path)
    return out_path


def main():
    frame_alpha = Image.open(FRAME_PATH).convert("RGBA")
    assert frame_alpha.size == (CARD_W, CARD_H), f"frame is {frame_alpha.size}, expected {(CARD_W, CARD_H)}"

    for lang in ("es", "en"):
        for card in CARDS:
            out_path = build_card(card, frame_alpha, lang=lang)
            print("saved", out_path)


if __name__ == "__main__":
    main()
