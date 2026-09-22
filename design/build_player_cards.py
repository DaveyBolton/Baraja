from PIL import Image, ImageDraw, ImageFont, ImageFilter
import os

# Frame FINAL, committed 2026-09-22 for both EN and ES decks - do not
# regenerate or otherwise alter this file. Single outer gold filigree
# border, corners pulled back with plain thin bars on the sides, cost gem
# top-left, suit-medallion ring bottom-center, one fully open interior (no
# baked panels - unlike every earlier frame version, so there is nothing
# left to black-panel-scan for; all geometry below is measured directly on
# this exact file instead).
FRAME_PATH = r"C:\Dev\Stephen-AI-Studio\ArtEngine\out\baraja_card_frame_v13\card_frame_v13_zero_margin.png"
ART_DIR = r"C:\Dev\Stephen-AI-Studio\ArtEngine\out\baraja_player_cards"
SUIT_DIR = r"C:\Dev\Calaverita\App\Assets\Resources\Art\Skulls\approved"
OUT_DIR = r"C:\Dev\Baraja\design\cards"  # Spanish (default language)
OUT_DIR_EN = r"C:\Dev\Baraja\design\cards_en"

FONT_PATH = r"C:\Dev\Baraja\App\Assets\Fonts\CinzelDecorative-Bold.ttf"
DARK_PLATE = (0, 0, 0)

CARD_W, CARD_H = 736, 1040

# Measured directly on FRAME_PATH (flood-filled from center, alpha==0):
# interior opening spans x 69-667, y 58-979.
ART_TOP = 68
ART_BOTTOM_MAX = 576  # keeps clear of the title regardless of a card's own title height
ART_MAX_W = 520  # width no longer the binding constraint - height budget (508) is now
ART_OVERSIZE = 1.0  # maxed out per Dave; was 0.85, left ~70px of the box unused

TITLE_CX = 368
TITLE_CENTER_Y = 624  # 10% of card height above the card's exact midline (520), per Dave
TITLE_SIZE = 50  # fixed - long names wrap to a second line instead of shrinking
TITLE_MAX_W = 520
TITLE_LINE_GAP = 4
TITLE_STROKE = 2
TITLE_FILL = (212, 175, 55, 255)
TITLE_STROKE_FILL = (40, 20, 5, 255)

BODY_SIZE = 34
BODY_MAX_W = 560
BODY_GAP_BELOW_TITLE = 65  # widened again per Dave - was 45
BODY_LINE_GAP = 8

# Cost gem socket, top-left corner (measured on FRAME_PATH by isolating the
# gem's purple facet color from the surrounding gold).
COST_CX, COST_CY, COST_R = 83, 58, 28
COST_FONT_SIZE = 30

# Suit-medallion socket, bottom-center (measured on FRAME_PATH: the ring's
# outer decorative rim bulges to about radius 68; SOCKET_R is the flatter
# inner disc, leaving that rim visible around whatever gets pasted there).
MEDALLION_CX, MEDALLION_CY, SOCKET_R = 368, 972, 50

# suit assignment: the 6 recurring suits cover aggression/control/economy/
# utility/power/execute; the specials (Candle, Marigold, Rainbow) each
# anchor their signature card per CARDS.md. Sealed Grave and Copal Smoke
# don't have a skull-shaped medallion icon (they're differently-shaped
# Blocker assets in Calaverita, not skulls) so those two reuse the nearest
# thematic skull (Silver/Death for the grave, Candle for copal-smoke).
CARDS = [
    dict(art="filo_de_hueso.png", name_es="Filo de Hueso", name_en="Bone Blade", cost="1",
         rules="Deal 6 dmg.", suit="red_heart_transparent.png"),
    dict(art="golpe_doble.png", name_es="Golpe Doble", name_en="Double Strike", cost="1",
         rules="Deal 4 dmg twice.", suit="red_heart_transparent.png"),
    dict(art="llama_de_copal.png", name_es="Llama de Copal", name_en="Copal Flame", cost="1",
         rules="Deal 5 dmg, apply 2 Burn.", suit="special_candle_transparent.png"),
    dict(art="bomba_de_cempasuchil.png", name_es="Bomba de Cempas\u00fachil", name_en="Marigold Bomb", cost="2",
         rules="Deal 10 dmg to all enemies.", suit="special_marigold_transparent.png"),
    dict(art="corte_final.png", name_es="Corte Final", name_en="Final Cut", cost="2",
         rules="Deal 8 dmg (16 if enemy below 50% HP).", suit="silver_death_transparent.png"),
    dict(art="escarcha.png", name_es="Escarcha", name_en="Frostbite", cost="1",
         rules="Deal 5 dmg, apply Freeze.", suit="blue_ice_transparent.png"),
    dict(art="escudo_de_hueso.png", name_es="Escudo de Hueso", name_en="Bone Shield", cost="1",
         rules="Gain 5 Block.", suit="blue_ice_transparent.png"),
    dict(art="tumba_sellada.png", name_es="Tumba Sellada", name_en="Sealed Grave", cost="2",
         rules="Gain 10 Block. Untargetable next turn.", suit="silver_death_transparent.png"),
    dict(art="humo_de_copal.png", name_es="Humo de Copal", name_en="Copal Smoke", cost="1",
         rules="Gain 4 Block, draw 1.", suit="special_candle_transparent.png"),
    dict(art="ofrenda_de_oro.png", name_es="Ofrenda de Oro", name_en="Golden Offering", cost="1",
         rules="Draw 2.", suit="green_money_transparent.png"),
    dict(art="bendicion_real.png", name_es="Bendici\u00f3n Real", name_en="Royal Blessing", cost="2",
         rules="Gain 8 Block, remove 1 debuff.", suit="purple_royalty_transparent.png"),
    dict(art="suerte_de_catrina.png", name_es="Suerte de Catrina", name_en="Catrina's Luck", cost="1",
         rules="Random: 8 dmg, 8 Block, or draw 2.", suit="rainbow_special_transparent.png"),
    dict(art="corona_de_espinas.png", name_es="Corona de Espinas", name_en="Crown of Thorns", cost="2",
         rules="Reflect 3 dmg when hit.", suit="purple_royalty_transparent.png"),
    dict(art="vela_eterna.png", name_es="Vela Eterna", name_en="Eternal Candle", cost="1",
         rules="Apply 1 Burn to enemy each turn.", suit="special_candle_transparent.png"),
    dict(art="corazon_de_rubi.png", name_es="Coraz\u00f3n de Rub\u00ed", name_en="Ruby Heart", cost="2",
         rules="+1 energy per turn, rest of combat.", suit="red_heart_transparent.png"),
    dict(art="diamante_de_hielo.png", name_es="Diamante de Hielo", name_en="Ice Diamond", cost="2",
         rules="+3 Block at the start of each turn.", suit="blue_ice_transparent.png"),
    dict(art="reliquia_dorada.png", name_es="Reliquia Dorada", name_en="Golden Relic", cost="2",
         rules="Deal 5 dmg whenever you play a Power.", suit="gold_gem_transparent.png"),
    dict(art="fuego_fatal.png", name_es="Fuego Fatal", name_en="Fatal Fire", cost="1",
         rules="Doubles enemy Burn stacks at end of turn.", suit="silver_death_transparent.png"),
]


def load_font(size):
    return ImageFont.truetype(FONT_PATH, size)


def wrap_to_width(draw, text, font, max_w, stroke_width=0):
    # Text never shrinks below its assigned size - a line too wide for the
    # budget wraps to another line instead (e.g. "Bomba de Cempas\u00fachil"
    # wraps the title to two lines rather than shrinking it).
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
    # Render oversized then crop to the exact ink bbox, so the returned
    # image's own top-left IS the visual top-left of the glyphs - callers
    # can then just center/position this cropped layer directly.
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
    suit_path = os.path.join(SUIT_DIR, suit_filename)
    suit = Image.open(suit_path).convert("RGBA")
    socket_d = int(SOCKET_R * 2 * 0.9)
    suit.thumbnail((socket_d, socket_d), Image.LANCZOS)
    sx = MEDALLION_CX - suit.width // 2
    sy = MEDALLION_CY - suit.height // 2
    shadow_shape = suit.split()[-1].point(lambda a: 140 if a > 10 else 0)
    shadow_layer = Image.new("RGBA", suit.size, (0, 0, 0, 255))
    shadow_layer.putalpha(shadow_shape)
    shadow_layer = shadow_layer.filter(ImageFilter.GaussianBlur(4))
    shadow_full = Image.new("RGBA", frame_alpha.size, (0, 0, 0, 0))
    shadow_full.paste(shadow_layer, (sx + 3, sy + 4), shadow_layer)
    frame_alpha = Image.alpha_composite(frame_alpha, shadow_full)
    frame_alpha.alpha_composite(suit, (sx, sy))
    return frame_alpha


def build_card(card, frame_alpha, lang="es"):
    frame_alpha = swap_medallion_skull(frame_alpha, card["suit"])

    bg = Image.new("RGBA", (CARD_W, CARD_H), DARK_PLATE + (255,))

    # --- card art: square icon-style illustration, own black background,
    # fit into the open box above the title, centered both ways within it.
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
    title_block_bottom = ly - TITLE_LINE_GAP

    # --- rules text: fixed size always, same as the title - a sentence
    # too wide for one line wraps to more lines instead of shrinking. Each
    # line independently centered ("aligned from the center out").
    body_font = load_font(BODY_SIZE)
    sentences = [s.strip() for s in card["rules"].split(".") if s.strip()]
    ly = title_block_bottom + BODY_GAP_BELOW_TITLE
    for sentence in sentences:
        line_text = sentence + "."
        for wrapped in wrap_to_width(draw, line_text, body_font, BODY_MAX_W, stroke_width=TITLE_STROKE):
            line_layer = render_text_layer(wrapped, body_font, TITLE_FILL,
                                            stroke_width=TITLE_STROKE, stroke_fill=TITLE_STROKE_FILL)
            lx = TITLE_CX - line_layer.width // 2
            mock.alpha_composite(line_layer, (lx, ly))
            ly += line_layer.height + BODY_LINE_GAP

    # --- cost: solid plate over the frame's baked gem (same "erase then
    # draw the real value" pattern as the suit medallion), so it stays
    # legible regardless of the gem's own color.
    d = ImageDraw.Draw(mock)
    d.ellipse([COST_CX - COST_R, COST_CY - COST_R, COST_CX + COST_R, COST_CY + COST_R],
              fill=(220, 220, 225, 255), outline=(180, 140, 30, 255), width=4)
    font_cost = load_font(COST_FONT_SIZE)
    cost_text = card["cost"]
    bbox = d.textbbox((0, 0), cost_text, font=font_cost)
    cw, ch = bbox[2] - bbox[0], bbox[3] - bbox[1]
    d.text((COST_CX - cw // 2, COST_CY - ch // 2 - bbox[1]), cost_text, font=font_cost, fill=(20, 15, 5, 255))

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
