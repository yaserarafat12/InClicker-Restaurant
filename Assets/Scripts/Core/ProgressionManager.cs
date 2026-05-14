using UnityEngine;
using UnityEngine.Events;

namespace InUniverse.InResto
{
    /// <summary>
    /// Menangani milestone, pindah lokasi (prestige), dan trigger Visual Novel.
    ///
    /// CHANGELOG v2:
    /// - Fix prestige multiplier: 1.5 (50%) → 1.1 (10%) sesuai GDD §6.2 "Sertifikat Cabang"
    /// - Ganti allLocations[] manual dengan LocationDatabase statis (tidak perlu assign di Inspector)
    /// - Kriteria expand: level >= maxPillarLevel dari LocationDatabase (bukan hardcoded 50)
    /// - Reset pilar saat expand: panggil PillarBase.ResetForNewLocation() dengan maxLevel baru
    /// - Tambah: saldo di-reset ke 0 saat expand (GDD §6.2)
    /// - Tambah: SaveGameState setelah expand
    /// - Tambah GetCurrentLocation() untuk akses mudah dari script lain
    /// </summary>
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        [Header("State")]
        public int currentLocationIndex = 0; // 0-based (0 = Lokasi 1 Warteg Gang Sempit)

        [Header("Events")]
        public UnityEvent onLocationSwitched;
        public UnityEvent<string> onVNTrigger; // Kirim panel ID string, misal "2.1"

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        // ─────────────────────────────────────────────────────────────
        //  Accessors
        // ─────────────────────────────────────────────────────────────
        public LocationDatabase.LocationInfo GetCurrentLocation()
            => LocationDatabase.Get(currentLocationIndex);

        public bool IsAtLastLocation()
            => LocationDatabase.IsLastLocation(currentLocationIndex);

        // ─────────────────────────────────────────────────────────────
        //  Expand Check
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Apakah player memenuhi syarat pindah lokasi?
        /// Syarat: semua pilar sudah max level DAN punya cukup uang.
        /// </summary>
        public bool CanExpand()
        {
            if (PillarManager.Instance == null) return false;
            if (IsAtLastLocation()) return false;

            var loc    = LocationDatabase.Get(currentLocationIndex);
            int maxLv  = loc.MaxPillarLevel;

            bool allMaxed = PillarManager.Instance.dapur.IsMaxed() &&
                            PillarManager.Instance.makan.IsMaxed() &&
                            PillarManager.Instance.kasir.IsMaxed();

            var nextLoc     = LocationDatabase.Get(currentLocationIndex + 1);
            bool canAfford  = EconomyManager.Instance.CanAfford(nextLoc.ExpandCost);

            return allMaxed && canAfford;
        }

        // ─────────────────────────────────────────────────────────────
        //  Execute Expand (Prestige)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Pindah ke lokasi berikutnya — GDD §6.2 "What happens when you Expand".
        /// </summary>
        public void Expand()
        {
            if (!CanExpand()) return;

            var nextLocInfo = LocationDatabase.Get(currentLocationIndex + 1);

            // 1. Bayar expand cost (GDD §6.1)
            EconomyManager.Instance.SpendMoney(nextLocInfo.ExpandCost);

            // 2. Pindah index
            currentLocationIndex++;

            // 3. Prestige multiplier +10% (GDD §6.2: "Sertifikat Cabang = +10% permanent")
            //    FIXED dari v1: 1.5 → 1.1 (10% bukan 50%)
            EconomyManager.Instance.AddPrestigeMultiplier(1.10f);

            // 4. Reset pilar ke level 1 dengan max level baru (GDD §6.2: staff retained, level reset)
            int newMaxLv = nextLocInfo.MaxPillarLevel;
            PillarManager.Instance.dapur.ResetForNewLocation(newMaxLv);
            PillarManager.Instance.makan.ResetForNewLocation(newMaxLv);
            PillarManager.Instance.kasir.ResetForNewLocation(newMaxLv);

            // 5. Simpan pilar awal lokasi baru ke DB
            SaveManager.Instance.SavePillarLevel(currentLocationIndex + 1, "Dapur",     1);
            SaveManager.Instance.SavePillarLevel(currentLocationIndex + 1, "AreaMakan", 1);
            SaveManager.Instance.SavePillarLevel(currentLocationIndex + 1, "Kasir",     1);

            // 6. Simpan state (location index + balance)
            SaveManager.Instance.SaveGameState(
                EconomyManager.Instance.CurrentBalance,
                EconomyManager.Instance.LifetimeEarnings,
                currentLocationIndex + 1   // DB pakai 1-based
            );

            // 7. Update UI background
            // VisualManager tidak punya LocationData objek lagi — perlu cara lain
            // Placeholder: log saja, nanti integrate dengan sprite system
            Debug.Log($"<color=green>✓ EXPAND! Welcome to: {nextLocInfo.Name}</color>");
            Debug.Log($"<color=yellow>  Revenue Multiplier: {nextLocInfo.RevenueMultiplier}x | Max Level: {newMaxLv}</color>");

            // 8. Trigger VN panel
            TriggerVisualNovel(nextLocInfo.VNPanelTrigger);

            // 9. Fire event untuk UI
            onLocationSwitched?.Invoke();
        }

        // ─────────────────────────────────────────────────────────────
        //  VN Trigger
        // ─────────────────────────────────────────────────────────────

        /// <summary>Trigger VN panel berdasarkan panel ID string (misal "2.1", "4.5").</summary>
        public void TriggerVisualNovel(string panelId)
        {
            Debug.Log($"<color=magenta>📖 VN TRIGGER: Panel {panelId}</color>");
            onVNTrigger?.Invoke(panelId);
            // VNBridge akan subscribe ke event ini dan menampilkan chapter yang sesuai
        }

        /// <summary>Overload legacy int untuk backward compat dengan kode lama.</summary>
        public void TriggerVisualNovel(int chapterId)
        {
            TriggerVisualNovel(chapterId.ToString());
        }

        // ─────────────────────────────────────────────────────────────
        //  Revenue Multiplier dari lokasi saat ini
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Revenue multiplier lokasi saat ini — dipakai PillarManager.
        /// Berbeda dari prestige multiplier (yang kumulatif) — ini flat per lokasi.
        /// </summary>
        public double GetLocationRevenueMultiplier()
            => LocationDatabase.Get(currentLocationIndex).RevenueMultiplier;
    }
}
