using UnityEngine;
using UnityEngine.UI;

namespace InUniverse.InResto
{
    /// <summary>
    /// Menangani tampilan visual utama: Background, teks saldo, dan indikator bottleneck.
    ///
    /// CHANGELOG v2:
    /// - HAPUS Update() per-frame (60x/detik → boros baterai, buang CPU)
    /// - GANTI ke event-driven: subscribe EconomyManager.OnBalanceChanged
    /// - Bottleneck text di-refresh setiap GameManager tick (via public method)
    /// - Tambah warna bottleneck visual: Hijau/Kuning/Merah sesuai GDD
    /// </summary>
    public class VisualManager : MonoBehaviour
    {
        public static VisualManager Instance { get; private set; }

        [Header("Main Background")]
        public SpriteRenderer backgroundRenderer;

        [Header("UI References")]
        public Text moneyText;
        public Text bottleneckText;
        public Image bottleneckIcon;

        [Header("Bottleneck Colors (GDD §3.3)")]
        public Color colorOptimal  = new Color(0.2f, 0.8f, 0.3f);  // Hijau
        public Color colorMinor    = new Color(1.0f, 0.8f, 0.1f);  // Kuning
        public Color colorCritical = new Color(0.9f, 0.2f, 0.2f);  // Merah

        // ─────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            // Subscribe ke event — dipanggil hanya saat saldo benar-benar berubah
            EconomyManager.OnBalanceChanged += RefreshMoneyText;
        }

        private void OnDisable()
        {
            EconomyManager.OnBalanceChanged -= RefreshMoneyText;
        }

        // ─────────────────────────────────────────────
        //  Event Handlers
        // ─────────────────────────────────────────────

        /// <summary>
        /// Dipanggil otomatis oleh EconomyManager.OnBalanceChanged.
        /// Hanya update teks — tidak render ulang seluruh scene.
        /// </summary>
        private void RefreshMoneyText(double newBalance)
        {
            if (moneyText == null) return;
            moneyText.text = EconomyManager.Instance.GetFormattedBalance();
        }

        // ─────────────────────────────────────────────
        //  Public API — dipanggil oleh GameManager tiap Tick
        // ─────────────────────────────────────────────

        /// <summary>
        /// Refresh status bottleneck. Dipanggil dari GameManager.ProcessTick()
        /// — maksimal 1x per detik, bukan 60x per detik.
        /// </summary>
        public void RefreshBottleneckUI()
        {
            if (bottleneckText == null || PillarManager.Instance == null) return;

            BottleneckSeverity severity;
            string statusMsg = PillarManager.Instance.GetBottleneckStatus(out severity);

            bottleneckText.text = "⚠ " + statusMsg;

            // Warna icon sesuai severitas
            if (bottleneckIcon != null)
            {
                bottleneckIcon.color = severity switch
                {
                    BottleneckSeverity.Optimal  => colorOptimal,
                    BottleneckSeverity.Minor    => colorMinor,
                    BottleneckSeverity.Critical => colorCritical,
                    _                           => colorOptimal
                };
            }
        }

        // ─────────────────────────────────────────────
        //  Location / Prestige
        // ─────────────────────────────────────────────

        /// <summary>Ganti background scene saat pindah lokasi.</summary>
        public void SetLocation(LocationData data)
        {
            if (backgroundRenderer != null && data != null && data.backgroundSprite != null)
            {
                backgroundRenderer.sprite = data.backgroundSprite;
            }
        }
    }

    /// <summary>Digunakan oleh PillarManager.GetBottleneckStatus() untuk memberi tahu UI.</summary>
    public enum BottleneckSeverity { Optimal, Minor, Critical }
}
