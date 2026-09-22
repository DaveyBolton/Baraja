# Bakes each tutorial page's title + body paragraph directly onto a copy of
# the parchment texture, one PNG per page per language (TutorialPages.cs is
# a fixed, compile-time list - same "static content set, bake once"
# reasoning as the store rows, gem buttons, and enemy/player cards). Sized
# generously per "hard to read on a phone" feedback - the old runtime block
# was only 340px tall in a 1920-tall reference canvas (18%) with a lot of
# empty, unused space below it; this uses a bigger parchment block and a
# clearly larger font instead.
from PIL import Image, ImageDraw, ImageFont
import os

PARCHMENT_PATH = r"C:\Dev\Baraja\App\Assets\Art\Panels\panel_parchment.png"
TITLE_FONT_PATH = r"C:\Dev\Baraja\App\Assets\Fonts\CinzelDecorative-Bold.ttf"
BODY_FONT_PATH = r"C:\Dev\Baraja\App\Assets\Fonts\CrimsonText-SemiBold.ttf"
OUT_DIR = r"C:\Dev\Baraja\design\tutorial_pages_baked"

# On-screen display is 1000x460 (natural parchment aspect is 900x340,
# 2.647:1; this is 2.17:1, ~18% squash - traded for real vertical room since
# the panel had ~950px of completely unused space below it). Baked at 2x
# that (2000x920) so text stays crisp on a high-DPI phone instead of
# upscaling a 1:1-resolution bake.
SCALE = 2
CANVAS_W, CANVAS_H = 1000 * SCALE, 460 * SCALE
TITLE_TOP = 34 * SCALE
TITLE_COLOR = (60, 20, 8)
BODY_TOP = 118 * SCALE
BODY_MAX_W = 860 * SCALE
BODY_MAX_H = 310 * SCALE
BODY_COLOR = (35, 26, 16)

PAGES = [
    dict(id="page0",
         title_es="El Objetivo", title_en="The Goal",
         body_es="Combate contra 9 enemigos y la jefa final, La Catrina, uno a la vez. "
                 "Ganas un combate cuando la HP de todos los enemigos llega a 0 antes que la tuya.",
         body_en="Fight through 9 enemies and the boss, La Catrina, one battle at a time. "
                 "You win a fight by bringing every enemy's HP to 0 before yours reaches 0."),
    dict(id="page1",
         title_es="Energía y Mano", title_en="Energy and Hand",
         body_es="Cada turno recibes 3 de Energía y robas 5 cartas. Cada carta cuesta 1 o 2 de Energía. "
                 "Las cartas sin jugar van al mazo de descarte al terminar el turno; cuando el mazo de robo "
                 "se vacía, el descarte se baraja de nuevo automáticamente.",
         body_en="Each turn you get 3 Energy and draw 5 cards. Every card costs 1 or 2 Energy to play. "
                 "Unplayed cards go to your discard pile at end of turn; when your draw pile runs dry, "
                 "the discard pile reshuffles into a new draw pile automatically."),
    dict(id="page2",
         title_es="Tipos de Carta", title_en="Card Types",
         body_es="Ataque: hace daño, necesita un enemigo objetivo. Habilidad: Bloqueo, robo de cartas u "
                 "otros efectos, casi nunca necesita objetivo. Poder: efecto permanente por el resto del "
                 "combate, se juega una sola vez y sigue activo.",
         body_en="Attack: deals damage, needs a target enemy. Skill: Block, card draw, or other utility, "
                 "almost never needs a target. Power: a permanent effect for the rest of the fight - play "
                 "it once and it keeps working."),
    dict(id="page3",
         title_es="Cómo Apuntar", title_en="Targeting",
         body_es="Toca el retrato de un enemigo para seleccionarlo (se resalta en blanco), luego toca una "
                 "carta de Ataque para golpear a ese objetivo. Las Habilidades y los Poderes no necesitan "
                 "que selecciones nada.",
         body_en="Tap an enemy's portrait to select it (it highlights white), then tap an Attack card to "
                 "hit that target. Skills and Powers don't need anything selected."),
    dict(id="page4",
         title_es="El Bloqueo", title_en="Block",
         body_es="El Bloqueo absorbe daño antes de que llegue a tu HP. Se pierde por completo al empezar "
                 "tu siguiente turno, a menos que un Poder como Diamante de Hielo lo renueve cada turno.",
         body_en="Block absorbs damage before it touches your HP. It resets to 0 at the start of your "
                 "next turn, unless a Power like Ice Diamond keeps refreshing it."),
    dict(id="page5",
         title_es="La Intención Enemiga", title_en="Enemy Intent",
         body_es="El texto naranja bajo cada enemigo dice exactamente qué hará en su próximo turno: "
                 "Ataque, Debilitar, Curarse, Bloquearse o Potenciarse. Nada de lo que hace un enemigo "
                 "es un secreto - juega alrededor de eso.",
         body_en="The orange text under each enemy tells you exactly what it will do on its next turn: "
                 "Attack, Weaken, Heal, Block, or Buff. Nothing an enemy does is hidden - plan around it."),
    dict(id="page6",
         title_es="Efectos de Estado", title_en="Status Effects",
         body_es="Quemadura: hace daño al final del turno de quien la tiene, luego se reduce a la mitad. "
                 "Debilitar: reduce el próximo daño que inflige quien lo tiene. Congelar: reduce a la "
                 "mitad el próximo golpe de quien lo tiene, luego desaparece. Intocable: un ataque falla "
                 "por completo durante un turno (Tumba Sellada).",
         body_en="Burn: deals damage at the end of the burning creature's turn, then halves. Weaken: "
                 "reduces the next damage the weakened creature deals. Freeze: halves the next hit that "
                 "creature lands, then wears off. Untargetable: an attack whiffs completely for one turn "
                 "(Sealed Grave)."),
    dict(id="page7",
         title_es="Estructura del Turno", title_en="Turn Structure",
         body_es="Toca \u201cEnd Turn\u201d cuando termines de jugar cartas o te quedes sin Energía. El enemigo "
                 "resuelve entonces su intención anunciada, se aplican las quemaduras, y el ciclo se "
                 "repite hasta que alguien caiga.",
         body_en="Tap \u201cEnd Turn\u201d when you're done playing cards or run out of Energy. The enemy then "
                 "resolves its telegraphed intent, burn damage ticks, and the cycle repeats until someone "
                 "falls."),
]


def wrap_lines(draw, text, font, max_w):
    words, lines, cur = text.split(" "), [], ""
    for word in words:
        trial = (cur + " " + word).strip()
        if draw.textbbox((0, 0), trial, font=font)[2] > max_w and cur:
            lines.append(cur)
            cur = word
        else:
            cur = trial
    if cur:
        lines.append(cur)
    return lines


def block_height(draw, text, font, max_w, line_h):
    return len(wrap_lines(draw, text, font, max_w)) * line_h


def fit_body_size(draw, bodies, max_w, max_h, start=48 * SCALE):
    size = start
    while size > 12:
        font = ImageFont.truetype(BODY_FONT_PATH, size)
        line_h = int(size * 1.28)
        if all(block_height(draw, b, font, max_w, line_h) <= max_h for b in bodies):
            return size, line_h, font
        size -= 2
    line_h = int(size * 1.28)
    return size, line_h, ImageFont.truetype(BODY_FONT_PATH, size)


def fit_title_size(draw, titles, max_w, start=64 * SCALE):
    size = start
    while size > 20:
        font = ImageFont.truetype(TITLE_FONT_PATH, size)
        if all(draw.textbbox((0, 0), t, font=font)[2] <= max_w for t in titles):
            return size, font
        size -= 2
    return size, ImageFont.truetype(TITLE_FONT_PATH, size)


def main():
    probe = Image.new("RGB", (10, 10))
    pd = ImageDraw.Draw(probe)

    all_titles = [p[f"title_{lang}"] for p in PAGES for lang in ("es", "en")]
    all_bodies = [p[f"body_{lang}"] for p in PAGES for lang in ("es", "en")]

    title_size, title_font = fit_title_size(pd, all_titles, CANVAS_W - 120 * SCALE)
    body_size, line_h, body_font = fit_body_size(pd, all_bodies, BODY_MAX_W, BODY_MAX_H)
    print(f"title={title_size} body={body_size} line_h={line_h}")

    os.makedirs(OUT_DIR, exist_ok=True)
    for page in PAGES:
        for lang in ("es", "en"):
            bg = Image.open(PARCHMENT_PATH).convert("RGB").resize((CANVAS_W, CANVAS_H), Image.LANCZOS)
            d = ImageDraw.Draw(bg)

            title = page[f"title_{lang}"]
            tbbox = d.textbbox((0, 0), title, font=title_font)
            tw = tbbox[2] - tbbox[0]
            d.text(((CANVAS_W - tw) / 2 - tbbox[0], TITLE_TOP - tbbox[1]), title, font=title_font, fill=TITLE_COLOR)

            body = page[f"body_{lang}"]
            lines = wrap_lines(d, body, body_font, BODY_MAX_W)
            block_h = len(lines) * line_h
            by = BODY_TOP + max(0, (BODY_MAX_H - block_h) // 2)  # vertically centered in its box
            for line in lines:
                lbbox = d.textbbox((0, 0), line, font=body_font)
                lw = lbbox[2] - lbbox[0]
                d.text(((CANVAS_W - lw) / 2 - lbbox[0], by - lbbox[1]), line, font=body_font, fill=BODY_COLOR)
                by += line_h

            out_path = os.path.join(OUT_DIR, f"{page['id']}_{lang}.png")
            bg.save(out_path)
    print(f"Baked {len(PAGES) * 2} tutorial page images to {OUT_DIR}")


if __name__ == "__main__":
    main()
