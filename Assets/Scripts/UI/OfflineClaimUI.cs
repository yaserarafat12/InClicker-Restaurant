using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace InUniverse.InResto
{
    /// <summary>
    /// UI Klaim Offline Income — GDD §4.2 "Juicy Claim Animation"
    ///
    /// Flow (persis sesuai GDD):
    ///   1. Screen fade to dark
    ///   2. Brankas besar muncul di center (scale 0 → 100%)
    ///   3. Brankas terbuka dengan explosion effect
    ///   4. Cash & coins berjatuhan (particle system)
    ///   5. Number counter dari 0 naik ke total amount (smooth lerp)
    ///   6. Dua tombol: [KLAIM 1x] dan [KLAIM 2x (Watch Ad)]
    ///
    /// Cara pakai:
    ///   Pasang script ini di Canvas overlay.
    ///   Script otomatis check OfflineIncomeManager.HasPendingClaim saat Start().
    ///   Atau panggil ShowClaimPopup() dari code lain.
    /// </summary>
    public class OfflineClaimUI : MonoBehaviour
    {
        // ─── Panel References ──────────────────────────────────────────
        [Header("Root Panel")]
        [SerializeField] private CanvasGroup rootPanel;       // Seluruh popup
        [SerializeField] private Image       darkOverlay;     // Background gelap semi-transparent

        [Header("Brankas Animasi")]
        [SerializeField] private RectTransform brankasContainer; // Parent brankas
        [SerializeField] private Image   brankasClosedImage;     // Sprite brankas tertutup
        [SerializeField] private Image   brankasOpenImage;       // Sprite brankas terbuka (swap)
        [SerializeField] private ParticleSystem coinParticles;   // Partikel koin emas

        [Header("Teks & Counter")]
        [SerializeField] private Text durationText;   // "Warteg lo jalan 3 jam 20 menit"
        [SerializeField] private Text amountText;     // Counter: "Rp 0" → "Rp 4.5M"
        [SerializeField] private float counterDuration = 2.0f; // Durasi animasi counter angka

        [Header("Tombol Klaim")]
        [SerializeField] private Button claimNormalButton; // [KLAIM Rp X]
        [SerializeField] private Text   claimNormalText;
        [SerializeField] private Button claimDoubleButton; // [KLAIM 2x - Watch Ad]
        [SerializeField] private Text   claimDoubleText;

        // ─── Internal ──────────────────────────────────────────────────
        private double _pendingAmount = 0;
        private bool   _claimed = false;
        private Coroutine _counterCoroutine;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────
        private void Start()
        {
            // Mulai tersembunyi
            if (rootPanel != null)
            {
                rootPanel.alpha          = 0f;
                rootPanel.interactable   = false;
                rootPanel.blocksRaycasts = false;
            }

            // Hook tombol
            if (claimNormalButton != null)
                claimNormalButton.onClick.AddListener(OnClaimNormal);
            if (claimDoubleButton != null)
                claimDoubleButton.onClick.AddListener(OnClaimDouble);

            // Cek otomatis saat game dibuka
            if (OfflineIncomeManager.Instance != null && OfflineIncomeManager.Instance.HasPendingClaim)
            {
                StartCoroutine(DelayedShow(0.8f)); // Beri waktu game engine finish init
            }
        }

        private IEnumerator DelayedShow(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowClaimPopup();
        }

        // ─────────────────────────────────────────────────────────────
        //  Public — dipanggil dari luar jika perlu
        // ─────────────────────────────────────────────────────────────
        public void ShowClaimPopup()
        {
            if (OfflineIncomeManager.Instance == null) return;
            if (!OfflineIncomeManager.Instance.HasPendingClaim) return;

            _pendingAmount = OfflineIncomeManager.Instance.PendingOfflineAmount;
            _claimed       = false;

            StartCoroutine(PlayOpenAnimation());
        }

        // ─────────────────────────────────────────────────────────────
        //  Animation Sequence (mirror GDD §4.2 step-by-step)
        // ─────────────────────────────────────────────────────────────
        private IEnumerator PlayOpenAnimation()
        {
            // ── Step 1: Fade in dark overlay ──────────────────────────
            rootPanel.interactable   = false;
            rootPanel.blocksRaycasts = true;
            rootPanel.DOFade(1f, 0.4f);
            yield return new WaitForSeconds(0.4f);

            // ── Step 2: Brankas muncul dari tengah (scale 0 → 100%) ───
            if (brankasContainer != null)
            {
                brankasContainer.localScale = Vector3.zero;
                brankasContainer.gameObject.SetActive(true);
                brankasContainer.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            }

            // Teks durasi muncul
            if (durationText != null && OfflineIncomeManager.Instance != null)
            {
                durationText.text  = OfflineIncomeManager.Instance.GetClaimSummary();
                durationText.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(0.6f);

            // ── Step 3: Brankas terbuka (explosion) ───────────────────
            if (brankasClosedImage != null) brankasClosedImage.gameObject.SetActive(false);
            if (brankasOpenImage   != null) brankasOpenImage.gameObject.SetActive(true);

            // Punch scale brankas saat "meledak"
            if (brankasContainer != null)
                brankasContainer.DOPunchScale(new Vector3(0.3f, 0.3f, 0), 0.4f, 8, 0.5f);

            // ── Step 4: Partikel koin ──────────────────────────────────
            if (coinParticles != null)
                coinParticles.Play();

            yield return new WaitForSeconds(0.3f);

            // ── Step 5: Counter angka naik dari 0 ─────────────────────
            if (amountText != null)
            {
                amountText.gameObject.SetActive(true);
                _counterCoroutine = StartCoroutine(AnimateCounter(0, _pendingAmount, counterDuration));
            }

            yield return new WaitForSeconds(counterDuration + 0.3f);

            // ── Step 6: Tombol muncul ─────────────────────────────────
            if (claimNormalButton != null)
            {
                string fmtAmount = EconomyManager.Instance != null
                    ? EconomyManager.Instance.FormatNumber(_pendingAmount)
                    : $"Rp {_pendingAmount:N0}";

                if (claimNormalText != null)
                    claimNormalText.text = $"KLAIM {fmtAmount}";
                if (claimDoubleText != null)
                    claimDoubleText.text = $"KLAIM 2X ({EconomyManager.Instance?.FormatNumber(_pendingAmount * 2) ?? "2x"})\n[Watch Ad]";

                claimNormalButton.gameObject.SetActive(true);
                claimNormalButton.transform.localScale = Vector3.zero;
                claimNormalButton.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

                yield return new WaitForSeconds(0.1f);

                if (claimDoubleButton != null)
                {
                    claimDoubleButton.gameObject.SetActive(true);
                    claimDoubleButton.transform.localScale = Vector3.zero;
                    claimDoubleButton.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                }
            }

            rootPanel.interactable = true;
        }

        // ─────────────────────────────────────────────────────────────
        //  Animated Counter
        // ─────────────────────────────────────────────────────────────
        private IEnumerator AnimateCounter(double from, double to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float   t        = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                double  current  = from + (to - from) * t;

                if (amountText != null && EconomyManager.Instance != null)
                    amountText.text = EconomyManager.Instance.FormatNumber(current);

                yield return null;
            }

            // Pastikan nilai final tepat
            if (amountText != null && EconomyManager.Instance != null)
                amountText.text = EconomyManager.Instance.FormatNumber(to);
        }

        // ─────────────────────────────────────────────────────────────
        //  Button Handlers
        // ─────────────────────────────────────────────────────────────
        private void OnClaimNormal()
        {
            if (_claimed) return;
            _claimed = true;

            OfflineIncomeManager.Instance?.ClaimNormal();
            ClosePanel();
        }

        private void OnClaimDouble()
        {
            if (_claimed) return;
            // TODO: AdManager.Instance.ShowRewardedAd(onSuccess: () => {
            //     OfflineIncomeManager.Instance?.ClaimDouble();
            //     ClosePanel();
            // });
            // Placeholder: langsung klaim 2x (nanti diganti dengan ad integration)
            _claimed = true;
            OfflineIncomeManager.Instance?.ClaimDouble();
            ClosePanel();
            Debug.Log("<color=cyan>[OfflineClaimUI] Ad placeholder — will integrate AdMob in Phase 5</color>");
        }

        private void ClosePanel()
        {
            if (_counterCoroutine != null)
                StopCoroutine(_counterCoroutine);

            if (coinParticles != null)
                coinParticles.Stop();

            rootPanel.interactable   = false;
            rootPanel.blocksRaycasts = false;
            rootPanel.DOFade(0f, 0.4f).OnComplete(() =>
            {
                if (brankasContainer != null) brankasContainer.gameObject.SetActive(false);
                if (claimNormalButton != null) claimNormalButton.gameObject.SetActive(false);
                if (claimDoubleButton != null) claimDoubleButton.gameObject.SetActive(false);
                if (amountText        != null) amountText.gameObject.SetActive(false);
            });
        }
    }
}
