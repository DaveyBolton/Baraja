"""Combat-panel enemy cards: the EXACT same pipeline as build_all_enemy_cards.py
(same geometry, same font, same masking, same medallion swap) - not a
reinvented layout. Two differences only: the silver frame instead of gold,
and blank rules text, since HP/Block/Intent are live numbers overlaid by
Unity at runtime, not static flavor text.
"""
import sys
sys.path.insert(0, r"C:\Dev\Baraja\design")
import build_all_enemy_cards as m
from PIL import Image
import os

SILVER_FRAME_PATH = r"C:\Dev\Baraja\design\enemy_frame_silver_full.png"
OUT_DIR = r"C:\Dev\Baraja\design\enemy_combat_cards"
OUT_DIR_EN = r"C:\Dev\Baraja\design\enemy_combat_cards_en"

frame_rgb = Image.open(SILVER_FRAME_PATH).convert("RGB")
w, h = frame_rgb.size
geo = m.geometry()

m.BLACK_THRESH = 115
eligible_rects = [
    (geo["ax"], geo["art_band"][0], 600, geo["art_band"][1]),
    (geo["nx"], geo["name_gap_full"][0], geo["nrx"], geo["name_gap_full"][1]),
    (geo["tx"], geo["text_band"][0], geo["trx"], geo["text_band"][1]),
]
frame_alpha = m.make_alpha_frame(frame_rgb, eligible_rects)

# build_card() writes to the imported module's OWN OUT_DIR/OUT_DIR_EN globals
# (design/cards, design/cards_en - the gold deck-view cards). Override them
# here so this run writes silver combat-panel cards instead of clobbering
# those.
m.OUT_DIR = OUT_DIR
m.OUT_DIR_EN = OUT_DIR_EN

for lang in ("es", "en"):
    out_dir = OUT_DIR if lang == "es" else OUT_DIR_EN
    os.makedirs(out_dir, exist_ok=True)
    for card in m.CARDS:
        blank_card = dict(card)
        blank_card["rules"] = ""  # left blank for the live HP/Block/Intent overlay
        out_path = m.build_card(blank_card, geo, frame_alpha, w, h, lang=lang)
        # build_card() writes to OUT_DIR/OUT_DIR_EN from the imported module -
        # override the module's own dirs so this run doesn't clobber the
        # gold deck-view cards it normally produces.
        print("saved", out_path)
