using UnityEngine;
using System.Collections.Generic;
using System;

namespace InUniverse.InResto
{
    /// <summary>
    /// Jantung finansial game. Menangani saldo, penambahan uang, dan pemformatan teks.
    /// Menggunakan 'double' untuk mendukung angka hingga 1e308 (Tycoon-ready).
    ///
    /// CHANGELOG v2:
    /// - Tambah BuyUpgrade() — alias SpendMoney() dengan return bool (dipakai PillarBase)
    /// - Tambah GetFormattedBalance() — format saldo saat ini (dipakai VisualManager)
    /// - Tambah FormatNumber()  — alias formatter untuk angka arbitrari (dipakai PillarUpgradeSlot)
    /// - Tambah CanAfford()     — cek saldo tanpa mutasi (helper UI)
    /// - Tambah OnUpgradePerformed event — dipancarkan tiap upgrade sukses agar UI segar
    /// - Fix: Invariant guard — saldo tidak pernah turun ke negatif
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Starting Balance")]
        [SerializeField] private double startingMoney = 200000; // Modal awal Rp 200.000

        public double CurrentBalance { get; private set; }
        public double LifetimeEarnings { get; private set; }
        private double prestigeMultiplier = 1.0;

        // --- Events ---
        // Dipancarkan setiap kali saldo berubah; kirimkan nilai baru sebagai argumen.
        public static event Action<double> OnBalanceChanged;
        // Dipancarkan khusus saat upgrade berhasil dibeli (UI slot perlu tahu level naik).
        public static event Action OnUpgradePerformed;

        public void AddPrestigeMultiplier(float bonus)
        {
            prestigeMultiplier *= bonus;
        }

        // ─────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // ─────────────────────────────────────────────
        //  Initialization
        // ─────────────────────────────────────────────
        public void Initialize(double savedBalance, double savedLifetime)
        {
            CurrentBalance = savedBalance > 0 ? savedBalance : startingMoney;
            LifetimeEarnings = savedLifetime;
            OnBalanceChanged?.Invoke(CurrentBalance);
        }

        // ─────────────────────────────────────────────
        //  Money Operations
        // ─────────────────────────────────────────────

        /// <summary>Tambah pemasukan (income dari pilar setiap tick).</summary>
        public void AddMoney(double amount)
        {
            if (amount <= 0) return;

            double finalAmount = amount * prestigeMultiplier;
            CurrentBalance += finalAmount;
            LifetimeEarnings += finalAmount;

            OnBalanceChanged?.Invoke(CurrentBalance);
        }

        /// <summary>
        /// Kurangi saldo untuk pengeluaran bebas.
        /// Mengembalikan true jika berhasil, false jika saldo tidak cukup.
        /// Invariant: saldo tidak pernah negatif.
        /// </summary>
        public bool SpendMoney(double amount)
        {
            if (amount <= 0) return true;
            if (CurrentBalance >= amount)
            {
                CurrentBalance -= amount;
                // Invariant guard
                if (CurrentBalance < 0) CurrentBalance = 0;
                OnBalanceChanged?.Invoke(CurrentBalance);
                return true;
            }

            Debug.Log("<color=red>InResto: Saldo tidak cukup!</color>");
            return false;
        }

        /// <summary>
        /// Alias SpendMoney() — digunakan oleh PillarBase.Upgrade().
        /// Memudahkan membaca niat kode ("beli upgrade" vs "habiskan uang").
        /// Juga memicu OnUpgradePerformed agar seluruh UI slot ter-refresh.
        /// </summary>
        public bool BuyUpgrade(double cost)
        {
            bool success = SpendMoney(cost);
            if (success)
            {
                OnUpgradePerformed?.Invoke();
            }
            return success;
        }

        // ─────────────────────────────────────────────
        //  Query / Helper
        // ─────────────────────────────────────────────

        /// <summary>Apakah saldo cukup untuk membayar sejumlah `amount`? (tidak ada mutasi)</summary>
        public bool CanAfford(double amount) => CurrentBalance >= amount;

        // ─────────────────────────────────────────────
        //  Formatting
        // ─────────────────────────────────────────────

        /// <summary>
        /// Format saldo saat ini menjadi string yang human-readable.
        /// Dipakai oleh VisualManager.
        /// </summary>
        public string GetFormattedBalance() => GetFormattedMoney(CurrentBalance);

        /// <summary>
        /// Alias GetFormattedMoney() untuk konsistensi nama yang diharapkan UI slot.
        /// </summary>
        public string FormatNumber(double amount) => GetFormattedMoney(amount);

        /// <summary>
        /// Mengubah angka besar menjadi format terbaca (K, M, B, T, Qa…)
        /// Contoh: 1.500.000 -> Rp 1.5M | 200.000 -> Rp 200.000
        /// Mendukung hingga 1e36 (lebih dari cukup untuk World Domination tier).
        /// </summary>
        public string GetFormattedMoney(double amount)
        {
            if (double.IsNaN(amount) || double.IsInfinity(amount)) return "Rp ???";
            if (amount < 1000) return $"Rp {amount:N0}";

            string[] suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };
            int i = 0;
            double val = amount;

            while (val >= 1000 && i < suffixes.Length - 1)
            {
                val /= 1000;
                i++;
            }

            return $"Rp {val:N2}{suffixes[i]}";
        }
    }
}
