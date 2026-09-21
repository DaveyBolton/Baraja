# Card frame / layout template — v1

Reference canvas: **750 x 1050** (5x a standard 2.5x3.5" card, matches the
resolution most CCG asset pipelines work at, e.g. Hearthstone/Slay the
Spire scale). Downsamples cleanly to any in-game size.

## Zones, top to bottom

| Zone | Region (approx, y in px) | Contents |
|---|---|---|
| Cost gem | top-left corner, ~120px circle, overlaps frame edge | Energy cost number, gold-rimmed gem socket |
| Art window | y 60-580 | Character/suit portrait, from the Enemies or Suits art |
| Name plate | y 580-660 | Card name (bilingual: ES primary, EN on hover/tooltip per the Options language toggle, not printed twice on the card itself — printing both permanently is the "leave margin for Spanish" problem from the localization note, so v1 prints whichever language is active) |
| Type/suit badge | small icon inset into the name plate's left edge | One of the 6 suit icons, or a special icon for the 5 one-off specials |
| Rules text box | y 660-950 | Card effect text, ornate parchment-style inset panel |
| Rarity accent | y 950-1050 | Bottom trim, gets the foil shader treatment (see VISUAL_DIRECTION.md) on Gold-tier cards only |

## What's shared vs. per-card

**One frame template, reused by every card.** Gold filigree bezel matching
the established Calaverita aesthetic (same ornamental language as
`header_finegold`/`title_finegold`/the button filigree assets). The suit
badge icon is what tells the two suits/cards apart, not a different frame
per suit, that keeps the art budget to one frame asset instead of six.

**The foil/rarity shader (from VISUAL_DIRECTION.md) is the only
per-rarity visual difference**, applied at the rarity-accent zone and
border edge for Gold-tier cards only, not a second frame asset.

## Status (2026-09-20)

Three iterations to lock this:

1. First frame merged the name plate into the art window with no divider.
2. Second, prompted for three explicit stacked panels, got the art/name/
   text split right, but Dave's read was that it looked like a picture
   boxed inside a frame rather than one merged card object, and asked
   for the gold border ~15% narrower.
3. Third addressed both: regenerated with an explicitly slender border
   (part of the fix was compositing technique, not just the prompt) and
   switched the production technique from "flat-paste art into a black
   window" to a real transparent overlay: chroma-key the frame's
   near-black pixels to alpha (this also opens up its papel-picado lace
   holes as genuine cutouts), let the character art bleed full-width
   underneath through the art zone and halfway into the name bar, then
   composite the alpha-keyed frame on top. That's what makes the art
   read as sitting *under* the ornate border rather than boxed inside it.

**This is now the production technique for all 18 cards**, not just a
one-off for this mockup: alpha-key the frame once, reuse it as an overlay
per card.

## Round 2 (same day): proportions, and a lesson on prompting vs. reference

Dave's read on the v3 frame: loved it, but wanted the gold frame
noticeably thinner, the art window bigger ("show off that beautiful
character"), and the text section down to about a third of the card with
larger type.

First attempt at the fix (v4) just asked for a thinner border again in
plain text ("no more than 5% of the card's width"). It came out thinner
than v3, but Dave's reaction was that the border "keeps getting bigger"
despite asking each time — the real lesson: **text-to-image generation
does not reliably respect precise numeric/proportion requests.** "No more
than 5%" reads to the model as a vague style cue, not a geometric
constraint, so results wobble across generations independent of the
number asked for.

The fix: stop guessing via prompt wording and **draw the exact geometry
by hand** (`design/card_frame_reference_flat.png` — flat gold rectangle,
precise 3%-of-width border, panels sized to the agreed proportions, name
plate sized to actually fit a title), then use ArtEngine's **edit mode**
to skin that exact reference in the gold-filigree style (same technique
the original skull colorways used). Edit mode still isn't pixel-perfect
(the cost gem drifted from top-left to bottom-center, and the panel
split shifted slightly), but the border thickness landed exactly where
asked because it was defined in pixels, not described in words. The
drifted gem was kept as a bottom decorative accent rather than fought
further; a second, hand-drawn gem circle handles the actual cost number
at top-left.

**Lesson for future art passes:** when a proportion or geometric
constraint actually matters (not just style/mood), draw the reference
and use edit mode. Reserve plain text-to-image for style and content,
not precise layout.

## Round 3 (same day): the v5 regeneration was the wrong move

Dave's reaction to v5 (the edit-mode-on-flat-reference frame): the border
was, if anything, bigger again, and — the real point — he'd already told
me v4's actual scrollwork design was beautiful; regenerating a new
illustration to fix thickness was itself the mistake, not just an
imperfect result. Precise pixel width doesn't matter if the ornament
density/visual weight reads as heavier.

**The fix: never regenerate to resize.** Went back to v4's actual
approved artwork and shrunk its border via a proper 9-slice (measured
the real thickness first — 107px left/right, 46px top, 60px bottom, out
of a 736x1040 canvas — then scaled the four corner blocks and squeezed
just the straight edge strips by 0.55x, enlarging the interior to fill
the reclaimed space, canvas size unchanged). Same exact artwork,
genuinely thinner border, zero AI involved in this step.

That enlarged interior then re-cramped the name plate (badge and title
overlapping again). Fixed in the compositing script, not the artwork:
the name text was centered across the whole name-zone width regardless
of where the badge sat; switched to badge-first, then left-aligned title
after it with its own padding. No image regeneration needed for that
either.

**Standing rule now:** a proportion problem gets fixed by measuring
pixels and directly manipulating the existing artwork (crop/scale/9-slice)
or by compositing logic, never by re-prompting the generator and hoping
the new result both fixes the issue and keeps the same look. Text-to-image
generation doesn't hold a style fixed across re-rolls reliably enough to
risk it once a design is approved.

## Round 4 (same day): name plate doubled

Even after the crowding fix, Dave's read was that the name plate itself
is "1/2 the size it should be" — not a spacing bug, an actual size call.
Same technique again: measured the plate's real height (77px) and used a
targeted 3-band vertical reslice (art/name/text, full width, borders
untouched) to double it to 154px, taking the freed space proportionally
from art and text (art 395->340, text 155->133) so the art:text balance
stays close to what was already approved. No regeneration.

## Round 5 (same day): suit badge moves to a bottom rosette medallion

Dave wanted to try moving the suit badge out of the name plate entirely,
into a round rosette medallion straddling the bottom border — like the
gem socket on the v5 frame we set aside. First attempt reused that v5
gem's housing directly and it came out messy (a sliver of the neighboring
lace flourish included, the skull mis-sized against the rim). Dave's
correction: don't borrow from the discarded frame at all — extract the
*same* clean gem housing that's already on our own approved frame (the
top-left cost gem), and duplicate that to the bottom with the skull
inside instead of the gem.

That framing is genuinely clean since it's an isolated corner element
with no neighboring ornament to catch in the crop, unlike the v5 gem
which sat between two lace flourishes. Measured precisely rather than
guessed (ball radius ~24.5px, rim outer radius ~38-40px, both from the
actual pixels) after a first pass overshot the crop radius and produced
an oversized skull spilling past the rim. Name plate is now text only,
centered.

## Round 6 (same day): stop cropping mismatched sources, draw it instead

Every attempt at the bottom medallion so far (from the v5 gem, then from
our own top-left gem housing) fought the same problem: cropping a circle
out of an image that was never composed as an isolated circular asset
drags along neighboring elements (lace flourishes, the panel/border
boundary) that don't belong. The v5-gem attempt carried a stray lace
sliver no matter how the crop was framed; even the cleaner top-left-gem
version was fighting the same class of problem, just with a smaller
error.

Dave's call: stop patching crops, **draw the ring procedurally instead**
— sample the frame's own actual gold RGB values directly from its
pixels (bright highlight ~(250,232,105), mid gold ~(222,178,50), shadow
gold ~(135,82,12)), then build the medallion from concentric beveled
circles in those exact colors plus a soft specular highlight arc, with a
small dark recessed socket behind the badge and a soft drop shadow under
it for depth. Zero cropping, zero artifacts, and the color matches the
frame exactly because it was sampled from the frame.

**Standing technique from here on:** any decorative element that needs
to sit cleanly on top of existing art (medallions, rosettes, badges,
gem sockets) gets drawn procedurally from sampled colors, not
crop-and-pasted from a different image, even an approved one. This is
faster in practice, not slower — the crop route took five attempts and
never fully succeeded; the procedural build worked on the first try.

## Round 7 (same day): the procedural ring wasn't good enough either

Sized 20% down and recolored from sampled border pixels, the procedural
ring was still wrong: Dave's read was "2 tone" — flat concentric bands
read as banded/stepped, not like the border's continuously graded,
richly detailed cast metal. A second attempt with a real per-pixel
radial gradient (numpy, smooth torus shading + directional specular)
was underway when Dave stopped it outright: **use ComfyUI, not more
hand-coded gradients.**

That's the right correction. The lesson from round 6 (draw, don't crop
mismatched sources) was correct as far as it went, but "draw" doesn't
mean "simulate metal shading by hand" - a diffusion model already does
that well, and PIL math was never going to match the richness of actual
generated scrollwork. The right tool per problem: crops from unrelated
images → draw a flat reference and skin it in edit mode (same move as
the frame itself in round 3). Simple flat colors/shapes → fine to draw
directly (the dark socket, the drop shadow). Rich ornamental metal
texture → generate it.

Built a flat reference circle (`design/medallion_reference_flat.png`,
outer ring + black center) and ran it through ArtEngine edit mode with
the same prompt language used for the frame ("keep the exact same ring
shape... only restyle the flat color... ornate polished gold filigree
scrollwork... matching a fine gold picture-frame ring"). One pass,
correct on the first try — real scrollwork, continuous gradient shading,
no banding. Chroma-keyed to alpha, dark socket and skull with drop
shadow added on top (that part is still hand-drawn, correctly - simple
flat shapes don't need generation), resized to the same 160px/
bottom-aligned placement as the procedural version.

**One gotcha worth remembering:** the reference was drawn at 400x400,
but ArtEngine's edit mode returned the result at 1024x1024 (its own
default canvas), not the reference's size. Proportions carried over
correctly, but any absolute pixel measurement (like the socket radius)
has to be re-measured on the actual output, not assumed from the
reference's own coordinates - this is what made the first socket/skull
placement attempt come out tiny.

## Round 8 (same day): thinner ring band, and a real color match

Two more notes on the AI-generated ring: the band itself was too thick,
and the gold read slightly orange/coppery next to the frame's more
yellow-brass gold. Both fixed by measuring and manipulating the existing
generated ring directly, not regenerating:

- **Thinner band:** for a radially symmetric ring, scaling the whole
  image up about its own center and cropping back to the original
  canvas size pushes the inner edge outward (band gets thinner) while
  the outer edge gets clamped by the canvas boundary (stays put). Same
  zoom-and-crop idea that failed for the rectangular card frame back in
  round 3 (it cut off the top/bottom border entirely there), but it
  works correctly here because a ring's symmetry means there's no "short
  side" to disappear the way the frame's already-thin top/bottom rails
  did.
- **Color match:** sampled both the ring's and the frame's actual gold
  pixels (mean, brightest 5%, darkest 15%) and compared channel ratios
  rather than eyeballing it. The ring was low on green and high on blue
  relative to red compared to the frame — reads orange/copper instead of
  yellow-brass. Fixed with a straight per-channel multiply (green x1.28,
  blue x0.68), no regeneration.

## Round 9 (same day): the ornate ring was the wrong call — go simple

After one more pass (thinner still, bled off the bottom edge), Dave's
verdict on the ornate scrollwork ring was "terrible" — scrapped outright,
not iterated on further. His replacement direction: **one simple solid
gold ring**, no scrollwork, no filigree, matching the border's gold,
bottom flush with the card's own bottom edge. Simpler than every prior
attempt, and it leaves far more visible room for the skull, which was
the actual point of the medallion in the first place.

Generated via the same flat-reference + edit-mode technique, just with
the prompt asking for "smooth, solid, polished gold... no scrollwork, no
filigree, no engraving, no pattern of any kind" instead of ornate detail.
One generation, correct on the first try. Color-matched the same way as
round 8 (sample both images' actual pixel stats — mean, brightest 5%,
darkest 15% — and correct by the numbers, not by eye): the ring came out
too pale/blue at the highlights and too light/washed in the shadows
compared to the frame, fixed with a contrast stretch plus a blue-channel
reduction. Thinned slightly with the same zoom-and-crop trick, socket
and skull added the same way as every version before it.

**Lesson:** ornate detail was solving a problem nobody asked to solve.
Once the goal was just "match the gold, make room for the skull,"
simpler construction got there in one shot where five rounds of
scrollwork refinement hadn't.

## Round 10 (same day): the crop-to-thin trick is banned

Asked to make the solid ring thinner, the zoom-and-crop trick (which
worked fine for the rectangular card frame's border in round 8) made
this ring read as *bigger* instead — the aggressive crop pushed the
ring's outer edge out to fill its entire bounding box with slightly
squared-off corners, so even though the gold band itself was thinner,
the medallion's total footprint looked more dominant on the card, the
opposite of the goal. Two attempts at this made it worse each time.

**Standing rule now: never crop to resize an element. Regenerate.** The
fix was to bake the exact thin proportions into the flat reference
itself (outer radius 190, inner radius 165 at 400x400 — a real 25px
band, not derived by cropping a thicker one) and run it through edit
mode fresh. One generation, correct proportions, no crop artifacts. From
here forward, any proportion change to a generated element goes through
a new reference + edit-mode pass, never a post-hoc crop.

## Round 11 (same day): color matching by eye vs. by measurement

Two guesses at "more yellow" swung wildly (first too orange, then
overcorrected to lemon-yellow) because both were driven by eyeballing
rather than measurement. The fix that actually worked: bake the frame's
own precisely sampled mean gold color `(194,162,50)` directly into the
flat reference as its fill color, and tell edit mode explicitly to
preserve that exact hue rather than "restyle" it — the earlier prompts
all invited the model to reinterpret what "gold" means. Then a real
two-point linear fit per RGB channel (solved from the frame's and ring's
own measured dark15%/bright5% pixel values, not guessed multipliers)
closed the remaining gap. This is the same discipline as round 8, just
applied earlier in the pipeline instead of as an afterthought.

Canonical files: `design/card_frame_template_v18.png` (alpha-keyed
frame), `design/card_mockup_catrina_menor_v18.png` (current mockup),
`design/medallion_final_matched.png` (the standalone medallion). Known
open item, not yet fixed: the medallion overlaps a couple of lines of
rules text on cards with longer text. Not yet imported into the Unity
project's Assets.

## Round 12 (same day): the "art seam" was never the character art

The seam above the flower crown, flagged as an open item since round 2
and blamed on the character art's own background/vignette through
several regenerations of that art, was never the character art's fault.
**Root cause, finally found:** the frame's "plain black" art panel
carries one continuous gradient across its *entire* height, from a
value around 70 near the top down to true black near the bottom — not a
separate decorative element, the whole panel. Every band-detection pass
this whole project used a chroma-key threshold of 38, which only ever
classified the panel's darker lower two-thirds as "black" - the lighter
upper third (values 38-70) was measured as *not* part of the content
panel at all, so the art window's top was consistently misdetected at
y=296 when the true edge (confirmed by looking at the plain frame's own
inset gold line) is y=95. Every mockup's art was placed starting at 296,
leaving the y=95-296 band showing the frame's own un-keyed gradient
pixels sitting on top, unrelated to whatever art was underneath.

This is why regenerating "catrina_menor" to fill more of the frame
(round 12's first move) didn't fix it — the new art was placed in
exactly the same wrong window as the old art. The actual fix: force the
three content-panel rectangles to true black *before* chroma-keying
(deterministic, doesn't depend on tuning a threshold against a gradient
that spans multiple brightness levels) and correct the art panel's top
bound to 95. Side effect: the frame's own cost-gem circle at top-left
sits within that corrected region and got erased by the forced-black
fix, so it's now redrawn in code (plain circle + gold outline) rather
than relying on the frame's own art there.

**Worth remembering for the other 6 enemy portraits**, and for the
Guardián de Ofrenda / Doble Calavera art that hasn't been dropped into a
card yet: none of them need re-cropping or regenerating for this reason
- the fix is in the frame/compositing code, not per-character art.

**Correction, immediately after:** the original approved `catrina_menor`
art is the one in use, not the "fills the frame edge to edge" regen from
earlier this round - that regen is discarded, it was solving a problem
that turned out not to exist. Also, the forced-black rectangles from the
fix above were drawn flush to each panel's edge, which erased the
frame's own thin gold inset accent line around each box. Fixed with a
10px inward padding on the rectangles so they only cover true interior,
never touch the accent line.

## Round 13 (same day): stretch distortion, and a gap at the corner

Two more from the taller art window (round 12's y=95 fix): the art
bleeding past its own box into the name-plate gap (fixed - contained
strictly within the box now, `bleed_bottom = art_band[1]`), and the
character reading visibly stretched taller/thinner than natural. The
stretch was a leftover from an earlier round's "stretch to fit" fix,
which was a flat resize ignoring aspect ratio - harmless when the window
was closer to square, badly distorting now that the window is much
taller. Reverted to aspect-preserving fill (cover-crop, matching height
and cropping the sides) instead of a flat stretch.

Also patched a thin gap at the art box's top-right corner: the padded
black-fill rectangle stopped a few pixels short of the frame's own
inset gold line there, leaving a sliver of the frame's gradient visible.
Extended the fill flush against that line's known x-position (measured,
not guessed).

**Then Dave caught the actual remaining bug** ("title and description
boxes both" sit on top of the frame) after two rounds of me checking the
wrong thing (art box edges, which were fine). Measured properly this
time: the name plate has its own ornate bracket-shaped border ("⌐" ")
that's narrower and extends further inward than the art/text panels'
plain straight line - true bounds 171-565 vs the art panel's 158-578.
Code was reusing the art panel's wider x-bounds for the name box's black
fill (`nx, nrx = ax, arx`), which covered roughly 13px of the bracket
decoration on each side. Fixed by measuring the name band's own x-bounds
independently rather than reusing the art panel's.

**One more from the same complaint, found on a closer look together:**
the medallion (baked into the frame image, bleeding up from the bottom
edge, top at y=880) overlaps the text panel's own bounds (bottom at
y=923). The text panel's forced-black-fill pass was painting solid black
right over that 43px overlap zone before chroma-keying, erasing the top
of the medallion's crown. Fixed by saving that region before the
black-fill pass and pasting it back after - the medallion is meant to
sit in front of the text panel, not get erased by it.

**And the actual title bug**, after two more rounds of checking the
wrong thing: `name_band` was never a properly-detected panel to begin
with. It's the fallback branch's crude leftover gap
(`art_band[1]` to `text_band[0]`), and that gap actually contains three
separate thin content stripes (~660-678, ~697-725, ~745-770) divided by
the frame's own thin gold accent lines - not one uniform zone the way
art_band and text_band are (those went through the real
longest-contiguous-run detection). The full-height rectangle was
painting straight across all of it, erasing those divider lines along
with everything else. Fixed by measuring the true sub-bands directly and
narrowing `name_band` to just the middle stripe (693-729) where the
title text actually sits - the box now reads as a clean double-ruled
rectangle, consistent with the art/text panels, instead of a smear over
the frame's own internal ruling.

**Last one this round, and the actual cause:** not a margin issue after
all - the 10px padding on the forced-black rectangle left an 11px gap
between the true border edge (y=94) and where the rectangle started
(y=105). That gap showed the frame's own un-keyed brownish gradient
(same artifact as round 12, values ~65-74/50-58/17-22), which reads
exactly like the art bleeding over the border even though the art itself
was never involved. Patched by extending the black fill down to the
real edge for the art panel's top specifically.

Dave asked to see the exact spot before any more fixing, and it turned
out the same corner had a second, bigger problem: the whole right-edge
transition zone (x~577-601) is a messy, irregular mix of un-keyed
gradient pixels running the full height of the art box, not a clean
gap with a straight boundary - row-by-row measurement showed it never
fully resolves to solid gold until past x~600. Chasing that irregular
edge with precise rectangles wasn't converging, so the fix was to widen
the exclusion zone generously (to x=600) and accept losing the thin
decorative double-line accent that ran down that side, in exchange for
it actually being clean.

## Round 14 (same day): the actual robust fix — stop touching the frame

Dave's call, after four straight rounds of rectangle patches each fixing
one spot and breaking or missing another: **stop pre-modifying the frame
image entirely. Put the frame on top, unconditionally, always.**

Every rectangle-patch approach was fighting the same losing battle: the
frame's decorative elements (brackets, accent lines, corner flourishes,
the medallion) aren't perfectly rectangular, so any hand-drawn rectangle
meant to force a "content" zone to true black before chroma-keying was
always at risk of clipping something real. Four rounds proved this
empirically - every fix shifted the bug rather than closing it.

The fix that actually holds: **never modify frame_rgb before
chroma-keying.** Key the original, untouched frame. Since the frame
composites on top of the art/background unconditionally, every
decorative element survives by construction - nothing ever painted over
it. The remaining question was whether the un-keyed gradient itself
(peaking ~74) would show through as an ugly patch - tested directly
(rendered the full card with zero rectangle patches) and confirmed it
would, a large brown rectangle sitting over the top of the art.

**Resolved by raising the chroma-key threshold from 38 to 78** - high
enough to catch the whole gradient (measured peak ~74). The worry that a
higher threshold would eat the scrollwork's own dark shading/recessed
groove detail turned out to be untested speculation, not a measured
fact: composited the keyed frame against a bright green background and
checked directly - the ornate filigree, its shadow detail, and the
gem socket all render perfectly intact at threshold 78. No holes.

All four rectangle patches, the medallion save/restore, and the
`art_band[0] = 95` override's supporting infrastructure are gone. The
whole class of "black fill erased a decorative element" bugs from
rounds 12 through 14 is closed by construction, not by chasing each
instance.

Canonical files: `design/card_frame_template_v19.png` (alpha-keyed
frame, threshold 78, frame image itself untouched), 
`design/card_mockup_catrina_menor_v19.png` (current mockup).

## Round 15 (same day): crown detail near the cost gem

Traced one more spot Dave flagged ("weird shading") to the character
art itself, not a frame bug this time: the crown's own top (gold jewel
highlights over a brown woven-cap dome, confirmed by matching pixel
values to the source) lands close to the cost gem in this much-taller
window, and read as debris next to it. Tried a couple of surgical fixes
(cropping the source tighter clipped feather tips instead; a small
punch-out circle didn't fully cover it) before just doing what Dave
asked directly: fill that whole corner area flat black. Trades a hard
edge across the top of the crown for guaranteed-clean debris removal.

## Round 16 (same day): the actual robust architecture

Dave's correction to the whole approach: stop precisely cropping/margining
the art to the window's exact edges. The window is just a transparent
hole in an opaque frame - place the character art (already has its own
clean black background) generously oversized behind it, and let the
frame's own opacity clip it naturally. No more margin math, no more
crop math, no more per-corner patches.

First pass at this **oversized the art and it bled into the wrong
places** - past the window into the gap between window and outer border,
even over the outer scrollwork itself. Root cause: chroma-keying was
still whole-image, color-only. Any dark pixel anywhere in the frame
(including gaps between decorative elements that were never meant to be
"content windows") keyed transparent, so once the art was big enough to
reach those areas, it showed through there too.

**Fix: restrict WHERE chroma-keying can apply, not just how.**
`make_alpha_frame` now takes a list of eligible rectangles (the three
known content boxes' outer bounds) and only tests pixels for
transparency inside them; everywhere else stays opaque no matter how
dark it is. Combined with placing the oversized art behind the frame,
this is the complete fix: decorative elements can never be erased
(round 14's fix, unchanged), and art can never bleed outside its box
(this round's fix) - both guaranteed by construction, not by chasing
individual spots.

Remaining polish: one more residual sliver of the frame's own gradient
(a pixel value of 82, just above the round-14 threshold of 78) needed
the threshold raised to 90 - reverified against a bright background
first, still zero scrollwork damage. The character is now placed at
1.55x the window height, pushed up so its top sits 190px above the
window (hides the crown's edge completely, at the cost of also hiding
most of the crown's jewels - flagged for Dave to weigh in on).

Sizing settled after two more adjustments: full crown/jewels needed to
be visible (dialed oversize back from 1.55 to 1.1, top_hide from 190 to
25 - the aggressive hiding was a workaround for the pre-restriction bleed
bug, no longer needed now that bleeding is prevented structurally), then
sized down further to 0.85 (smaller than the window) and centered for
even black margin on all sides, since edge-to-edge read as too big.

**Confirmed clean by a full sweep** of every corner and gap (all four
art-box corners, the gap below it, both text-box corners, the name
plate) - solid black everywhere, no artifacts. Dave's verdict: "the best
yet." This compositing approach (spatially-restricted chroma-key at
threshold 90, oversized-then-scaled-down art behind an untouched frame)
is now the standard for all 18 cards, not just this one - the same
script applies directly to each of the other 6 enemy portraits and the
remaining suits once their art is ready.

## Round 17: all 7 enemy cards, generalized pipeline

Generalized the single-card script into `build_all_enemy_cards.py`
(one CARDS list entry per enemy: art file, name, rules text pulled
straight from ENEMIES.md's real HP/pattern data, and a suit badge for
the bottom medallion). Corner number matches each enemy's actual fight
order from ENEMIES.md (1, 2, 4, 5, 7, 8, B for boss) rather than a
placeholder. Output goes to `design/cards/`.

**Medallion badge now swaps per card** rather than always showing
Catrina's baked-in purple skull: `swap_medallion_skull()` clears the
socket circle (measured center/radius on the production frame) and
drops in each card's own suit skull with a matching drop shadow, same
technique as the original medallion build.

**First pass reintroduced the round-13 corner sliver** (the gap between
the art panel's right edge and the outer border) - the `x=600` extension
fixed on Catrina Menor's card specifically hadn't been carried into the
generalized eligible_rects. Fixed the same way, in the shared script
this time so it applies to every card automatically.

**Then Dave caught it going deeper: the character PNGs' own backgrounds
aren't pure black** (measured 4-11 per channel at the corners, varying
per image - Perro Xolo and Guardián de Ofrenda were the most visibly
off). This isn't a compositing issue, it's in the source art files
themselves. Fixed with a dedicated cleaning pass
(`design/cards_source_clean/`): any pixel with max(R,G,B) < 25 gets
forced to true (0,0,0), safely below any real character color/detail.
The card-building script now reads from this cleaned folder instead of
ArtEngine's raw output. Catrina Menor's card was folded into the same
unified CARDS list and rebuilt too, superseding the individually-built
version from earlier rounds.

Canonical: `design/cards/*.png` (all 7), `design/cards_source_clean/*.png`
(the cleaned source art, reusable for any future rebuild),
`build_all_enemy_cards.py` is the reference script for the technique.

**Final cleanup:** the working background fill was `(12, 8, 16)`, a
close-but-not-exact approximation of black, fine for a mockup but not
for the production template. Changed to true `(0, 0, 0)` everywhere it's
used (the base canvas fill and the name-plate backing plate, which was
also compositing at 235/255 alpha - now 255, fully opaque). Verified at
the pixel level: text box and name box backgrounds are confirmed
`(0, 0, 0)`. One caveat that isn't a compositing issue: the character
source PNG itself has a near-black (8, 8, 8) background baked into the
approved art, not something this script controls - imperceptible
difference, would need editing the character images themselves to
change.

Canonical files: `design/card_frame_template_v19.png`,
`design/card_mockup_catrina_menor_v19.png`.

## Deferred

Suit-tinted frame borders (if the neutral-gold-plus-badge approach reads
as flat once real cards are in hand), a distinct boss/legendary frame
shape beyond the shader treatment.
