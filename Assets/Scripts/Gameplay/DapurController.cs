using UnityEngine;

namespace InUniverse.InResto
{
    /// <summary>
    /// Pilar 1: Dapur (Produksi).
    /// Menentukan berapa banyak porsi makanan yang bisa dimasak per detik.
    /// </summary>
    public class DapurController : PillarBase
    {
        [Header("Dapur Specific")]
        public double baseProductionPerSecond = 1.0;
        
        private double currentProduction;

        protected override void Start()
        {
            pillarName = "Dapur";
            type = PillarType.Dapur;
            base.Start();
        }

        protected override void UpdateUpgradeCost()
        {
            // Harga naik secara eksponensial: base * 1.15^level.
            // MVP balance: puluhan ribu di awal supaya progress terasa naik, bukan instan.
            currentUpgradeCost = 10000 * Mathf.Pow(1.15f, currentLevel - 1);
            currentProduction = baseProductionPerSecond * currentLevel;
        }

        public double GetProductionRate()
        {
            return currentProduction;
        }

        // Dipanggil setiap Tick oleh GameManager
        public double ProcessTick()
        {
            // Menghasilkan porsi makanan
            return currentProduction;
        }
    }
}
