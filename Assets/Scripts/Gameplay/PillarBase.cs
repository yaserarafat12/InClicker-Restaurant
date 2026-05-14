using UnityEngine;

namespace InUniverse.InResto
{
    /// <summary>
    /// Base class untuk 3 pilar: Dapur, Area Makan, Kasir.
    /// Menangani logika level dan upgrade yang bersifat umum.
    ///
    /// CHANGELOG v2:
    /// - Fix: SavePillarLevel hardcoded "1" diganti dengan ProgressionManager.currentLocationIndex
    /// - Tambah SetLevel() — dipanggil SaveManager saat load save data
    /// - Tambah GetMaxLevel() — dipakai ProgressionManager untuk cek syarat Expand
    /// </summary>
    public abstract class PillarBase : MonoBehaviour
    {
        [Header("Pillar Settings")]
        public string pillarName;
        public PillarType type;

        [Header("Runtime Data")]
        [SerializeField] protected int currentLevel = 1;
        [SerializeField] protected int maxLevel = 20; // Batas per lokasi, di-set oleh LocationData
        [SerializeField] protected double currentUpgradeCost;

        [Header("Visual Components")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Sprite[] levelSprites; // Sprite berubah tiap milestone level

        // ─────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────
        protected virtual void Start()
        {
            UpdateUpgradeCost();
            UpdateVisuals();
        }

        // ─────────────────────────────────────────────
        //  Core: Upgrade
        // ─────────────────────────────────────────────
        public virtual void Upgrade()
        {
            if (currentLevel >= maxLevel)
            {
                Debug.Log($"<color=yellow>InResto: {pillarName} sudah max level ({maxLevel})!</color>");
                return;
            }

            double cost = GetUpgradeCost();
            if (EconomyManager.Instance.BuyUpgrade(cost))
            {
                currentLevel++;
                OnUpgradeSuccess();
                UpdateUpgradeCost();
                UpdateVisuals();

                // Simpan ke DB — ambil locationIndex dari ProgressionManager agar tidak hardcoded
                int locId = ProgressionManager.Instance != null
                    ? ProgressionManager.Instance.currentLocationIndex + 1
                    : 1;
                SaveManager.Instance.SavePillarLevel(locId, type.ToString(), currentLevel);

                Debug.Log($"<color=cyan>{pillarName} upgraded to Level {currentLevel}!</color>");
            }
            else
            {
                Debug.Log($"<color=red>InResto: Saldo tidak cukup upgrade {pillarName}!</color>");
            }
        }

        // ─────────────────────────────────────────────
        //  Abstracts & Virtuals
        // ─────────────────────────────────────────────
        protected abstract void UpdateUpgradeCost();

        protected virtual void UpdateVisuals()
        {
            if (levelSprites != null && levelSprites.Length > 0 && spriteRenderer != null)
            {
                int index = Mathf.Clamp(currentLevel / 10, 0, levelSprites.Length - 1);
                spriteRenderer.sprite = levelSprites[index];
            }
        }

        protected virtual void OnUpgradeSuccess()
        {
            // Override di subclass untuk SFX / partikel
        }

        // ─────────────────────────────────────────────
        //  Public Accessors
        // ─────────────────────────────────────────────
        public double GetUpgradeCost() => currentUpgradeCost;
        public int GetLevel() => currentLevel;
        public int GetMaxLevel() => maxLevel;
        public bool IsMaxed() => currentLevel >= maxLevel;

        /// <summary>
        /// Dipanggil oleh SaveManager saat load game — set level tanpa biaya.
        /// </summary>
        public void SetLevel(int savedLevel)
        {
            currentLevel = Mathf.Clamp(savedLevel, 1, maxLevel);
            UpdateUpgradeCost();
            UpdateVisuals();
        }

        /// <summary>
        /// Dipanggil saat pindah lokasi (Prestige) — reset ke level 1.
        /// </summary>
        public void ResetForNewLocation(int newMaxLevel)
        {
            currentLevel = 1;
            maxLevel = newMaxLevel;
            UpdateUpgradeCost();
            UpdateVisuals();
        }
    }

    public enum PillarType
    {
        Dapur,
        AreaMakan,
        Kasir
    }
}
