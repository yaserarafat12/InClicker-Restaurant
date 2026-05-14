using UnityEngine;

namespace InUniverse.InResto
{
    /// <summary>
    /// ScriptableObject untuk satu panel Visual Novel.
    /// Buat asset via: Create → InResto → VN Chapter Data
    /// Naming convention: VNPanel_0_1, VNPanel_1_5, VNPanel_7_8, dst.
    ///
    /// GDD §7.2: 54 panels total, Act 0-7
    /// </summary>
    [CreateAssetMenu(fileName = "VNPanel_0_0", menuName = "InResto/VN Panel Data")]
    public class VNChapterData : ScriptableObject
    {
        [Header("Identity")]
        public string panelId;          // Format: "0.1", "1.5", "7.8"
        public string chapterTitle;     // "Tiga Kali Gagal"

        [Header("Trigger")]
        public VNTriggerType triggerType;
        public double        triggerValue;  // Revenue threshold, location index, dll.

        [Header("Content — Array of Dialog Lines")]
        public VNDialog[] dialogs;

        [Header("Background")]
        public Sprite backgroundSprite;  // Gambar latar belakang scene

        [Header("Flags")]
        public bool isSkippableOnReplay = true;
        public bool isPremiumOnly       = false;  // Butuh premium unlock? (GDD §8.2)
    }

    [System.Serializable]
    public class VNDialog
    {
        public string speakerName;       // "MC", "Nenek", "Ojan", dll.
        public Sprite characterSprite;   // Full body PNG overlay
        [TextArea(2, 8)]
        public string dialogueText;
        public SpeakerPosition position; // Kiri atau kanan layar
    }

    public enum VNTriggerType
    {
        Revenue,     // Unlock saat total earnings >= triggerValue
        Location,    // Unlock saat pindah ke lokasi index = triggerValue
        TimeDays,    // Unlock setelah triggerValue hari gameplay
        Manual,      // Di-trigger secara manual dari ProgressionManager
    }

    public enum SpeakerPosition
    {
        Left,
        Right,
        Center,
    }
}
