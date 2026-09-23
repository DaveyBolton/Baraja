from PIL import Image, ImageDraw, ImageFont, ImageFilter
import os

# Same frame family as build_player_cards.py's committed gold frame (seed
# 1979140122), emerald-green instead of gold. Generated clean at effectively zero margin
# already (no crop/scale step needed). Only ART_TOP/TITLE_CENTER_Y reuse
# gold's layout - the cost-gem and medallion sockets are measured
# directly on THIS frame file (each generation places its ring/gem a bit
# differently even at identical scale).
# Do not regenerate or otherwise alter this file.
FRAME_PATH = r"C:\Dev\Stephen-AI-Studio\ArtEngine\out\baraja_card_frame_v20_green\card_frame_v20_green_zero_margin.png"
ART_DIR = r"C:\Dev\Stephen-AI-Studio\ArtEngine\out\baraja_player_cards"
SUIT_DIR = r"C:\Dev\Calaverita\App\Assets\Resources\Art\Skulls\approved"  # unused for medallions now, see BADGE_DIR
BADGE_DIR = r"C:\Dev\Baraja\design\medallion_badges"
OUT_DIR = r"C:\Dev\Baraja\design\cards_green"
OUT_DIR_EN = r"C:\Dev\Baraja\design\cards_green_en"

FONT_PATH = r"C:\Dev\Baraja\App\Assets\Fonts\CinzelDecorative-Bold.ttf"
DARK_PLATE = (0, 0, 0)

CARD_W, CARD_H = 736, 1040

ART_TOP = 68
ART_BOTTOM_MAX = 576
ART_MAX_W = 520
ART_OVERSIZE = 1.0

TITLE_CX = 368
TITLE_CENTER_Y = 624
TITLE_SIZE = 50
TITLE_MAX_W = 520
TITLE_LINE_GAP = 4
TITLE_STROKE = 2
TITLE_FILL = (225, 245, 220, 255)  # pale mint, reads well against black and this frame's green
TITLE_STROKE_FILL = (10, 35, 15, 255)

BODY_SIZE = 34
BODY_MAX_W = 560
BODY_GAP_BELOW_TITLE = 65
BODY_LINE_GAP = 8

COST_CX, COST_CY, COST_R = 97, 75, 56
COST_FONT_SIZE = 80

MEDALLION_CX, MEDALLION_CY, SOCKET_R = 368, 959, 50

CARDS = [
    dict(art="filo_de_hueso.png", name_es="Filo de Hueso", name_en="Bone Blade", cost="1",
         rules="Deal 6 dmg.", rules_es="Inflige 6 de da\u00f1o.", suit="red_heart_transparent.png"),
    dict(art="golpe_doble.png", name_es="Golpe Doble", name_en="Double Strike", cost="1",
         rules="Deal 4 dmg twice.", rules_es="Inflige 4 de da\u00f1o dos veces.", suit="red_heart_transparent.png"),
    dict(art="llama_de_copal.png", name_es="Llama de Copal", name_en="Copal Flame", cost="1",
         rules="Deal 5 dmg, apply 2 Burn.", rules_es="Inflige 5 de da\u00f1o, aplica 2 de Quemadura.",
         suit="special_candle_transparent.png"),
    dict(art="bomba_de_cempasuchil.png", name_es="Bomba de Cempas\u00fachil", name_en="Marigold Bomb", cost="2",
         rules="Deal 10 dmg to all enemies.", rules_es="Inflige 10 de da\u00f1o a todos los enemigos.",
         suit="special_marigold_transparent.png"),
    dict(art="corte_final.png", name_es="Corte Final", name_en="Final Cut", cost="2",
         rules="Deal 8 dmg (16 if enemy below 50% HP).",
         rules_es="Inflige 8 de da\u00f1o (16 si el enemigo tiene menos del 50% de HP).",
         suit="silver_death_transparent.png"),
    dict(art="escarcha.png", name_es="Escarcha", name_en="Frostbite", cost="1",
         rules="Deal 5 dmg, apply Freeze.", rules_es="Inflige 5 de da\u00f1o, aplica Congelaci\u00f3n.",
         suit="blue_ice_transparent.png"),
    dict(art="escudo_de_hueso.png", name_es="Escudo de Hueso", name_en="Bone Shield", cost="1",
         rules="Gain 5 Block.", rules_es="Gana 5 de Bloqueo.", suit="blue_ice_transparent.png"),
    dict(art="tumba_sellada.png", name_es="Tumba Sellada", name_en="Sealed Grave", cost="2",
         rules="Gain 10 Block. Untargetable next turn.",
         rules_es="Gana 10 de Bloqueo. No se te puede atacar el pr\u00f3ximo turno.",
         suit="silver_death_transparent.png"),
    dict(art="humo_de_copal.png", name_es="Humo de Copal", name_en="Copal Smoke", cost="1",
         rules="Gain 4 Block, draw 1.", rules_es="Gana 4 de Bloqueo, roba 1 carta.",
         suit="special_candle_transparent.png"),
    dict(art="ofrenda_de_oro.png", name_es="Ofrenda de Oro", name_en="Golden Offering", cost="1",
         rules="Draw 2.", rules_es="Roba 2 cartas.", suit="green_money_transparent.png"),
    dict(art="bendicion_real.png", name_es="Bendici\u00f3n Real", name_en="Royal Blessing", cost="2",
         rules="Gain 8 Block, remove 1 debuff.", rules_es="Gana 8 de Bloqueo, elimina 1 debilitaci\u00f3n.",
         suit="purple_royalty_transparent.png"),
    dict(art="suerte_de_catrina.png", name_es="Suerte de Catrina", name_en="Catrina's Luck", cost="1",
         rules="Random: 8 dmg, 8 Block, or draw 2.",
         rules_es="Aleatorio: 8 de da\u00f1o, 8 de Bloqueo, o roba 2 cartas.",
         suit="rainbow_special_transparent.png"),
    dict(art="corona_de_espinas.png", name_es="Corona de Espinas", name_en="Crown of Thorns", cost="2",
         rules="Reflect 3 dmg when hit.", rules_es="Refleja 3 de da\u00f1o al recibir un golpe.",
         suit="purple_royalty_transparent.png"),
    dict(art="vela_eterna.png", name_es="Vela Eterna", name_en="Eternal Candle", cost="1",
         rules="Apply 1 Burn to enemy each turn.", rules_es="Aplica 1 de Quemadura al enemigo cada turno.",
         suit="special_candle_transparent.png"),
    dict(art="corazon_de_rubi.png", name_es="Coraz\u00f3n de Rub\u00ed", name_en="Ruby Heart", cost="2",
         rules="+1 energy per turn, rest of combat.", rules_es="+1 de energ\u00eda por turno, el resto del combate.",
         suit="red_heart_transparent.png"),
    dict(art="diamante_de_hielo.png", name_es="Diamante de Hielo", name_en="Ice Diamond", cost="2",
         rules="+3 Block at the start of each turn.", rules_es="+3 de Bloqueo al inicio de cada turno.",
         suit="blue_ice_transparent.png"),
    dict(art="reliquia_dorada.png", name_es="Reliquia Dorada", name_en="Golden Relic", cost="2",
         rules="Deal 5 dmg whenever you play a Power.",
         rules_es="Inflige 5 de da\u00f1o cada vez que juegues un Poder.", suit="gold_gem_transparent.png"),
    dict(art="fuego_fatal.png", name_es="Fuego Fatal", name_en="Fatal Fire", cost="1",
         rules="Doubles enemy Burn stacks at end of turn.",
         rules_es="Duplica las cargas de Quemadura del enemigo al final del turno.",
         suit="silver_death_transparent.png"),
]


def load_font(size):
    return ImageFont.truetype(FONT_PATH, size)


def wrap_to_width(draw, text, font, max_w, stroke_width=0):
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
    socket_d = int(SOCKET_R * 2 * 1.05)
    badge.thumbnail((socket_d, socket_d), Image.LANCZOS)
    sx = MEDALLION_CX - badge.width // 2
    sy = MEDALLION_CY - badge.height // 2
    frame_alpha.alpha_composite(badge, (sx, sy))
    return frame_alpha


def build_card(card, frame_alpha, lang="es"):
    frame_alpha = swap_medallion_skull(frame_alpha, card["suit"])

    bg = Image.new("RGBA", (CARD_W, CARD_H), DARK_PLATE + (255,))

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

    body_font = load_font(BODY_SIZE)
    rules_text = card["rules_es"] if lang == "es" else card["rules"]
    sentences = [s.strip() for s in rules_text.split(".") if s.strip()]
    ly = title_block_bottom + BODY_GAP_BELOW_TITLE
    for sentence in sentences:
        line_text = sentence + "."
        for wrapped in wrap_to_width(draw, line_text, body_font, BODY_MAX_W, stroke_width=TITLE_STROKE):
            line_layer = render_text_layer(wrapped, body_font, TITLE_FILL,
                                            stroke_width=TITLE_STROKE, stroke_fill=TITLE_STROKE_FILL)
            lx = TITLE_CX - line_layer.width // 2
            mock.alpha_composite(line_layer, (lx, ly))
            ly += line_layer.height + BODY_LINE_GAP

    d = ImageDraw.Draw(mock)
    d.ellipse([COST_CX - COST_R, COST_CY - COST_R, COST_CX + COST_R, COST_CY + COST_R],
              fill=(225, 240, 225, 255), outline=(20, 110, 50, 255), width=4)
    font_cost = load_font(COST_FONT_SIZE)
    cost_text = card["cost"]
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
