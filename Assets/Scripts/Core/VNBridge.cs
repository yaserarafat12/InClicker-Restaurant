using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace InUniverse.InResto
{
    /// <summary>
    /// Jembatan gameplay Tycoon → Visual Novel — GDD §7
    ///
    /// CHANGELOG v2 — Rebuild total dari switch-case ke ScriptableObject:
    /// - Ganti switch-case hardcoded → VNChapterData[] ScriptableObject
    /// - Subscribe ke ProgressionManager.onVNTrigger (string panel ID, misal "2.1")
    /// - Revenue-based trigger: cek setiap kali LifetimeEarnings berubah
    /// - Multi-dialog per panel: navigasi dengan Next button / tap anywhere
    /// - Unskippable first time, skippable replay (GDD §7.3)
    /// - Story Archive: track unlocked panels
    ///
    /// Setup Unity:
    ///   1. Buat VNChapterData assets (via Create → InResto → VN Panel Data)
    ///   2. Assign ke allPanels[] di Inspector
    ///   3. Assign UI references
    /// </summary>
    public class VNBridge : MonoBehaviour
    {
        public static VNBridge Instance { get; private set; }

        // ─── VN Data ─────────────────────────────────────────────────
        [Header("All VN Panels (54 total — assign di Inspector)")]
        [SerializeField] private VNChapterData[] allPanels;

        // ─── UI References ───────────────────────────────────────────
        [Header("VN UI Elements")]
        [SerializeField] private GameObject  vnOverlay;
        [SerializeField] private CanvasGroup vnAlpha;
        [SerializeField] private Image       backgroundImage;
        [SerializeField] private Image       characterImage;
        [SerializeField] private Text        speakerNameText;
        [SerializeField] private Text        dialogueText;
        [SerializeField] private Button      nextButton;     // Tap anywhere / Next
        [SerializeField] private Button      skipButton;     // Top-right Skip (replay only)

        // ─── State ───────────────────────────────────────────────────
        private VNChapterData  currentPanel;
        private int            currentDialogIndex = 0;
        private bool           isFirstTimeViewing = true;
        private bool           isPlaying          = false;
        private double         lastCheckedEarnings = 0;

        private readonly HashSet<string> unlockedPanels = new HashSet<string>();

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
            if (vnOverlay != null) vnOverlay.SetActive(false);

            // Subscribe ke ProgressionManager string-based VN trigger
            if (ProgressionManager.Instance != null)
                ProgressionManager.Instance.onVNTrigger.AddListener(ShowPanelById);

            // Revenue-based trigger checks
            EconomyManager.OnBalanceChanged += CheckRevenueTriggers;

            // Buttons
            if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
            if (skipButton != null) skipButton.onClick.AddListener(SkipPanel);
        }

        private void OnDestroy()
        {
            EconomyManager.OnBalanceChanged -= CheckRevenueTriggers;
        }

        // ─────────────────────────────────────────────────────────────
        //  Trigger System
        // ─────────────────────────────────────────────────────────────

        /// <summary>Dipanggil setiap balance berubah — cek revenue-trigger panels.</summary>
        private void CheckRevenueTriggers(double balance)
        {
            if (allPanels == null || EconomyManager.Instance == null) return;

            double lifetime = EconomyManager.Instance.LifetimeEarnings;
            if (lifetime <= lastCheckedEarnings) return;
            lastCheckedEarnings = lifetime;

            foreach (var panel in allPanels)
            {
                if (panel == null) continue;
                if (panel.triggerType != VNTriggerType.Revenue) continue;
                if (unlockedPanels.Contains(panel.panelId)) continue;
                if (lifetime >= panel.triggerValue)
                {
                    ShowPanel(panel);
                    break;
                }
            }
        }

        /// <summary>Trigger by panel ID string — dipanggil ProgressionManager saat expand.</summary>
        public void ShowPanelById(string panelId)
        {
            if (allPanels == null) return;
            foreach (var panel in allPanels)
            {
                if (panel != null && panel.panelId == panelId)
                {
                    ShowPanel(panel);
                    return;
                }
            }
            Debug.LogWarning($"[VNBridge] Panel '{panelId}' tidak ada di allPanels[]");
        }

        // ─────────────────────────────────────────────────────────────
        //  Panel Display
        // ─────────────────────────────────────────────────────────────
        private void ShowPanel(VNChapterData panel)
        {
            if (isPlaying) return; // Tidak interrupt VN yang sedang berjalan

            currentPanel       = panel;
            currentDialogIndex = 0;
            isFirstTimeViewing = !unlockedPanels.Contains(panel.panelId);
            isPlaying          = true;
            unlockedPanels.Add(panel.panelId);

            if (skipButton != null)
                skipButton.gameObject.SetActive(!isFirstTimeViewing);

            StartCoroutine(FadeInAndDisplay());
        }

        private IEnumerator FadeInAndDisplay()
        {
            if (vnOverlay == null) { isPlaying = false; yield break; }

            vnOverlay.SetActive(true);
            if (vnAlpha != null) { vnAlpha.alpha = 0f; vnAlpha.DOFade(1f, 0.5f); }
            yield return new WaitForSeconds(0.5f);

            ShowCurrentDialog();
        }

        private void ShowCurrentDialog()
        {
            if (currentPanel?.dialogs == null || currentDialogIndex >= currentPanel.dialogs.Length)
            {
                CloseVN();
                return;
            }

            VNDialog d = currentPanel.dialogs[currentDialogIndex];

            if (speakerNameText != null)
                speakerNameText.text = d.speakerName;

            if (dialogueText != null)
            {
                dialogueText.text = "";
                dialogueText.DOText(d.dialogueText, 0.6f).SetEase(Ease.Linear);
            }

            if (characterImage != null)
            {
                characterImage.sprite = d.characterSprite;
                characterImage.gameObject.SetActive(d.characterSprite != null);
            }

            if (backgroundImage != null && currentPanel.backgroundSprite != null)
                backgroundImage.sprite = currentPanel.backgroundSprite;
        }

        // ─────────────────────────────────────────────────────────────
        //  Navigation
        // ─────────────────────────────────────────────────────────────
        private void OnNextClicked()
        {
            if (!isPlaying) return;
            currentDialogIndex++;
            ShowCurrentDialog();
        }

        public void SkipPanel()
        {
            if (!isPlaying || isFirstTimeViewing) return; // First time = tidak bisa skip
            CloseVN();
        }

        // ─────────────────────────────────────────────────────────────
        //  Close
        // ─────────────────────────────────────────────────────────────
        private void CloseVN()
        {
            isPlaying = false;
            if (vnAlpha != null)
                vnAlpha.DOFade(0f, 0.4f).OnComplete(() =>
                {
                    if (vnOverlay != null) vnOverlay.SetActive(false);
                });
            Debug.Log($"<color=magenta>[VNBridge] ✓ Panel '{currentPanel?.panelId}' selesai.</color>");
        }

        // ─────────────────────────────────────────────────────────────
        //  Story Archive Query
        // ─────────────────────────────────────────────────────────────
        public bool IsPanelUnlocked(string panelId) => unlockedPanels.Contains(panelId);
        public int  UnlockedCount => unlockedPanels.Count;
        public int  TotalPanels   => allPanels?.Length ?? 54;
    }
}
