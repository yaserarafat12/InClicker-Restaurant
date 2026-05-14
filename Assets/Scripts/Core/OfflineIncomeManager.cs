using UnityEngine;
using System;

namespace InUniverse.InResto
{
    /// <summary>
    /// Sistem Offline Income — "Time Travel" dari GDD §4.2.
    ///
    /// Cara kerja:
    ///   1. Saat OnApplicationQuit / OnApplicationPause → simpan timestamp sekarang
    ///   2. Saat game dibuka lagi → hitung selisih waktu (maks 7 hari)
    ///   3. Kalikan RPS (Revenue Per Second) saat itu dengan selisih waktu
    ///   4. Tampilkan UI klaim (1x gratis / 2x dengan ad)
    ///
    /// .gemini.d — Game Dev Protocol §4: Time-Travel Testing
    ///   Sertakan MockTime untuk QA: bisa fast-forward 7 hari tanpa manipulasi OS.
    /// </summary>
    public class OfflineIncomeManager : MonoBehaviour
    {
        public static OfflineIncomeManager Instance { get; private set; }

        // Batas maksimum akumulasi offline: 7 hari (cegah exploit + dorong engagement mingguan)
        private const double MAX_OFFLINE_SECONDS = 604800.0; // 7 × 24 × 3600

        // Minimum selisih waktu agar UI klaim muncul (biar tidak muncul langsung setelah tutup)
        private const double MIN_OFFLINE_SECONDS = 60.0; // 1 menit

        // Hasil kalkulasi — diisi saat CalculateOfflineIncome() dipanggil
        public double PendingOfflineAmount  { get; private set; } = 0;
        public double OfflineSecondsElapsed { get; private set; } = 0;
        public bool   HasPendingClaim       { get; private set; } = false;

        // Flag: apakah player sudah klaim atau belum
        private bool _claimed = false;

        // ── Mock Time (QA / Testing) ──────────────────────────────────
        // Set ke true di editor untuk simulasi "buka setelah 7 hari"
        [Header("QA / Testing")]
        [SerializeField] private bool useMockTime = false;
        [SerializeField] private double mockOffsetSeconds = 86400; // default: 1 hari

        // ─────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            CalculateOfflineIncome();
        }

        private void OnApplicationQuit()   => SaveLastLoginTime();
        private void OnApplicationPause(bool paused) { if (paused) SaveLastLoginTime(); }

        // ─────────────────────────────────────────────
        //  Core Calculation
        // ─────────────────────────────────────────────

        /// <summary>
        /// Hitung berapa uang yang terkumpul selama offline.
        /// Dipanggil sekali saat Start(), sebelum UI klaim ditampilkan.
        /// </summary>
        public void CalculateOfflineIncome()
        {
            _claimed = false;

            long lastLoginUnix = SaveManager.Instance.GetLastLoginTime();
            if (lastLoginUnix <= 0)
            {
                // First time play — tidak ada offline income
                HasPendingClaim = false;
                return;
            }

            // Hitung selisih waktu
            double realSeconds;
            if (useMockTime && Application.isEditor)
            {
                realSeconds = mockOffsetSeconds;
                Debug.Log($"<color=magenta>[MockTime] Simulating {mockOffsetSeconds:N0} seconds offline.</color>");
            }
            else
            {
                long nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                realSeconds  = nowUnix - lastLoginUnix;
            }

            // Cap 7 hari
            OfflineSecondsElapsed = Math.Min(realSeconds, MAX_OFFLINE_SECONDS);

            if (OfflineSecondsElapsed < MIN_OFFLINE_SECONDS)
            {
                HasPendingClaim = false;
                return;
            }

            // Hitung RPS dari kondisi pilar saat ini
            double rps = 0;
            if (PillarManager.Instance != null)
            {
                rps = PillarManager.Instance.GetRevenuePerSecond();
            }

            PendingOfflineAmount = OfflineSecondsElapsed * rps;

            if (PendingOfflineAmount > 0)
            {
                HasPendingClaim = true;
                Debug.Log($"<color=cyan>OfflineIncome: Rp{PendingOfflineAmount:N0} earned in {FormatDuration(OfflineSecondsElapsed)}.</color>");
            }
        }

        // ─────────────────────────────────────────────
        //  Claim Actions
        // ─────────────────────────────────────────────

        /// <summary>Klaim 1x (tanpa ad).</summary>
        public void ClaimNormal()
        {
            if (!HasPendingClaim || _claimed) return;
            EconomyManager.Instance.AddMoney(PendingOfflineAmount);
            FinalizeClaim();
            Debug.Log($"<color=green>Offline income claimed (1x): Rp{PendingOfflineAmount:N0}</color>");
        }

        /// <summary>Klaim 2x (setelah watch ad — dipanggil oleh Ad Manager).</summary>
        public void ClaimDouble()
        {
            if (!HasPendingClaim || _claimed) return;
            EconomyManager.Instance.AddMoney(PendingOfflineAmount * 2.0);
            FinalizeClaim();
            Debug.Log($"<color=green>Offline income claimed (2x / ad): Rp{PendingOfflineAmount * 2:N0}</color>");
        }

        private void FinalizeClaim()
        {
            HasPendingClaim = false;
            _claimed = true;
            PendingOfflineAmount = 0;
            SaveLastLoginTime(); // Reset timer
        }

        // ─────────────────────────────────────────────
        //  Save / Load Helpers
        // ─────────────────────────────────────────────
        private void SaveLastLoginTime()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SaveManager.Instance.SaveLastLoginTime(now);
        }

        // ─────────────────────────────────────────────
        //  Utility
        // ─────────────────────────────────────────────

        /// <summary>Format durasi detik ke string "X jam Y menit".</summary>
        public string FormatDuration(double totalSeconds)
        {
            TimeSpan ts = TimeSpan.FromSeconds(totalSeconds);
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours} jam {ts.Minutes} menit";
            return $"{ts.Minutes} menit";
        }

        /// <summary>
        /// Helper UI: Ringkasan yang bisa ditampilkan di klaim popup.
        /// Contoh: "Warteg lo jalan 3 jam 20 menit\nTerkumpul: Rp 4.5M"
        /// </summary>
        public string GetClaimSummary()
        {
            return $"Warteg lo jalan <b>{FormatDuration(OfflineSecondsElapsed)}</b>\n" +
                   $"Terkumpul: <b>{EconomyManager.Instance.FormatNumber(PendingOfflineAmount)}</b>";
        }
    }
}
