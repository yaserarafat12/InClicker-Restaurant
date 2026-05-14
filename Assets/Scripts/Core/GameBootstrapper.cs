using UnityEngine;

namespace InUniverse.InResto
{
    /// <summary>
    /// Script "Sakti" untuk mengotomatisasi setup game.
    /// Cukup pasang satu script ini di scene, dan dia akan membangun sistem engine lo.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Setup Options")]
        public bool autoCreateManagers = true;
        public bool autoConnectUI = true;

        private void Awake()
        {
            Debug.Log("<color=yellow>InResto: System Boot Sequence Started...</color>");

            if (autoCreateManagers)
            {
                EnsureManagersExist();
            }
        }

        /// <summary>
        /// Mengecek apakah GameObject [MANAGERS] sudah ada.
        /// Jika belum, buat secara otomatis dan tambahkan semua komponen manager.
        /// </summary>
        private void EnsureManagersExist()
        {
            GameObject managerObj = GameObject.Find("[MANAGERS]");
            
            if (managerObj == null)
            {
                managerObj = new GameObject("[MANAGERS]");
                Debug.Log("<color=cyan>Bootstrapper: Creating [MANAGERS] GameObject...</color>");
                
                // Urutan PENTING — SaveManager harus pertama (yang lain bergantung padanya)
                managerObj.AddComponent<SaveManager>();          // 1. DB Init
                managerObj.AddComponent<EconomyManager>();       // 2. Finansial Engine
                managerObj.AddComponent<PillarManager>();        // 3. Bottleneck Calculator
                managerObj.AddComponent<RushHourManager>();      // 4. Tap Mechanic (butuh PillarManager)
                managerObj.AddComponent<ProgressionManager>();   // 5. Lokasi & Prestige
                managerObj.AddComponent<OfflineIncomeManager>(); // 6. Offline Earnings
                managerObj.AddComponent<VisualManager>();        // 7. UI Layer
                managerObj.AddComponent<VNBridge>();             // 8. Visual Novel
                managerObj.AddComponent<GameManager>();          // 9. Master Conductor (terakhir!)
            }
            
            DontDestroyOnLoad(managerObj);
        }
    }
}
