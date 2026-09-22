#!/usr/bin/env python3
"""Build-time grammar MVP for Gloom Bean.

Generates deterministic, validated semantic/spatial plans. Unity consumes the
JSON and builds ordinary StageBuilder/AtlasBuilder objects; the existing art
stack dresses those objects.
"""
from __future__ import annotations
import argparse, hashlib, json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets/GloomBean/Resources/GrammarMvp"
SEEDS = (137, 911, 2026, 4096)

class Rand:
    def __init__(self, seed: int):
        self.s = seed & 0xFFFFFFFF
    def u32(self) -> int:
        self.s = (1664525 * self.s + 1013904223) & 0xFFFFFFFF
        return self.s
    def pick(self, seq):
        return seq[self.u32() % len(seq)]
    def integer(self, lo: int, hi: int) -> int:
        return lo + self.u32() % (hi - lo + 1)

def v(x, y):
    return {"x": round(float(x), 3), "y": round(float(y), 3)}

def surface(role, kind, x, y, w, return_only=False, off=False):
    return {
        "role": role, "kind": kind, "x": round(x, 3), "y": round(y, 3),
        "w": round(w, 3), "returnOnly": return_only,
        "deactivateOnTurn": off,
    }

def generate(seed: int) -> dict:
    r = Rand(seed)
    surfaces = [surface("entry", "floor", 3.5, 0, 17)]
    forward = [surfaces[0]]
    x_end, y = 12.0, 0.0
    for i in range(6):
        gap = 1.4 + 0.2 * r.integer(0, 4)
        dy = r.pick((-1.0, 0.0, 0.0, 1.0))
        y = max(-1.0, min(3.0, y + dy))
        width = float(r.integer(6, 10))
        center = x_end + gap + width * 0.5
        item = surface("forward", "floor", center, y, width)
        surfaces.append(item); forward.append(item)
        x_end = center + width * 0.5

    # Secret branch uses ordinary current-art ledges, not a one-off picture.
    secret_base = forward[2]
    surfaces += [
        surface("secret", "ledge", secret_base["x"] - 1.4, secret_base["y"] + 1.8, 3.2),
        surface("secret", "ledge", secret_base["x"] + 2.2, secret_base["y"] + 3.1, 3.4),
    ]

    # Turn reveals a second route above the forward route. It is intentionally
    # generated from the forward topology so layout and return path co-vary.
    return_surfaces = []
    for i, f in enumerate(reversed(forward[1:])):
        ry = f["y"] + 3.2 + (i % 2) * 0.7
        rs = surface("return", "ledge", f["x"], ry, min(5.2, max(3.6, f["w"] * 0.65)), True)
        surfaces.append(rs); return_surfaces.append(rs)

    far = forward[-1]
    source_floor = forward[1]
    secret_top = surfaces[-len(return_surfaces)-1]
    hazards = []
    for a, b in zip(forward[:-1], forward[1:]):
        right = a["x"] + a["w"] * 0.5
        left = b["x"] - b["w"] * 0.5
        gap = max(0.0, left - right)
        if gap > 1.45:
            hazards.append({"x": round((right+left)*0.5,3), "y": round(min(a["y"],b["y"])-1.25,3), "width": round(gap,3)})

    enemies = []
    for f in forward[2:-1:2]:
        enemies.append({"x": round(f["x"],3), "y": round(f["y"]+0.65,3), "direction": -1 if r.u32() & 1 else 1})

    coins = []
    for f in forward[1:-1]:
        coins.append({
            "ax": round(f["x"]-min(2.0,f["w"]*.3),3), "ay": round(f["y"]+1.2,3),
            "bx": round(f["x"]+min(2.0,f["w"]*.3),3), "by": round(f["y"]+1.2,3),
            "count": r.integer(3, 6),
        })

    possession = r.pick(("Marionette", "Echo", "Molt"))
    max_x = far["x"] + far["w"] * 0.5 + 7
    return {
        "schemaVersion": 1,
        "id": f"GB-GRAMMAR-{seed:04d}",
        "seed": seed,
        "title": f"Generated Laundry {seed}",
        "worldId": "W1",
        "visualCourse": 3,
        "possession": possession,
        "bounds": {"x": -8.0, "y": -10.0, "w": round(max_x+16,3), "h": 36.0},
        "spawn": v(2.0, 1.1),
        "exit": v(2.0, 1.1),
        "source": v(source_floor["x"]-1.2, source_floor["y"]+1.0),
        "cure": v(5.0, 1.0),
        "key": v(far["x"]-2.0, far["y"]+1.35),
        "nail": v(far["x"]+2.0, far["y"]+0.45),
        "mercy": v(secret_top["x"], secret_top["y"]+1.25),
        "surfaces": surfaces,
        "hazards": hazards,
        "enemies": enemies,
        "coins": coins,
        "productionTrace": [
            "Level -> Entry Forward Turn Return Exit",
            "Forward -> SafeRun Transform Beat Beat Key",
            "Turn -> Nail Reveal(Return)",
            "Return -> AlternateHighPath OptionalSecret Exit",
            "Visual -> ExistingObjectDrivenArt(StageBuilderObjects)",
        ],
    }

def canonical(plan: dict) -> str:
    return json.dumps(plan, indent=2, sort_keys=True) + "\n"

def validate(plan: dict) -> list[str]:
    errors = []
    if plan.get("schemaVersion") != 1: errors.append("schema")
    surfaces = plan.get("surfaces") or []
    roles = {s["role"] for s in surfaces}
    for role in ("entry","forward","return","secret"):
        if role not in roles: errors.append("missing-role:"+role)
    if not any(s["returnOnly"] for s in surfaces): errors.append("no-return-only")
    if plan["exit"] != plan["spawn"]: errors.append("exit-not-at-entry")
    if not (plan["source"]["x"] < plan["key"]["x"] < plan["nail"]["x"]): errors.append("semantic-order")
    b = plan["bounds"]; xmin,xmax=b["x"],b["x"]+b["w"]; ymin,ymax=b["y"],b["y"]+b["h"]
    for i,s in enumerate(surfaces):
        if s["w"] <= 0: errors.append(f"surface-{i}-width")
        if not (xmin <= s["x"] <= xmax and ymin <= s["y"] <= ymax): errors.append(f"surface-{i}-bounds")
    forward = [s for s in surfaces if s["role"] in ("entry","forward")]
    forward.sort(key=lambda s:s["x"])
    for i,(a,z) in enumerate(zip(forward[:-1],forward[1:])):
        gap=(z["x"]-z["w"]*.5)-(a["x"]+a["w"]*.5)
        if gap > 2.3: errors.append(f"forward-gap-{i}:{gap:.2f}")
        if abs(z["y"]-a["y"]) > 1.01: errors.append(f"forward-rise-{i}")
    expected = {
        "Level -> Entry Forward Turn Return Exit",
        "Turn -> Nail Reveal(Return)",
        "Visual -> ExistingObjectDrivenArt(StageBuilderObjects)",
    }
    if not expected.issubset(set(plan.get("productionTrace") or [])): errors.append("production-trace")
    return errors

def write_all() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    for seed in SEEDS:
        plan = generate(seed)
        errors = validate(plan)
        if errors: raise SystemExit(f"{seed}: {errors}")
        (OUT / f"{plan['id']}.json").write_text(canonical(plan), encoding="utf-8")

def check() -> None:
    for seed in SEEDS:
        plan=generate(seed); errors=validate(plan)
        if errors: raise SystemExit(f"{seed}: invalid generated plan: {errors}")
        path=OUT/f"{plan['id']}.json"
        if not path.exists(): raise SystemExit(f"missing {path}")
        if path.read_text(encoding="utf-8") != canonical(plan): raise SystemExit(f"drift {path}")
    print("GRAMMAR_MVP_CHECK_PASS")

def audit() -> None:
    fingerprints=set()
    for seed in SEEDS:
        plan=generate(seed); errors=validate(plan)
        if errors: raise SystemExit(f"{seed}: {errors}")
        geometry=json.dumps(plan["surfaces"],sort_keys=True).encode()
        fingerprints.add(hashlib.sha256(geometry).hexdigest())
        if len([s for s in plan["surfaces"] if s["role"]=="return"]) < 5: raise SystemExit("return too small")
        if len(plan["productionTrace"]) < 5: raise SystemExit("trace too small")
    if len(fingerprints)!=len(SEEDS): raise SystemExit("seeded plans are not distinct")
    print(f"plans={len(SEEDS)} distinct={len(fingerprints)}")
    print("GRAMMAR_MVP_AUDIT_PASS")

def main():
    p=argparse.ArgumentParser()
    p.add_argument("--write",action="store_true")
    p.add_argument("--check",action="store_true")
    p.add_argument("--audit",action="store_true")
    args=p.parse_args()
    if not (args.write or args.check or args.audit): args.write=args.check=args.audit=True
    if args.write: write_all()
    if args.check: check()
    if args.audit: audit()

if __name__=="__main__": main()
