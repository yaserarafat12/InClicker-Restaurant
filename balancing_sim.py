"""
InResto Economy Balancing Simulator
====================================
Wajib per .gemini.d — Game Dev Protocol §3: Economy Balancing (The Simulation Layer)

Tujuan:
  - Simulasikan progres pemain secara otomatis berdasarkan logika upgrade eksponensial
  - Prediksi seberapa jauh pemain maju dalam 1 jam, 24 jam, dan 7 hari
  - Deteksi dini paywall / timegate yang terlalu ketat
  - Output laporan tabel + grafik ASCII untuk review cepat

Cara pakai:
  python balancing_sim.py                    # simulasi default (semua lokasi)
  python balancing_sim.py --hours 24         # focus 24 jam
  python balancing_sim.py --location 3       # mulai dari lokasi 3
  python balancing_sim.py --strategy greedy  # upgrade pilar tertinggi dulu
"""

import argparse
import math
import sys
from dataclasses import dataclass, field
from typing import List, Dict, Tuple

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

# ─────────────────────────────────────────────────────────────────────────────
#  CONSTANTS — Mirror dari GDD §3 dan GDD §6
# ─────────────────────────────────────────────────────────────────────────────

LOCATIONS = [
    {"id": 1, "name": "Warteg Gang Sempit",         "max_level": 20,  "revenue_mult": 1.0,    "expand_cost": 0},
    {"id": 2, "name": "Warteg Pinggir Jalan",        "max_level": 25,  "revenue_mult": 5.0,    "expand_cost": 500_000_000},
    {"id": 3, "name": "Warung Rakyat",               "max_level": 35,  "revenue_mult": 10.0,   "expand_cost": 50_000_000_000},
    {"id": 4, "name": "Warung Keluarga",             "max_level": 50,  "revenue_mult": 25.0,   "expand_cost": 1_000_000_000_000},
    {"id": 5, "name": "Restoran Sederhana",          "max_level": 70,  "revenue_mult": 50.0,   "expand_cost": 20_000_000_000_000},
    {"id": 6, "name": "Restoran Kelas Menengah",     "max_level": 90,  "revenue_mult": 100.0,  "expand_cost": 400_000_000_000_000},
    {"id": 7, "name": "Fine Dining",                 "max_level": 120, "revenue_mult": 250.0,  "expand_cost": 8_000_000_000_000_000},
    {"id": 8, "name": "Franchise Nasional",          "max_level": 150, "revenue_mult": 500.0,  "expand_cost": 160_000_000_000_000_000},
    {"id": 9, "name": "Cabang Internasional (Tokyo)","max_level": 200, "revenue_mult": 1000.0, "expand_cost": 3_200_000_000_000_000_000},
    {"id":10, "name": "World Domination",            "max_level": 999, "revenue_mult": 2500.0, "expand_cost": 0},
]

# Base costs per pilar (dari GDD §A.2 dan pillar controllers)
BASE_COSTS = {
    "dapur":      10_000,
    "area_makan": 12_000,
    "kasir":      11_000,
}

# Exponent per pilar (dari controller .cs files)
COST_MULTIPLIERS = {
    "dapur":      1.15,
    "area_makan": 1.18,
    "kasir":      1.16,
}

BASE_PRODUCTION = {
    "dapur":      1.0,   # rate base (× 1.15^level, proportional)
    "area_makan": 0.9,   # FIXED: exponential (× 1.15^level), bukan linear +1 kursi
    "kasir":      0.8,   # rate base (× 1.16^level, proportional)
}

BASE_PRICE_PER_DISH = 15_000  # Rp
EATING_DURATION     = 5.0     # detik (default, bisa turun dengan upgrade area makan)
TICK_INTERVAL       = 1.0     # detik

PRESTIGE_BONUS_PER_LOCATION = 0.10  # +10% per lokasi


# ─────────────────────────────────────────────────────────────────────────────
#  HELPERS
# ─────────────────────────────────────────────────────────────────────────────

def upgrade_cost(pillar: str, level: int) -> float:
    """Harga upgrade ke level berikutnya dari level saat ini."""
    base = BASE_COSTS[pillar]
    mult = COST_MULTIPLIERS[pillar]
    return math.floor(base * (mult ** (level - 1)))


def production_rate(pillar: str, level: int) -> float:
    """
    Seberapa besar kontribusi pilar pada income per tick.
    FIXED: area_makan sekarang exponential (mirror MakanController v2).
    """
    if pillar == "dapur":
        # Mirror DapurController: baseProductionPerSecond * currentLevel (linear * level)
        return BASE_PRODUCTION["dapur"] * level
    elif pillar == "area_makan":
        # FIXED: exponential, bukan linear — mirror MakanController v2
        # baseCapacityRate * 1.15^(level-1)
        return BASE_PRODUCTION["area_makan"] * (1.15 ** (level - 1))
    elif pillar == "kasir":
        # Mirror KasirController: baseProcessingRate * currentLevel
        return BASE_PRODUCTION["kasir"] * level
    return 0.0


def effective_rps(dapur_lv: int, makan_lv: int, kasir_lv: int,
                  revenue_mult: float, prestige_bonus: float) -> float:
    """
    Revenue Per Second — dibatasi bottleneck (minimum dari 3 pilar).
    Sesuai GDD §3.2 dan PillarManager.ProcessTick()
    """
    p = production_rate("dapur", dapur_lv)
    c = production_rate("area_makan", makan_lv)
    k = production_rate("kasir", kasir_lv)

    effective = min(p, c, k)
    rps = effective * BASE_PRICE_PER_DISH * revenue_mult * (1 + prestige_bonus)
    return rps


def format_rupiah(amount: float) -> str:
    if amount >= 1e12:  return f"Rp {amount/1e12:.2f}T"
    if amount >= 1e9:   return f"Rp {amount/1e9:.2f}B"
    if amount >= 1e6:   return f"Rp {amount/1e6:.2f}M"
    if amount >= 1e3:   return f"Rp {amount/1e3:.1f}K"
    return f"Rp {amount:.0f}"


def format_time(seconds: float) -> str:
    if seconds >= 86400:    return f"{seconds/86400:.1f} hari"
    if seconds >= 3600:     return f"{seconds/3600:.1f} jam"
    if seconds >= 60:       return f"{seconds/60:.1f} menit"
    return f"{seconds:.0f} detik"


def bottleneck_label(dapur_lv, makan_lv, kasir_lv) -> str:
    p = production_rate("dapur", dapur_lv)
    c = production_rate("area_makan", makan_lv)
    k = production_rate("kasir", kasir_lv)
    mn = min(p, c, k)
    if mn == p: return "🔴 Dapur"
    if mn == c: return "🔴 Area Makan"
    return "🔴 Kasir"


# ─────────────────────────────────────────────────────────────────────────────
#  UPGRADE STRATEGIES
# ─────────────────────────────────────────────────────────────────────────────

def pick_upgrade_balanced(dapur_lv, makan_lv, kasir_lv, max_lv) -> str:
    """Upgrade pilar terlemah dulu (strategi optimal GDD §3.2)."""
    p = production_rate("dapur", dapur_lv)
    c = production_rate("area_makan", makan_lv)
    k = production_rate("kasir", kasir_lv)
    mn = min(p, c, k)
    if mn == p and dapur_lv < max_lv:     return "dapur"
    if mn == c and makan_lv < max_lv:     return "area_makan"
    if mn == k and kasir_lv < max_lv:     return "kasir"
    # Semua bottleneck sama atau max
    for pil, lv in [("dapur", dapur_lv), ("area_makan", makan_lv), ("kasir", kasir_lv)]:
        if lv < max_lv: return pil
    return None


def pick_upgrade_greedy(dapur_lv, makan_lv, kasir_lv, max_lv) -> str:
    """Upgrade pilar terkuat terus (strategi suboptimal — untuk perbandingan)."""
    p = production_rate("dapur", dapur_lv)
    c = production_rate("area_makan", makan_lv)
    k = production_rate("kasir", kasir_lv)
    mx = max(p, c, k)
    if mx == p and dapur_lv < max_lv:     return "dapur"
    if mx == c and makan_lv < max_lv:     return "area_makan"
    if mx == k and kasir_lv < max_lv:     return "kasir"
    for pil, lv in [("dapur", dapur_lv), ("area_makan", makan_lv), ("kasir", kasir_lv)]:
        if lv < max_lv: return pil
    return None


STRATEGIES = {
    "balanced": pick_upgrade_balanced,
    "greedy":   pick_upgrade_greedy,
}


# ─────────────────────────────────────────────────────────────────────────────
#  SIMULATOR
# ─────────────────────────────────────────────────────────────────────────────

@dataclass
class SimResult:
    location_name:   str
    time_elapsed_s:  float
    cash:            float
    dapur_lv:        int
    makan_lv:        int
    kasir_lv:        int
    total_earned:    float
    upgrades_bought: int
    bottleneck:      str
    rps_at_end:      float


def simulate(
    start_location_idx: int = 0,
    simulate_hours: float = 168,      # 7 hari default
    strategy: str = "balanced",
    starting_cash: float = 200_000,
    verbose: bool = False,
) -> List[SimResult]:

    pick_upgrade = STRATEGIES.get(strategy, pick_upgrade_balanced)

    cash       = starting_cash
    total_earned = 0.0
    loc_idx    = start_location_idx
    prestige_bonus = loc_idx * PRESTIGE_BONUS_PER_LOCATION

    dapur_lv   = 1
    makan_lv   = 1
    kasir_lv   = 1

    total_seconds  = simulate_hours * 3600
    elapsed        = 0.0
    upgrades_bought = 0

    # Snapshot checkpoints
    checkpoints = {3600: None, 86400: None, 604800: None}  # 1 jam, 24 jam, 7 hari
    results = []

    loc = LOCATIONS[loc_idx]
    max_lv = loc["max_level"]
    rev_mult = loc["revenue_mult"]

    if verbose:
        print(f"\n{'='*60}")
        print(f"  InResto Economy Simulation — Strategy: {strategy.upper()}")
        print(f"  Start: {loc['name']} | Cash: {format_rupiah(cash)}")
        print(f"{'='*60}\n")

    while elapsed < total_seconds:
        # Tick income
        rps = effective_rps(dapur_lv, makan_lv, kasir_lv, rev_mult, prestige_bonus)
        income = rps * TICK_INTERVAL
        cash         += income
        total_earned += income
        elapsed      += TICK_INTERVAL

        # Cek expand ke lokasi berikutnya
        if (dapur_lv >= max_lv and makan_lv >= max_lv and kasir_lv >= max_lv
                and loc_idx < len(LOCATIONS) - 1):
            next_loc = LOCATIONS[loc_idx + 1]
            if cash >= next_loc["expand_cost"]:
                cash -= next_loc["expand_cost"]
                loc_idx += 1
                loc = LOCATIONS[loc_idx]
                max_lv = loc["max_level"]
                rev_mult = loc["revenue_mult"]
                prestige_bonus += PRESTIGE_BONUS_PER_LOCATION
                dapur_lv = makan_lv = kasir_lv = 1
                if verbose:
                    print(f"  [{format_time(elapsed)}] 🏪 PINDAH → {loc['name']} | Cash: {format_rupiah(cash)}")

        # Beli upgrade jika ada yang terjangkau
        to_upgrade = pick_upgrade(dapur_lv, makan_lv, kasir_lv, max_lv)
        if to_upgrade:
            current_lvs = {"dapur": dapur_lv, "area_makan": makan_lv, "kasir": kasir_lv}
            cost = upgrade_cost(to_upgrade, current_lvs[to_upgrade])
            if cash >= cost:
                cash -= cost
                if to_upgrade == "dapur":     dapur_lv += 1
                elif to_upgrade == "area_makan": makan_lv += 1
                elif to_upgrade == "kasir":   kasir_lv += 1
                upgrades_bought += 1

        # Snapshot
        for cp_s, cp_val in checkpoints.items():
            if cp_val is None and elapsed >= cp_s:
                checkpoints[cp_s] = SimResult(
                    location_name   = loc["name"],
                    time_elapsed_s  = elapsed,
                    cash            = cash,
                    dapur_lv        = dapur_lv,
                    makan_lv        = makan_lv,
                    kasir_lv        = kasir_lv,
                    total_earned    = total_earned,
                    upgrades_bought = upgrades_bought,
                    bottleneck      = bottleneck_label(dapur_lv, makan_lv, kasir_lv),
                    rps_at_end      = rps,
                )

    # Final snapshot
    final = SimResult(
        location_name   = loc["name"],
        time_elapsed_s  = elapsed,
        cash            = cash,
        dapur_lv        = dapur_lv,
        makan_lv        = makan_lv,
        kasir_lv        = kasir_lv,
        total_earned    = total_earned,
        upgrades_bought = upgrades_bought,
        bottleneck      = bottleneck_label(dapur_lv, makan_lv, kasir_lv),
        rps_at_end      = rps,
    )

    out = []
    for label, cp in [("1 Jam", checkpoints[3600]),
                      ("24 Jam", checkpoints[86400]),
                      ("7 Hari", checkpoints[604800])]:
        if cp:
            out.append((label, cp))
    out.append(("AKHIR SIM", final))

    return out


# ─────────────────────────────────────────────────────────────────────────────
#  REPORT GENERATOR
# ─────────────────────────────────────────────────────────────────────────────

def print_report(results, strategy):
    print(f"\n{'═'*72}")
    print(f"  LAPORAN SIMULASI ECONOMY — InResto v6.0")
    print(f"  Strategi: {strategy.upper()}")
    print(f"{'═'*72}")
    print(f"  {'Checkpoint':<12} {'Lokasi':<28} {'Saldo':<12} {'RPS':<12} {'D/M/K Lv':<14} {'Bottleneck'}")
    print(f"  {'-'*68}")

    for label, r in results:
        lvs = f"{r.dapur_lv}/{r.makan_lv}/{r.kasir_lv}"
        print(f"  {label:<12} {r.location_name:<28} {format_rupiah(r.cash):<12} "
              f"{format_rupiah(r.rps_at_end)+'/s':<12} {lvs:<14} {r.bottleneck}")

    print(f"{'═'*72}\n")

    # Cek paywall warnings
    print("  ⚠  PAYWALL / TIMEGATE ANALYSIS:")
    for label, r in results:
        # Warning: jika setelah 24 jam masih di lokasi 1
        if label == "24 Jam" and "Gang Sempit" in r.location_name:
            print(f"  🔴 KRITIS: Setelah 24 jam masih di lokasi 1! → Turunkan expand cost atau percepat early RPS")
        # Warning: jika setelah 7 hari masih di < lokasi 5
        if label == "7 Hari" and r.location_name in [l["name"] for l in LOCATIONS[:4]]:
            print(f"  🟡 PERINGATAN: Setelah 7 hari baru di '{r.location_name}' → Progres terlalu lambat untuk retensi")
        # Good path
        if label == "7 Hari" and r.location_name in [l["name"] for l in LOCATIONS[5:]]:
            print(f"  🟢 OK: Setelah 7 hari sudah di '{r.location_name}' — progres sehat")

    print()


def print_ascii_chart(results):
    """Bar chart ASCII sederhana untuk RPS di tiap checkpoint."""
    print("  RPS PROGRESSION CHART:")
    vals = [(label, r.rps_at_end) for label, r in results]
    max_rps = max(r for _, r in vals) or 1
    bar_max = 40

    for label, rps in vals:
        bar_len = int((rps / max_rps) * bar_max)
        bar = "█" * bar_len
        print(f"  {label:<12} |{bar:<{bar_max}}| {format_rupiah(rps)}/s")
    print()


def compare_strategies():
    """Jalankan kedua strategi dan bandingkan hasil 7 hari."""
    print(f"\n{'═'*72}")
    print(f"  STRATEGY COMPARISON — Balanced vs Greedy (7 hari)")
    print(f"{'═'*72}")

    for strat in ["balanced", "greedy"]:
        results = simulate(strategy=strat, simulate_hours=168)
        final_label, final = results[-1]
        prev_label, checkpoint_7d = results[-2] if len(results) >= 2 else (None, final)
        r = checkpoint_7d

        print(f"\n  [{strat.upper()}]")
        print(f"    Lokasi: {r.location_name}")
        print(f"    Pilar:  Dapur Lv{r.dapur_lv} | Area Makan Lv{r.makan_lv} | Kasir Lv{r.kasir_lv}")
        print(f"    Total Earned: {format_rupiah(r.total_earned)}")
        print(f"    RPS Akhir:    {format_rupiah(r.rps_at_end)}/s")
        print(f"    Upgrades:     {r.upgrades_bought}x")

    print(f"\n  KESIMPULAN: Strategi Balanced seharusnya menghasilkan RPS lebih tinggi")
    print(f"  karena meminimalkan bottleneck. Selisih besar = balance game sudah baik.\n")


# ─────────────────────────────────────────────────────────────────────────────
#  MAIN
# ─────────────────────────────────────────────────────────────────────────────

def main():
    parser = argparse.ArgumentParser(description="InResto Economy Balancing Simulator")
    parser.add_argument("--hours",    type=float, default=168,       help="Simulasi berapa jam (default: 168 = 7 hari)")
    parser.add_argument("--location", type=int,   default=0,         help="Mulai dari lokasi index 0-9 (default: 0)")
    parser.add_argument("--strategy", type=str,   default="balanced",help="balanced / greedy")
    parser.add_argument("--compare",  action="store_true",           help="Bandingkan semua strategi")
    parser.add_argument("--verbose",  action="store_true",           help="Print detail setiap pindah lokasi")
    args = parser.parse_args()

    if args.compare:
        compare_strategies()
        return

    results = simulate(
        start_location_idx = max(0, min(args.location, len(LOCATIONS) - 1)),
        simulate_hours     = args.hours,
        strategy           = args.strategy,
        verbose            = args.verbose,
    )

    print_report(results, args.strategy)
    print_ascii_chart(results)

    # Saran otomatis
    print("  SARAN UNTUK DEVELOPER:")
    for label, r in results:
        if label == "1 Jam":
            if r.location_name == LOCATIONS[0]["name"]:
                print(f"  • Dalam 1 jam masih di Lokasi 1 — cek apakah early RPS (Rp{format_rupiah(r.rps_at_end)}/s) cukup engaging")
            else:
                print(f"  • Dalam 1 jam sudah pindah ke '{r.location_name}' — early game feels fast! ✓")


if __name__ == "__main__":
    main()
