using UnityEngine;
using System.Collections;

namespace InUniverse.InResto
{
    /// <summary>
    /// Konduktor utama game. Menyatukan Economy, Save, dan Gameplay.
    /// Memiliki sistem Tick untuk simulasi passive income.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float tickInterval = 1.0f; // 1 detik per tick
        [SerializeField] private float autosaveInterval = 120f; // 2 menit

        private bool isGameRunning = false;

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

        private void Start()
        {
            StartCoroutine(InitializeSequence());
        }

        private IEnumerator InitializeSequence()
        {
            // 1. Tunggu SaveManager siap (DB sudah init di Awake)
            yield return new WaitForSeconds(0.1f);

            // 2. Load PlayerData dari SQLite
            double balance, lifetime;
            int locationId;
            SaveManager.Instance.LoadGameState(out balance, out lifetime, out locationId);

            // 3. Inject ke EconomyManager
            EconomyManager.Instance.Initialize(balance, lifetime);

            // 4. Load level pilar dari DB dan terapkan ke controller
            if (PillarManager.Instance != null)
            {
                int dapurLv, makanLv, kasirLv;
                SaveManager.Instance.LoadAllPillarsForLocation(locationId, out dapurLv, out makanLv, out kasirLv);

                if (PillarManager.Instance.dapur != null) PillarManager.Instance.dapur.SetLevel(dapurLv);
                if (PillarManager.Instance.makan != null) PillarManager.Instance.makan.SetLevel(makanLv);
                if (PillarManager.Instance.kasir != null) PillarManager.Instance.kasir.SetLevel(kasirLv);

                Debug.Log($"<color=cyan>Pillars loaded: Dapur Lv{dapurLv} | Makan Lv{makanLv} | Kasir Lv{kasirLv}</color>");
            }

            // 5. Hitung offline income (sebelum game loop jalan)
            if (OfflineIncomeManager.Instance != null)
            {
                OfflineIncomeManager.Instance.CalculateOfflineIncome();
                // UI klaim akan ditampilkan oleh OfflineClaimUI yang subscribe ke HasPendingClaim
            }

            // 6. Jalankan loop utama
            isGameRunning = true;
            StartCoroutine(GameLoop());
            StartCoroutine(AutosaveLoop());

            Debug.Log("<color=cyan>InResto: Game Engine Started! ✓</color>");
        }

        private IEnumerator GameLoop()
        {
            while (isGameRunning)
            {
                yield return new WaitForSeconds(tickInterval);

                // Proses income dari 3 pilar
                if (PillarManager.Instance != null)
                {
                    PillarManager.Instance.ProcessTick();
                }

                // Refresh bottleneck UI 1x per tick (bukan 60x per frame)
                if (VisualManager.Instance != null)
                {
                    VisualManager.Instance.RefreshBottleneckUI();
                }
            }
        }

        private IEnumerator AutosaveLoop()
        {
            while (isGameRunning)
            {
                yield return new WaitForSeconds(autosaveInterval);
                PerformSave();
                Debug.Log("<color=grey>InResto: Autosave completed...</color>");
            }
        }

        public void PerformSave()
        {
            if (EconomyManager.Instance == null || SaveManager.Instance == null) return;

            SaveManager.Instance.SaveGameState(
                EconomyManager.Instance.CurrentBalance,
                EconomyManager.Instance.LifetimeEarnings,
                1 // TODO: Ganti dengan CurrentLocationID jika sudah ada sistem lokasi
            );
        }

        private void OnApplicationQuit()
        {
            PerformSave();
            Debug.Log("<color=yellow>InResto: Saving on quit...</color>");
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) PerformSave();
        }
    }
}
