# MASTER PLAN — InResto: Restaurant Tycoon
## Roadmap Development Lengkap (Unity + .gemini.d Protocols)

**Versi:** 1.0 | **Tanggal:** 2026-04-17 | **Status:** Active Development  
**GDD Ref:** v6.0 | **Stack:** Unity (C#) + SQLite + DOTween

---

## LEGEND STATUS
- ✅ Selesai & verified
- 🔄 In Progress
- ⏳ Pending
- 🔴 Critical / Blocker
- 🟡 Important
- 🟢 Nice to have

---

## FASE 0 — FOUNDATION FIXES (Sesi Ini) ✅ SELESAI

> Perbaikan critical bugs + missing features di scripts yang sudah ada.
> Tidak ada fitur baru, hanya menstabilkan fondasi.

| # | Task | Status | File |
|---|------|--------|------|
| F0.1 | Tambah `BuyUpgrade()`, `GetFormattedBalance()`, `FormatNumber()` | ✅ | EconomyManager.cs |
| F0.2 | Fix hardcoded `locationId=1` di PillarBase | ✅ | PillarBase.cs |
| F0.3 | Tambah `SetLevel()`, `IsMaxed()`, `ResetForNewLocation()` | ✅ | PillarBase.cs |
| F0.4 | Hapus `Update()` per-frame di VisualManager | ✅ | VisualManager.cs |
| F0.5 | Event-driven bottleneck UI + warna severity | ✅ | VisualManager.cs |
| F0.6 | Hapus `Update()` per-frame di PillarUpgradeSlot | ✅ | PillarUpgradeSlot.cs |
| F0.7 | Subscribe `OnBalanceChanged` + `OnUpgradePerformed` | ✅ | PillarUpgradeSlot.cs |
| F0.8 | `GetBottleneckStatus(out BottleneckSeverity)` + ratio-based | ✅ | PillarManager.cs |
| F0.9 | `OfflineIncomeManager.cs` — kalkulator offline earnings | ✅ | Scripts/Core/ |
| F0.10 | `SaveLastLoginTime()` + `GetLastLoginTime()` | ✅ | SaveManager.cs |
| F0.11 | `LoadPillarLevel()` + `LoadAllPillarsForLocation()` | ✅ | SaveManager.cs |
| F0.12 | Parameterized queries (anti SQL injection) | ✅ | SaveManager.cs |
| F0.13 | Load pilar dari DB saat `InitializeSequence()` | ✅ | GameManager.cs |
| F0.14 | `OfflineIncomeManager` di GameBootstrapper | ✅ | GameBootstrapper.cs |
| F0.15 | `balancing_sim.py` — economy simulator Python | ✅ | Root project |

**🔴 TEMUAN KRITIS dari balancing_sim.py:**
> Area Makan selalu jadi bottleneck ekstrem (Lv133 saat Dapur Lv27).  
> Penyebab: formula scaling-nya linear (`+1 kursi/level`) vs Dapur/Kasir yang proportional.  
> **Wajib diperbaiki di Fase 1 sebelum game bisa diplay dengan balance yang benar.**

---

## FASE 1 — ECONOMY BALANCING & CORE MECHANICS ⏳

> Semua sistem game loop harus terbukti benar secara matematis sebelum lanjut ke visual.
> Panduan: `.gemini.d/game_dev.md §1,3,4,5`

### 1.1 Area Makan Scaling Fix 🔴
```
File: MakanController.cs
Problem: baseCapacity + (level-1) → terlalu lambat, perlu exponential
Fix: currentCapacity = baseCapacity * Mathf.Pow(1.1f, currentLevel - 1)
      lalu update balancing_sim.py dengan formula baru dan re-run
```

### 1.2 DapurController — Formula Alignment 🟡
- Verifikasi formula `baseProductionPerSecond * currentLevel` sesuai GDD §A.2
- Tambahkan sub-upgrade: `Kualitas Bahan` (revenue multiplier) + `Skill Koki` (cook time)

### 1.3 KasirController — QRIS/Payment Methods 🟡
- Tambahkan sub-upgrade: `Kecepatan Layanan` + `Sistem Pembayaran`
- `Sistem Pembayaran` tier: Cash → Transfer → QRIS → Contactless → Auto-clear

### 1.4 Rush Hour System (Tap Mechanic) 🟡
```
File baru: RushHourManager.cs
- EnergyBar (0-100%), +2% per tap
- Saat 100% → RushHour Mode (10 detik):
    - Dapur 5x speed
    - Kasir instant
    - Revenue pop-up besar
- Cooldown: 0 (bisa langsung isi ulang)
- Haptic + screen shake + orange glow
```

### 1.5 Manager System (Ojan, Rin, Dani) 🟡
```
File baru: ManagerBase.cs, OjanManager.cs, RinManager.cs, DaniManager.cs
- Unlock via ProgressionManager milestone
- Active skill + cooldown
- Passive bonus permanent
- Manager UI panel (3 slot dengan portrait + cooldown timer)
```

### 1.6 Offline Income Claim UI 🟡
```
File baru: OfflineClaimUI.cs
- Popup saat game dibuka dengan HasPendingClaim = true
- Animasi: brankas buka → uang jatuh → counter naik
- Dua tombol: KLAIM 1x | KLAIM 2x (Watch Ad)
- Dipicu dari OfflineIncomeManager.HasPendingClaim
```

### 1.7 Re-run balancing_sim.py setelah semua fixes ✅ per milestone
```
python balancing_sim.py --compare
python balancing_sim.py --hours 24 --verbose
Target: 
- 1 jam → setidaknya Lokasi 3 (Warung Rakyat)
- 24 jam → setidaknya Lokasi 5 (Restoran Sederhana)
- 7 hari → Lokasi 8-9 (Franchise/Tokyo)
```

---

## FASE 2 — STORY SYSTEM (Visual Novel) ⏳

> 54 panel VN yang jadi USP utama game ini.
> Panduan: GDD §7 | `.gemini.d/game_dev.md §2`

### 2.1 VNPanel System Refactor 🟡
```
File: VNBridge.cs (upgrade besar)
- Ganti switch-case sederhana dengan ScriptableObject per chapter
- VNChapter.cs (ScriptableObject): array of VNDialog { speaker, text, bgSprite, charSprite }
- VNPanelData asset per panel (0.1 sampai 7.9)
- Trigger: milestone-based (revenue / level / location / time)
```

### 2.2 Chapter Data (54 Panel) 🟡
```
Folder: Assets/Data/VNChapters/
Format: VNChapter_0_1.asset, VNChapter_1_1.asset, dst.
Isi awal: Act 0 (3 panel) dan Act 1 (11 panel) — total 14 panel untuk MVP
```

### 2.3 Story Trigger System 🟡
```
File baru: StoryTriggerManager.cs
- Subscribe ke EconomyManager.LifetimeEarnings (revenue-based trigger)
- Subscribe ke ProgressionManager.onLocationSwitched
- Daftar trigger: [panelId, triggerType, value]
  Contoh: { "1.11", TriggerType.Revenue, 1_000_000 }
           { "4.1",  TriggerType.Location, 4 }
```

### 2.4 Story Archive UI 🟢
```
File baru: StoryArchiveUI.cs
- Grid semua 54 panel (locked/unlocked state)
- Thumbnail per panel
- Tap → replay panel
- Accessible dari main menu
```

---

## FASE 3 — EXPANSION & PRESTIGE ⏳

> Sistem pindah lokasi yang memberikan prestige multiplier.
> Panduan: GDD §6 | ProgressionManager.cs sudah ada pondasinya.

### 3.1 ProgressionManager Upgrade 🟡
- Fix: `AddPrestigeMultiplier(1.5f)` terlalu besar → align ke GDD (+10% per lokasi)
- Tambah UI "Expand" button yang muncul saat semua pilar max
- Animasi pindah lokasi (screen fade + background transition)
- `SaveManager.SaveLocationIndex()` — persist lokasi saat ini

### 3.2 LocationData Population 🟡
```
Assets/Data/Locations/ → buat 10 ScriptableObject LocationData
Setiap data isi: locationID, name, maxLevel per pilar, unlockCost, revenueMultiplier
```

### 3.3 Sertifikat Cabang UI 🟢
- Visual token yang ditampilkan di main UI
- Setiap token = +10% revenue badge

---

## FASE 4 — VISUAL POLISH & ANIMATION ⏳

> Juiciness dari GDD §10.4 dan .gemini.d §2

### 4.1 Sprite System 🟡
```
PillarBase.UpdateVisuals() sudah ada, perlu:
- Set sprite per milestone: Lv1-10, 11-25, 26-50, 51-100
- Animasi sprite bergerak (cook motion, staff walking, kasir scanning)
- Placeholder: SVG-generated cartoon sprites (sesuai .gemini.d §2 — no plain div placeholders!)
```

### 4.2 Customer Sprite System 🟡
```
File baru: CustomerManager.cs
- Spawn customer sprites di area makan (jumlah = currentCapacity)
- Customer tap → chat bubble muncul (feedback bottleneck)
- Customer leave animation jika antrean penuh
```

### 4.3 Revenue Pop-ups 🟢
```
File baru: MoneyPopup.cs
- Floating "+Rp X" text saat customer bayar
- Coin particle dari customer ke cash display
- Skala pop-up bertambah saat Rush Hour aktif
```

### 4.4 DOTween Audit 🟡
- Pastikan DOTween sudah ter-install di project (VNBridge + PillarUpgradeSlot sudah pakai)
- Review semua `DOPunchScale` dan `DOFade` — cegah memory leak (DOTween Kill on disable)

---

## FASE 5 — MONETIZATION ⏳

> Ethical F2P sesuai GDD §8 | .gemini.d/security.md

### 5.1 Ad Manager (AdMob) 🟡
```
File baru: AdManager.cs
- Rewarded video: OfflineClaim 2x | Rush Hour refill | Manager cooldown skip
- Frequency caps: max 5x/hari claim, 3x/jam rush, 2x/skill/hari
- Fallback: jika no-fill, berikan bonus 1.5x (bukan 2x)
```

### 5.2 Premium Unlock (IAP) 🟡
```
File baru: PremiumManager.cs
- Unity IAP integration
- Unlock: semua VN panel + no ads + 2x offline permanent
- Price: Rp 49.000 (sesuai GDD §8.2)
- Restore purchase flow
```

### 5.3 Cosmetic Shop 🟢
- Character skin system
- Location theme (night mode, rain ambience)
- Purely cosmetic, no gameplay advantage

---

## FASE 6 — TESTING & QA ⏳

> .gemini.d/ops_architecture.md §1 | game_dev.md §4,5

### 6.1 Economy QA — Time Travel Test 🟡
```
OfflineIncomeManager.useMockTime = true (sudah ada!)
Test cases:
- 1 menit offline → tidak muncul klaim UI
- 1 jam offline → klaim muncul dengan amount benar
- 8 hari offline → klaim di-cap 7 hari (tidak lebih)
```

### 6.2 Anti-Cheat Validation 🟡
```
File baru: AntiCheatValidator.cs
- Deteksi RPS anomali: jika income/tick > theoretical max → flag
- Deteksi saldo negatif: EconomyManager sudah ada invariant guard ✓
- Timestamp validation untuk offline income
```

### 6.3 Save Data Stress Test 🟡
- Simulasi crash mid-save (interrupt SQLite write)
- `ResetSave()` context menu sudah ada di SaveManager ✓
- Backup save system: `SaveManager.CreateBackup()` (TODO)

### 6.4 Performance Profiling 🟡
- Target: < 5% CPU, < 5% baterai/jam (GDD §9.3)
- Check: tidak ada `Update()` baru yang ditambahkan tanpa review
- Event-driven architecture audit

---

## FASE 7 — LAUNCH PREP ⏳

### 7.1 Android Build
- Target Android 8.0+ (API 26)
- APK size target: < 50 MB
- Proguard/R8 minification

### 7.2 Google Play Store
- Store listing: screenshot, deskripsi, rating PEGI 3
- Privacy policy (wajib untuk AdMob)
- UU PDP Indonesia compliance (.gemini.d/security.md §6,7)

---

## DEPENDENCY MAP

```
SaveManager
    ↓
EconomyManager ← OnBalanceChanged → [VisualManager, PillarUpgradeSlot]
    ↓                               
PillarManager → ProcessTick() → EconomyManager.AddMoney()
    ↓
PillarBase (Dapur, Makan, Kasir)
    ↓
ProgressionManager → TriggerVisualNovel() → VNBridge
    ↓
OfflineIncomeManager → EconomyManager.AddMoney()
    ↓
GameManager (conductor — init semua, jalankan loop)
    ↑
GameBootstrapper (scene entry point)
```

---

## BALANCING TARGETS (Hasil Simulasi + Rekomendasi)

| Checkpoint | Target Lokasi | Target RPS | Status |
|-----------|--------------|------------|--------|
| 1 jam     | Lokasi 3-4   | Rp 5-50M/s | 🔴 Area Makan overscaled |
| 24 jam    | Lokasi 5-6   | Rp 100-300M/s | 🔴 Area Makan overscaled |
| 7 hari    | Lokasi 8-9   | Rp 300M-1B/s | ✅ Sehat |

**Immediate fix needed:** `MakanController.UpdateUpgradeCost()` — ganti formula dari linear ke exponential.

---

## NEXT STEPS (Prioritas Sesi Berikutnya)

1. **[CRITICAL]** Fix MakanController scaling → re-run balancing_sim.py → verify balance
2. **[HIGH]** Implement RushHourManager.cs (tap mechanic, energy bar)
3. **[HIGH]** Implement OfflineClaimUI.cs (animasi brankas)
4. **[HIGH]** Populate LocationData ScriptableObjects (10 lokasi)
5. **[MEDIUM]** VNBridge upgrade → ScriptableObject-based chapter system
6. **[MEDIUM]** CustomerManager.cs (sprite + tap-to-chat bottleneck hint)

---

*MASTER_PLAN.md — InResto v1.0 | Last updated: 2026-04-17*  
*Generated after full codebase audit + balancing simulation run*
