using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace InUniverse.InResto
{
    /// <summary>
    /// Sistem Rush Hour — GDD §4.1 "Tap Mechanic"
    ///
    /// Cara kerja:
    ///   1. Player tap area warteg → Energy bar +2%
    ///   2. Saat energy 100% → auto-trigger Rush Hour Mode (10 detik)
    ///   3. Selama Rush Hour: semua pilar 5x lebih cepat, revenue pop-up lebih besar
    ///   4. Setelah selesai: energy reset ke 0, bisa langsung isi ulang (no cooldown)
    ///
    /// GDD Quote: "Player tap BUKAN untuk produce individual item, tapi untuk
    ///             membakar semangat seluruh tim."
    ///
    /// .gemini.d §2: Setiap tap WAJIB memberikan visual feedback (DOTween + haptic)
    /// </summary>
    public class RushHourManager : MonoBehaviour
    {
        public static RushHourManager Instance { get; private set; }

        // ─── Configuration ────────────────────────────────────────────
        [Header("Energy Settings")]
        [SerializeField] private float energyPerTap      = 2f;    // % per tap
        [SerializeField] private float maxEnergy         = 100f;
        [SerializeField] private float rushHourDuration  = 10f;   // detik
        [SerializeField] private float rushHourMultiplier = 5f;   // 5x speed

        [Header("Anti-Cheat (GDD §A anti-exploit + .gemini.d §5)")]
        [SerializeField] private float maxTapsPerSecond  = 15f;   // tap fisik max ~8/detik, batas 15 = cukup toleran

        // ─── UI References ─────────────────────────────────────────────
        [Header("UI References")]
        [SerializeField] private Slider   energyBar;          // Bar horizontal di top screen
        [SerializeField] private Image    energyBarFill;      // Image fill untuk warna animasi
        [SerializeField] private Image    rushHourOverlay;    // Orange glow overlay (CanvasGroup)
        [SerializeField] private CanvasGroup overlayGroup;
        [SerializeField] private Text     rushHourTimerText;  // "Rush Hour! 8s"

        [Header("Colors")]
        [SerializeField] private Color colorNormal    = new Color(0.3f, 0.7f, 1.0f);   // Biru tenang
        [SerializeField] private Color colorAlmostFull= new Color(1.0f, 0.8f, 0.1f);   // Kuning excited
        [SerializeField] private Color colorRushHour  = new Color(1.0f, 0.4f, 0.1f);   // Orange hot

        [Header("Screen Effects")]
        [SerializeField] private RectTransform gameplayArea; // Target untuk screen shake

        // ─── State ─────────────────────────────────────────────────────
        private float currentEnergy      = 0f;
        private bool  isRushHourActive   = false;
        private float rushHourTimeLeft   = 0f;
        private float rushMultiplierApplied = 1f;

        // Anti-cheat: track tap frequency
        private float tapCountThisSecond = 0f;
        private float tapWindowTimer     = 0f;

        // Events untuk GameManager dan UI
        public static event System.Action<bool>  OnRushHourStateChanged;  // true = start, false = end
        public static event System.Action<float> OnEnergyChanged;         // 0-1 normalized

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            ResetEnergy(animate: false);
        }

        private void Update()
        {
            // Update rush hour countdown
            if (isRushHourActive)
            {
                rushHourTimeLeft -= Time.deltaTime;
                if (rushHourTimerText != null)
                    rushHourTimerText.text = $"🔥 Rush Hour! {rushHourTimeLeft:F1}s";

                if (rushHourTimeLeft <= 0f)
                    EndRushHour();
            }

            // Anti-cheat: reset tap window counter setiap detik
            tapWindowTimer += Time.deltaTime;
            if (tapWindowTimer >= 1f)
            {
                tapCountThisSecond = 0f;
                tapWindowTimer     = 0f;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Public API — dipanggil oleh tap input handler di scene
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Dipanggil oleh InputHandler / Button saat player tap area warteg.
        /// Mengembalikan true jika tap valid (tidak di-reject anti-cheat).
        /// </summary>
        public bool OnPlayerTap(Vector2 tapScreenPosition)
        {
            // Anti-cheat: reject jika melebihi batas fisik manusia
            tapCountThisSecond++;
            if (tapCountThisSecond > maxTapsPerSecond)
            {
                Debug.Log("<color=red>[RushHour] Tap anomaly detected — ignoring.</color>");
                return false;
            }

            // Jika sedang Rush Hour, tap tidak menambah energy (sudah full)
            if (isRushHourActive) return false;

            // Tambah energy
            currentEnergy = Mathf.Min(currentEnergy + energyPerTap, maxEnergy);
            OnEnergyChanged?.Invoke(currentEnergy / maxEnergy);
            UpdateEnergyBarUI();

            // Feedback: ripple effect di posisi tap
            SpawnTapFeedback(tapScreenPosition);

            // Haptic
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            #endif

            // Cek apakah energy penuh → trigger Rush Hour
            if (currentEnergy >= maxEnergy)
            {
                StartRushHour();
            }

            return true;
        }

        /// <summary>
        /// Isi ulang energy bar instan — dipakai setelah Watch Ad (GDD §8.2).
        /// </summary>
        public void RefillEnergyByAd()
        {
            if (isRushHourActive) return; // Tidak berlaku saat Rush Hour aktif
            currentEnergy = maxEnergy;
            OnEnergyChanged?.Invoke(1f);
            UpdateEnergyBarUI();
            StartRushHour();
            Debug.Log("<color=green>[RushHour] Energy refilled via ad!</color>");
        }

        public bool IsRushHourActive   => isRushHourActive;
        public float GetRushMultiplier => rushMultiplierApplied;
        public float GetEnergyNormalized => currentEnergy / maxEnergy;

        // ─────────────────────────────────────────────────────────────
        //  Rush Hour Logic
        // ─────────────────────────────────────────────────────────────
        private void StartRushHour()
        {
            isRushHourActive      = true;
            rushHourTimeLeft      = rushHourDuration;
            rushMultiplierApplied = rushHourMultiplier;

            OnRushHourStateChanged?.Invoke(true);

            StartCoroutine(RushHourEffects());
            Debug.Log("<color=orange>🔥 RUSH HOUR STARTED!</color>");
        }

        private void EndRushHour()
        {
            isRushHourActive      = false;
            rushHourTimeLeft      = 0f;
            rushMultiplierApplied = 1f;

            OnRushHourStateChanged?.Invoke(false);
            ResetEnergy(animate: true);

            // Fade out overlay
            if (overlayGroup != null)
                overlayGroup.DOFade(0f, 0.5f);

            if (rushHourTimerText != null)
                rushHourTimerText.text = "";

            Debug.Log("<color=grey>[RushHour] Rush Hour ended. Tap to fill again!</color>");
        }

        // ─────────────────────────────────────────────────────────────
        //  Visual Effects
        // ─────────────────────────────────────────────────────────────
        private IEnumerator RushHourEffects()
        {
            // 1. Screen flash orange
            if (overlayGroup != null)
            {
                overlayGroup.alpha = 0f;
                overlayGroup.DOFade(0.35f, 0.2f).SetLoops(2, LoopType.Yoyo);
                yield return new WaitForSeconds(0.4f);
                overlayGroup.DOFade(0.15f, 0.3f); // Stay subtle glow
            }

            // 2. Camera shake (screen shake ringan)
            if (gameplayArea != null)
            {
                gameplayArea.DOShakeAnchorPos(0.4f, strength: 8f, vibrato: 12, randomness: 45);
            }

            // 3. Energy bar → merah pulsing
            if (energyBarFill != null)
            {
                energyBarFill.DOColor(colorRushHour, 0.2f);
                energyBarFill.DOFade(0.7f, 0.5f).SetLoops(-1, LoopType.Yoyo); // Pulsing
            }

            yield return new WaitForSeconds(rushHourDuration);

            // 4. Cleanup pulse
            if (energyBarFill != null)
            {
                energyBarFill.DOKill();
                energyBarFill.DOFade(1f, 0.1f);
            }
        }

        private void SpawnTapFeedback(Vector2 screenPos)
        {
            // DOTween scale punch pada gameplay area (bukan spawn object)
            // Dalam implementasi full: spawn RippleParticle prefab di screenPos
            if (gameplayArea != null)
            {
                // Micro-shake: sangat ringan biar responsif tapi tidak annoying
                gameplayArea.DOPunchAnchorPos(new Vector2(2f, 2f), 0.1f, 3, 0.5f);
            }
        }

        private void ResetEnergy(bool animate)
        {
            currentEnergy = 0f;
            OnEnergyChanged?.Invoke(0f);

            if (energyBar != null)
            {
                if (animate)
                    DOTween.To(() => energyBar.value, v => energyBar.value = v, 0f, 0.3f);
                else
                    energyBar.value = 0f;
            }

            if (energyBarFill != null)
                energyBarFill.color = colorNormal;
        }

        private void UpdateEnergyBarUI()
        {
            float normalized = currentEnergy / maxEnergy;

            if (energyBar != null)
                energyBar.value = normalized;

            // Ganti warna bar sesuai fill level
            if (energyBarFill != null)
            {
                Color targetColor = normalized > 0.75f ? colorAlmostFull : colorNormal;
                energyBarFill.DOColor(targetColor, 0.1f);
            }
        }
    }
}
