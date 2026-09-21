from PIL import Image, ImageDraw, ImageFont, ImageFilter
import os

FRAME_PATH = r"C:\Dev\Baraja\design\card_frame_v4_medallion_matched.png"
GEOM_REF_PATH = r"C:\Dev\Baraja\design\card_frame_v4_nameplate2x.png"
ART_DIR = r"C:\Dev\Stephen-AI-Studio\ArtEngine\out\baraja_player_cards"
SUIT_DIR = r"C:\Dev\Calaverita\App\Assets\Resources\Art\Skulls\approved"
OUT_DIR = r"C:\Dev\Baraja\design\cards"  # Spanish (default language)
OUT_DIR_EN = r"C:\Dev\Baraja\design\cards_en"

BLACK_THRESH = 90
DARK_PLATE = (0, 0, 0)

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
    dict(art="bomba_de_cempasuchil.png", name_es="Bomba de Cempasuchil", name_en="Marigold Bomb", cost="2",
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
    dict(art="bendicion_real.png", name_es="Bendicion Real", name_en="Royal Blessing", cost="2",
         rules="Gain 8 Block, remove 1 debuff.", suit="purple_royalty_transparent.png"),
    dict(art="suerte_de_catrina.png", name_es="Suerte de Catrina", name_en="Catrina's Luck", cost="1",
         rules="Random: 8 dmg, 8 Block, or draw 2.", suit="rainbow_special_transparent.png"),
    dict(art="corona_de_espinas.png", name_es="Corona de Espinas", name_en="Crown of Thorns", cost="2",
         rules="Reflect 3 dmg when hit.", suit="purple_royalty_transparent.png"),
    dict(art="vela_eterna.png", name_es="Vela Eterna", name_en="Eternal Candle", cost="1",
         rules="Apply 1 Burn to enemy each turn.", suit="special_candle_transparent.png"),
    dict(art="corazon_de_rubi.png", name_es="Corazon de Rubi", name_en="Ruby Heart", cost="2",
         rules="+1 energy per turn, rest of combat.", suit="red_heart_transparent.png"),
    dict(art="diamante_de_hielo.png", name_es="Diamante de Hielo", name_en="Ice Diamond", cost="2",
         rules="+3 Block at the start of each turn.", suit="blue_ice_transparent.png"),
    dict(art="reliquia_dorada.png", name_es="Reliquia Dorada", name_en="Golden Relic", cost="2",
         rules="Deal 5 dmg whenever you play a Power.", suit="gold_gem_transparent.png"),
    dict(art="fuego_fatal.png", name_es="Fuego Fatal", name_en="Fatal Fire", cost="1",
         rules="Doubles enemy Burn stacks at end of turn.", suit="silver_death_transparent.png"),
]


def find_black_panels(img, min_h):
    w, h = img.size
    cx = w // 2
    px = img.convert("RGB").load()
    def is_black(x, y):
        r, g, b = px[x, y]
        return r < BLACK_THRESH and g < BLACK_THRESH and b < BLACK_THRESH
    bands, in_band, start = [], False, 0
    for y in range(h):
        black = is_black(cx, y)
        if black and not in_band:
            in_band, start = True, y
        elif not black and in_band:
            in_band = False
            if y - start > min_h:
                bands.append((start, y))
    if in_band and h - start > min_h:
        bands.append((start, h))
    return [b for b in bands if b[0] > 5 and b[1] < h - 5]


def panel_x_bounds(img, y):
    w, _ = img.size
    px = img.convert("RGB").load()
    def is_black(x):
        r, g, b = px[x, y]
        return r < BLACK_THRESH and g < BLACK_THRESH and b < BLACK_THRESH
    best, run_start = (0, 0, 0), None
    for x in range(w):
        if is_black(x):
            if run_start is None:
                run_start = x
        else:
            if run_start is not None:
                length = x - run_start
                if length > best[0]:
                    best = (length, run_start, x)
                run_start = None
    if run_start is not None:
        length = w - run_start
        if length > best[0]:
            best = (length, run_start, w)
    return best[1], best[2] - 1


def load_font(size):
    for p in (r"C:\Windows\Fonts\georgiab.ttf", r"C:\Windows\Fonts\segoeuib.ttf"):
        if os.path.exists(p):
            return ImageFont.truetype(p, size)
    return ImageFont.load_default()


def make_alpha_frame(frame_rgb, eligible_rects):
    frame = frame_rgb.convert("RGBA")
    px = frame.load()
    w, h = frame.size
    mask = Image.new("1", (w, h), 0)
    md = ImageDraw.Draw(mask)
    for (x0, y0, x1, y1) in eligible_rects:
        md.rectangle([x0, y0, x1, y1], fill=1)
    mpx = mask.load()
    for y in range(h):
        for x in range(w):
            if not mpx[x, y]:
                continue
            r, g, b, a = px[x, y]
            if r < BLACK_THRESH and g < BLACK_THRESH and b < BLACK_THRESH:
                px[x, y] = (r, g, b, 0)
    return frame


def geometry():
    geom_ref = Image.open(GEOM_REF_PATH).convert("RGB")
    bands = find_black_panels(geom_ref, min_h=100)
    if len(bands) >= 3:
        art_band, name_band, text_band = bands[0], bands[1], bands[2]
    else:
        art_band, text_band = bands[0], bands[1]
        name_band = (art_band[1], text_band[0])
    name_gap_full = name_band
    name_band = (693, 729)
    ax, arx = panel_x_bounds(geom_ref, (art_band[0] + art_band[1]) // 2)
    tx, trx = panel_x_bounds(geom_ref, (text_band[0] + text_band[1]) // 2)
    nx, nrx = panel_x_bounds(geom_ref, (name_band[0] + name_band[1]) // 2)
    art_band = (95, art_band[1])
    return dict(ax=ax, arx=arx, tx=tx, trx=trx, nx=nx, nrx=nrx,
                art_band=art_band, name_band=name_band, text_band=text_band,
                name_gap_full=name_gap_full)


MEDALLION_CX, MEDALLION_CY, SOCKET_R = 368, 960, 60


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


def build_card(card, geo, frame_alpha, w, h, lang="es"):
    frame_alpha = swap_medallion_skull(frame_alpha, card["suit"])
    ax, arx = geo["ax"], geo["arx"]
    tx, trx = geo["tx"], geo["trx"]
    nx, nrx = geo["nx"], geo["nrx"]
    art_band, name_band, text_band = geo["art_band"], geo["name_band"], geo["text_band"]

    bg = Image.new("RGB", (w, h), DARK_PLATE)
    art = Image.open(os.path.join(ART_DIR, card["art"])).convert("RGB")
    win_cx = (ax + arx) // 2
    win_top, win_bottom = art_band[0], art_band[1]
    win_h = win_bottom - win_top
    oversize = 0.75  # icon art reads better a bit smaller than a bust portrait
    target_h = int(win_h * oversize)
    scale = target_h / art.height
    target_w = int(art.width * scale)
    art_resized = art.resize((target_w, target_h), Image.LANCZOS)
    paste_x = win_cx - target_w // 2
    paste_y = win_top + (win_h - target_h) // 2
    bg.paste(art_resized, (paste_x, paste_y))

    plate = Image.new("RGBA", (nrx - nx, name_band[1] - name_band[0]), DARK_PLATE + (255,))
    bg_rgba = bg.convert("RGBA")
    bg_rgba.alpha_composite(plate, (nx, name_band[0]))
    bg = bg_rgba.convert("RGB")

    mock = bg.convert("RGBA")
    mock.alpha_composite(frame_alpha)
    mock = mock.convert("RGB")
    d = ImageDraw.Draw(mock)

    font_name = load_font(30)
    font_text = load_font(23)
    font_cost = load_font(26)

    name = card["name_es"] if lang == "es" else card["name_en"]
    ncx = (nx + nrx) // 2
    ncy = (name_band[0] + name_band[1]) // 2
    d.text((ncx, ncy), name, font=font_name, fill=(240, 225, 200), anchor="mm")

    rules = card["rules"]
    pad = 18
    tzx, tzy = tx + pad, text_band[0] + pad
    max_w = (trx - tx) - 2 * pad
    words, lines, cur = rules.split(" "), [], ""
    for word in words:
        trial = (cur + " " + word).strip()
        if d.textbbox((0, 0), trial, font=font_text)[2] > max_w and cur:
            lines.append(cur)
            cur = word
        else:
            cur = trial
    if cur:
        lines.append(cur)
    ly = tzy
    for line in lines:
        d.text((tzx, ly), line, font=font_text, fill=(225, 225, 230))
        ly += 29

    cost_cx, cost_cy = 165, 131
    gem_r = 40
    d.ellipse([cost_cx - gem_r, cost_cy - gem_r, cost_cx + gem_r, cost_cy + gem_r],
              fill=(220, 220, 225), outline=(180, 140, 30), width=5)
    cost_text = card["cost"]
    bbox = d.textbbox((0, 0), cost_text, font=font_cost)
    cw, ch = bbox[2] - bbox[0], bbox[3] - bbox[1]
    d.text((cost_cx - cw // 2, cost_cy - ch // 2 - bbox[1]), cost_text, font=font_cost, fill=(20, 15, 5))

    out_dir = OUT_DIR if lang == "es" else OUT_DIR_EN
    os.makedirs(out_dir, exist_ok=True)
    out_path = os.path.join(out_dir, card["art"])
    mock.save(out_path)
    return out_path


def main():
    frame_rgb = Image.open(FRAME_PATH).convert("RGB")
    w, h = frame_rgb.size
    geo = geometry()
    eligible_rects = [
        (geo["ax"], geo["art_band"][0], 600, geo["art_band"][1]),
        (geo["nx"], geo["name_gap_full"][0], geo["nrx"], geo["name_gap_full"][1]),
        (geo["tx"], geo["text_band"][0], geo["trx"], geo["text_band"][1]),
    ]
    frame_alpha = make_alpha_frame(frame_rgb, eligible_rects)

    for lang in ("es", "en"):
        for card in CARDS:
            out_path = build_card(card, geo, frame_alpha, w, h, lang=lang)
            print("saved", out_path)


if __name__ == "__main__":
    main()
