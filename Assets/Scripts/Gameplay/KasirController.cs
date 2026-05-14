using UnityEngine;

namespace InUniverse.InResto
{
    /// <summary>
    /// Pilar 3: Kasir (Distribusi/Transaksi).
    /// Menentukan berapa banyak porsi yang bisa dibayar (dikonversi jadi uang) per detik.
    /// </summary>
    public class KasirController : PillarBase
    {
        [Header("Kasir Specific")]
        public double baseProcessingRate = 0.8; // Sedikit lebih lambat dari dapur di awal
        public double pricePerDish = 15000; // Harga makanan standar (Rupiah)
        
        private double currentProcessingRate;

        protected override void Start()
        {
            pillarName = "Kasir";
            type = PillarType.Kasir;
            base.Start();
        }

        protected override void UpdateUpgradeCost()
        {
            currentUpgradeCost = 11000 * Mathf.Pow(1.16f, currentLevel - 1);
            currentProcessingRate = baseProcessingRate * currentLevel;
        }

        public double GetProcessingRate()
        {
            return currentProcessingRate;
        }

        // Mengonversi porsi menjadi uang
        // Input: porsi yang tersedia dari dapur & kursi yang terisi
        public double ProcessSales(double availableDishes)
        {
            // Sales dibatasi oleh 2 hal: 
            // 1. Berapa yang bisa diproses Kasir
            // 2. Berapa makanan yang sudah jadi (Dapur)
            double salesToProcess = Mathf.Min((float)currentProcessingRate, (float)availableDishes);
            
            double revenue = salesToProcess * pricePerDish;
            return revenue;
        }
    }
}
