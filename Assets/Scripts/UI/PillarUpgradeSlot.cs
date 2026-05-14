using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace InUniverse.InResto
{
    /// <summary>
    /// Menangani satu kartu upgrade di menu UI (Dapur / Area Makan / Kasir).
    ///
    /// CHANGELOG v2:
    /// - HAPUS Update() per-frame polling → sekarang event-driven
    /// - Subscribe EconomyManager.OnBalanceChanged → refresh afford-state saat saldo berubah
    /// - Subscribe EconomyManager.OnUpgradePerformed → refresh level text saat upgrade sukses
    /// - Tambah visual state: hijau (mampu beli) / abu-abu (belum mampu) / kuning (bottleneck!)
    /// - Tambah "MAX" label saat pilar sudah max level
    /// </summary>
    public class PillarUpgradeSlot : MonoBehaviour
    {
        [Header("Pillar Reference")]
        public PillarBase targetPillar;

        [Header("UI Components")]
        public Text pillarNameText;
        public Text levelText;
        public Text costText;
        public Button upgradeButton;
        public CanvasGroup canvasGroup;

        [Header("Button Colors")]
        public UnityEngine.UI.Image buttonImage;
        public Color colorCanAfford  = new Color(0.15f, 0.75f, 0.3f);   // Hijau
        public Color colorCannotAfford = new Color(0.5f, 0.5f, 0.5f);   // Abu
        public Color colorBottleneck = new Color(1.0f, 0.75f, 0.1f);    // Kuning (direkomendasikan)

        // ─────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────
        private void Start()
        {
            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeClicked);

            UpdateUI();
        }

        private void OnEnable()
        {
            // Refresh cukup saat saldo atau upgrade state berubah — bukan tiap frame
            EconomyManager.OnBalanceChanged  += OnBalanceChanged;
            EconomyManager.OnUpgradePerformed += OnUpgradePerformed;
        }

        private void OnDisable()
        {
            EconomyManager.OnBalanceChanged  -= OnBalanceChanged;
            EconomyManager.OnUpgradePerformed -= OnUpgradePerformed;
        }

        // ─────────────────────────────────────────────
        //  Event Handlers
        // ─────────────────────────────────────────────
        private void OnBalanceChanged(double newBalance)
        {
            RefreshAffordState();
        }

        private void OnUpgradePerformed()
        {
            UpdateUI(); // Level berubah — refresh semua teks
        }

        // ─────────────────────────────────────────────
        //  UI Refresh
        // ─────────────────────────────────────────────

        /// <summary>Refresh lengkap: nama, level, harga, dan status button.</summary>
        public void UpdateUI()
        {
            if (targetPillar == null || EconomyManager.Instance == null) return;

            if (pillarNameText != null)
                pillarNameText.text = targetPillar.pillarName;

            if (levelText != null)
            {
                if (targetPillar.IsMaxed())
                    levelText.text = $"Lv. {targetPillar.GetLevel()} — MAX";
                else
                    levelText.text = $"Lv. {targetPillar.GetLevel()} / {targetPillar.GetMaxLevel()}";
            }

            double cost = targetPillar.GetUpgradeCost();
            if (costText != null)
                costText.text = targetPillar.IsMaxed()
                    ? "— SUDAH MAX —"
                    : EconomyManager.Instance.FormatNumber(cost);

            RefreshAffordState();
        }

        /// <summary>Hanya refresh interactability button — dipanggil saat saldo berubah.</summary>
        private void RefreshAffordState()
        {
            if (targetPillar == null || upgradeButton == null) return;

            bool isMaxed    = targetPillar.IsMaxed();
            double cost     = targetPillar.GetUpgradeCost();
            bool canAfford  = !isMaxed && EconomyManager.Instance.CanAfford(cost);

            upgradeButton.interactable = canAfford && !isMaxed;

            if (canvasGroup != null)
                canvasGroup.alpha = (canAfford || isMaxed) ? 1.0f : 0.6f;

            // Warna button
            if (buttonImage != null)
            {
                if (isMaxed)
                    buttonImage.color = colorCannotAfford;
                else if (canAfford)
                    buttonImage.color = colorCanAfford;
                else
                    buttonImage.color = colorCannotAfford;
            }
        }

        // ─────────────────────────────────────────────
        //  Button Handler
        // ─────────────────────────────────────────────
        private void OnUpgradeClicked()
        {
            if (targetPillar == null) return;

            targetPillar.Upgrade();

            // "Juice" — efek pantul DOTween saat klik
            transform.DOPunchScale(new Vector3(0.12f, 0.12f, 0), 0.25f, 5, 0.5f);
        }
    }
}
