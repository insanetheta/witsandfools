#!/usr/bin/env python3
"""Generate the remaining missing card art via OpenRouter NanoBanana (Gemini Flash Image).

Reads Assets/Data/card_catalog.json for all card ids, skips those already present in
Assets/Resources/CardArt, pulls each prompt from nanobanana_art_prompts.md (the backtick
line after **id**), or from AUTHORED below for the 10 cards that lacked a doc prompt.
Saves directly into Assets/Resources/CardArt/{id}.png.

Usage:  python3 generate_all_missing.py [--limit N]   (omit --limit for all)
"""
import base64, json, os, re, sys, time, urllib.request

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # repo root
ART = os.path.join(ROOT, "witsandfools", "Assets") if os.path.basename(ROOT) != "witsandfools" else os.path.join(ROOT, "Assets")
# Resolve Assets relative to this file instead (robust):
HERE = os.path.dirname(os.path.abspath(__file__))            # .../Assets/Art
ASSETS = os.path.dirname(HERE)                                # .../Assets
CATALOG = os.path.join(ASSETS, "Data", "card_catalog.json")
PROMPT_DOC = os.path.join(HERE, "nanobanana_art_prompts.md")
OUT_DIR = os.path.join(ASSETS, "Resources", "CardArt")

API_KEY = (lambda _p: open(_p).read().strip() if os.path.exists(_p) else os.environ.get("OPENROUTER_API_KEY", ""))(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".openrouter_key"))  # key read from untracked Assets/Art/.openrouter_key (never committed)
MODEL = "google/gemini-2.5-flash-image"
API_URL = "https://openrouter.ai/api/v1/chat/completions"

STYLE_PREFIX = ("Renaissance oil painting, Caravaggio chiaroscuro lighting, warm candlelight, "
    "rich earth tones and gold accents, dark atmospheric background, dramatic shadows, "
    "16th century Italian aesthetic, painterly brushwork, portrait 2:3 aspect ratio, "
    "no text or numbers in the image. ")

# Prompts authored for the 10 cards that had no doc entry (bare descriptions; STYLE_PREFIX added).
AUTHORED = {
 "schemer_expurgator": "portrait of a severe inquisitor-scholar blacking out lines of a forbidden book with heavy ink strokes, stacks of censored tomes, candlelit scriptorium with dark shelves, intense secrecy",
 "schemer_censor": "portrait of a stern official pressing a black wax seal to suppress a sealed letter, red ribbon and confiscated documents, single taper candle, shadowed bureau office",
 "brute_scorched_earth": "portrait of a grim armored soldier raising a flaming torch before a burning field at night, smoke and drifting embers, scarred face lit by firelight, war-torn landscape",
 "brute_forge_master": "portrait of a burly blacksmith hammering a glowing sword blade on an anvil, sparks flying, heavy leather apron, muscular scarred arms, fiery forge glow in a dark smithy",
 "trickster_transmuter": "portrait of a theatrical conjurer transmuting a coin between gloved hands with a flourish, swirling colored smoke, carnival stage, Venetian masks in the shadowed background",
 "trickster_identity_thief": "portrait of a masked figure holding up a stolen face-shaped mask resembling another person, mid-disguise swapping cloaks, moonlit Venetian carnival alley, sly intrigue",
 "hoarder_glutton": "portrait of a corpulent richly-dressed merchant feasting greedily at a laden table, one hand grabbing coins the other food, overflowing platters and goblets, candlelit dining hall",
 "hoarder_opportunist": "portrait of a sharp-eyed trader snatching a coin purse the instant a back is turned, calculating grin, market stall with brass scales and ledgers, warm candlelight",
 "hoarder_grudge_keeper": "portrait of a bitter old moneylender clutching a thick ledger of debts, narrowed vengeful eyes, tally marks and unpaid notes, dim counting house with an iron-bound chest",
 "hoarder_dragons_hoard": "portrait of a miser reclining atop a mountain of gold coins, jewelled goblets and treasure, greedy protective posture, candlelit vault glittering with riches",
}

def load_missing():
    cards = json.load(open(CATALOG))["cards"]
    ids = [c["id"] for c in cards]
    done = set(os.path.splitext(f)[0] for f in os.listdir(OUT_DIR) if f.endswith(".png"))
    return [i for i in ids if i not in done]

def parse_doc_prompts():
    text = open(PROMPT_DOC).read()
    out = {}
    # **id** ... line, then a line wrapped in backticks = the prompt
    for m in re.finditer(r"\*\*([a-z_]+)\*\*[^\n]*\n`([^`]+)`", text):
        out[m.group(1)] = m.group(2).strip()
    return out

def prompt_for(card_id, doc):
    if card_id in doc:
        return doc[card_id]            # doc prompts already include the full style line
    if card_id in AUTHORED:
        return STYLE_PREFIX + AUTHORED[card_id]
    return None

def generate(prompt):
    payload = {"model": MODEL, "messages": [{"role": "user", "content": f"Generate an image: {prompt}"}]}
    headers = {"Authorization": f"Bearer {API_KEY}", "Content-Type": "application/json",
               "HTTP-Referer": "https://witsandfools.com", "X-Title": "Wits and Fools Card Art"}
    req = urllib.request.Request(API_URL, data=json.dumps(payload).encode(), headers=headers, method="POST")
    with urllib.request.urlopen(req, timeout=180) as resp:
        data = json.load(resp)
    msg = (data.get("choices") or [{}])[0].get("message", {})
    for img in msg.get("images", []) or []:
        url = img.get("image_url", {}).get("url", "") if isinstance(img, dict) else ""
        if not url and isinstance(img, dict):
            url = img.get("url", "")
        if url.startswith("data:image"):
            return url.split(",", 1)[1]
    raise RuntimeError("no image in response: " + json.dumps(data)[:300])

def main():
    if not API_KEY:
        print("ERROR: set OPENROUTER_API_KEY env var before running."); sys.exit(1)
    limit = None
    if "--limit" in sys.argv:
        limit = int(sys.argv[sys.argv.index("--limit") + 1])
    missing = load_missing()
    doc = parse_doc_prompts()
    if limit:
        missing = missing[:limit]
    print(f"generating {len(missing)} cards -> {OUT_DIR}")
    ok = fail = 0
    for i, cid in enumerate(missing):
        p = prompt_for(cid, doc)
        if not p:
            print(f"[{i+1}/{len(missing)}] {cid}: NO PROMPT, skip"); fail += 1; continue
        for attempt in (1, 2):
            try:
                b64 = generate(p)
                open(os.path.join(OUT_DIR, cid + ".png"), "wb").write(base64.b64decode(b64))
                kb = len(base64.b64decode(b64)) // 1024
                print(f"[{i+1}/{len(missing)}] {cid}: OK ({kb} KB)"); ok += 1; break
            except Exception as e:
                print(f"[{i+1}/{len(missing)}] {cid}: attempt {attempt} ERROR {str(e)[:120]}")
                if attempt == 2: fail += 1
                else: time.sleep(3)
        time.sleep(1.5)
    print(f"DONE ok={ok} fail={fail}")

if __name__ == "__main__":
    main()
