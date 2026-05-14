using UnityEngine;

namespace InUniverse.InResto
{
    /// <summary>
    /// Pilar 2: Area Makan (Kapasitas).
    /// Menentukan berapa banyak pelanggan yang bisa duduk sekaligus.
    ///
    /// CHANGELOG v2 — CRITICAL FIX (dari balancing_sim.py):
    /// - BUG: Formula lama linear (+1 kursi/level) → Area Makan butuh Lv133 agar balance
    ///        dengan Dapur Lv27. Ini karena production rate Dapur & Kasir proportional,
    ///        sementara Makan linear.
    /// - FIX: Capacity sekarang exponential (×1.15 per level), sama dengan pilar lain.
    ///        Setelah fix, balance target: D/M/K ratio seharusnya ≈ 1:1:1 saat optimal.
    /// - Tambah GetCapacityRate() — nilai yang langsung bisa dibandingkan dengan pilar lain
    ///   di PillarManager.ProcessTick() (tidak perlu bagi eating duration di luar)
    /// - Tambah sub-upgrade: Kenyamanan (tip multiplier) dan Kebersihan (complaint reduction)
    ///   sesuai GDD §3.1 Pilar 2
    /// </summary>
    public class MakanController : PillarBase
    {
        [Header("Makan Specific")]
        [SerializeField] private double baseCapacityRate = 0.9; // Sedikit di bawah Dapur (1.0) agar seimbang di level awal

        [Header("Sub-Upgrades (GDD §3.1)")]
        [SerializeField] private int kenyamananLevel   = 0; // +2% tip per level
        [SerializeField] private int kebersihanLevel   = 0; // -1% complaint per level

        // ─── Internal state ───
        private double currentCapacityRate;  // setara: porsi yang bisa ditampung per detik
        private int    currentCapacitySeat;  // jumlah kursi (untuk display UI saja)

        // ─── GDD §3.1: "Kenyamanan" memberi tip bonus ───
        public float GetTipMultiplier()    => 1f + (kenyamananLevel * 0.02f);
        // GDD §3.1: "Kebersihan" kurangi complaint ───
        public float GetComplaintReduction() => kenyamananLevel * 0.01f;

        // ─────────────────────────────────────────────
        protected override void Start()
        {
            pillarName = "Area Makan";
            type       = PillarType.AreaMakan;
            base.Start();
        }

        protected override void UpdateUpgradeCost()
        {
            // Harga: sama formula dengan pilar lain tapi base lebih tinggi (kursi = mahal)
            currentUpgradeCost = 12000.0 * System.Math.Pow(1.18, currentLevel - 1);

            // FIX: Exponential growth sama seperti Dapur & Kasir
            // baseCapacityRate × 1.15^(level-1)  →  di level 20: ≈ 16.4 (vs Dapur 20.0, Kasir 16.0)
            // Artinya pada level max lokasi 1 (Lv20) ketiga pilar hampir seimbang ✓
            currentCapacityRate = baseCapacityRate * System.Math.Pow(1.15, currentLevel - 1);

            // Kursi fisik untuk display (visual saja, tidak mempengaruhi RPS)
            // GDD §3.1: Lv1-10 = 3 kursi, 11-25 = 6, 26-50 = 12, 51-100 = 24
            currentCapacitySeat = currentLevel switch
            {
                int l when l <= 10  => 3,
                int l when l <= 25  => 6,
                int l when l <= 50  => 12,
                int l when l <= 100 => 24,
                _                   => 24 + ((currentLevel - 100) / 10)
            };
        }

        // ─────────────────────────────────────────────
        //  Public Accessors
        // ─────────────────────────────────────────────

        /// <summary>
        /// Rate yang digunakan PillarManager untuk bottleneck calculation.
        /// Satuan: porsi-ekivalen per detik (bisa dibandingkan langsung dengan Dapur/Kasir).
        /// </summary>
        public double GetCapacityRate() => currentCapacityRate;

        /// <summary>Jumlah kursi fisik — untuk display UI dan customer spawn count.</summary>
        public int GetCapacity() => currentCapacitySeat;

        /// <summary>
        /// Eating duration: makin tinggi level, makin cepat meja berganti (turnover).
        /// GDD: "Kenyamanan meningkatkan tip, Kebersihan kurangi complaint."
        /// Ini berbeda dari rate — ini adalah lama duduk per customer dalam detik.
        /// </summary>
        public float GetEatingDuration()
        {
            // Level 1: 5 detik, turun 0.02 detik per level, minimum 2 detik
            return Mathf.Max(2.0f, 5.0f - (currentLevel * 0.02f));
        }
    }
}
