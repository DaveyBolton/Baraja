from PIL import Image
import numpy as np
import os

SRC_DIR = r"C:\Dev\Stephen-AI-Studio\ArtEngine\out\baraja_enemies_v3"
OUT_DIR = r"C:\Dev\Baraja\design\cards_source_clean"
os.makedirs(OUT_DIR, exist_ok=True)

FILES = ["calaca_menor.png", "alma_en_pena.png", "perro_xolo.png",
         "guardian_de_ofrenda.png", "doble_calavera.png", "la_catrina.png",
         "catrina_menor.png"]

THRESH = 25  # anything this dark or darker is background, not real content -
             # measured corners ranged 0-11, this leaves real headroom while
             # staying well clear of any actual character color/detail

for f in FILES:
    img = Image.open(os.path.join(SRC_DIR, f)).convert("RGB")
    arr = np.array(img).astype(int)
    maxc = arr.max(axis=2)
    mask = maxc < THRESH
    before_count = mask.sum()
    arr[mask] = [0, 0, 0]
    out = Image.fromarray(arr.astype("uint8"), "RGB")
    out.save(os.path.join(OUT_DIR, f))
    print(f, "pixels forced to true black:", before_count)
