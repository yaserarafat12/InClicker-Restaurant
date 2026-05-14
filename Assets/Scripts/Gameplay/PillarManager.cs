using UnityEngine;

namespace InUniverse.InResto
{
    /// <summary>
    /// Mengelola hubungan antara Dapur, Makan, dan Kasir.
    /// Menghitung total pendapatan per detik berdasarkan bottleneck terkecil.
    /// </summary>
    public class PillarManager : MonoBehaviour
    {
        public static PillarManager Instance { get; private set; }

        [Header("References")]
        public DapurController dapur;
        public MakanController makan;
        public KasirController kasir;

        [Header("Realtime Stats")]
        [SerializeField] private double currentIncomePerSecond;

        /// <summary>Dipanggil OfflineIncomeManager untuk hitung offline earnings.</summary>
        public double GetRevenuePerSecond() => currentIncomePerSecond;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            AutoFindPillars();
        }

        public void AutoFindPillars()
        {
            if (dapur == null) dapur = FindObjectOfType<DapurController>();
            if (makan == null) makan = FindObjectOfType<MakanController>();
            if (kasir == null) kasir = FindObjectOfType<KasirController>();
            
            if (dapur != null && makan != null && kasir != null)
                Debug.Log("<color=green>PillarManager: All pillars found and connected!</color>");
        }

        public void ProcessTick()
        {
            if (dapur == null || makan == null || kasir == null) return;

            // 1. Ambil rate dari tiap pilar — semua dalam satuan yang sama (porsi-ekivalen/detik)
            double productionRate = dapur.GetProductionRate();      // Dapur: porsi masak/detik
            double capacityRate   = makan.GetCapacityRate();        // Makan: FIX — exponential, sudah dalam satuan rate
            double processingRate = kasir.GetProcessingRate();      // Kasir: transaksi/detik

            // 2. Bottleneck = pilar terkecil menentukan seluruh throughput
            double effectiveRate = System.Math.Min(productionRate, System.Math.Min(capacityRate, processingRate));

            // 3. Rush Hour multiplier (5x saat aktif — GDD §4.1)
            float rushMultiplier = RushHourManager.Instance != null
                ? RushHourManager.Instance.GetRushMultiplier
                : 1f;

            // 4. Revenue = throughput × harga × tip multiplier × rush multiplier
            double tipMultiplier = makan.GetTipMultiplier();
            double income        = effectiveRate * kasir.pricePerDish * tipMultiplier * rushMultiplier;
            currentIncomePerSecond = income;

            // 5. Location revenue multiplier (dari ProgressionManager, berbeda dgn prestige bonus)
            double locationMult = ProgressionManager.Instance != null
                ? ProgressionManager.Instance.GetLocationRevenueMultiplier()
                : 1.0;
            income *= locationMult;

            // 6. Tambahkan ke Economy (prestige multiplier diterapkan di dalam AddMoney)
            EconomyManager.Instance.AddMoney(income);
        }

        /// <summary>
        /// Mengembalikan string status bottleneck DAN severity enum untuk pewarnaan UI.
        /// Sesuai GDD §3.3: 🟢 Optimal | 🟡 Minor | 🔴 Critical
        /// </summary>
        public string GetBottleneckStatus(out BottleneckSeverity severity)
        {
            double p = dapur.GetProductionRate();
            double c = makan.GetCapacityRate();   // FIX: pakai GetCapacityRate() bukan GetCapacity()/5
            double k = kasir.GetProcessingRate();

            double minRate = System.Math.Min(p, System.Math.Min(c, k));
            double maxRate = System.Math.Max(p, System.Math.Max(c, k));

            // Hitung rasio imbalance: seberapa jauh pilar terkuat vs terlemah
            double imbalanceRatio = maxRate > 0 ? (maxRate - minRate) / maxRate : 0;

            if (imbalanceRatio < 0.15)
            {
                severity = BottleneckSeverity.Optimal;
                return "Semua lancar! 🟢";
            }
            else if (imbalanceRatio < 0.35)
            {
                severity = BottleneckSeverity.Minor;
                if (p <= c && p <= k) return "Dapur agak lambat 🟡";
                if (c <= p && c <= k) return "Meja mulai penuh 🟡";
                return "Kasir mulai padat 🟡";
            }
            else
            {
                severity = BottleneckSeverity.Critical;
                if (p <= c && p <= k) return "🔴 Dapur Kewalahan! Upgrade Dapur!";
                if (c <= p && c <= k) return "🔴 Gak Ada Tempat Duduk! Upgrade Area Makan!";
                return "🔴 Antrian Kasir Panjang! Upgrade Kasir!";
            }
        }

        /// <summary>Overload tanpa out parameter — untuk kode lama yang tidak butuh severity.</summary>
        public string GetBottleneckStatus()
        {
            return GetBottleneckStatus(out _);
        }
    }
}
