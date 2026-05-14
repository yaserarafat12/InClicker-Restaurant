using UnityEngine;
using DG.Tweening;

namespace InUniverse.InResto
{
    /// <summary>
    /// Mengelola menu upgrade utama. 
    /// Bisa dibuka/tutup dengan animasi.
    /// </summary>
    public class UpgradeMenuManager : MonoBehaviour
    {
        public static UpgradeMenuManager Instance { get; private set; }

        [Header("UI Panels")]
        public RectTransform menuPanel;
        public PillarUpgradeSlot[] allSlots;

        private bool isOpen = false;
        private Vector2 closedPos;
        private Vector2 openPos;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Simpan posisi buat animasi slide
            openPos = menuPanel.anchoredPosition;
            closedPos = new Vector2(openPos.x, openPos.y - 1200); // Slide ke bawah
            menuPanel.anchoredPosition = closedPos;
        }

        public void ToggleMenu()
        {
            isOpen = !isOpen;
            
            if (isOpen)
            {
                menuPanel.DOAnchorPos(openPos, 0.5f).SetEase(Ease.OutBack);
                RefreshAllSlots();
            }
            else
            {
                menuPanel.DOAnchorPos(closedPos, 0.5f).SetEase(Ease.InBack);
            }
        }

        public void RefreshAllSlots()
        {
            foreach (var slot in allSlots)
            {
                if (slot != null) slot.UpdateUI();
            }
        }
    }
}
