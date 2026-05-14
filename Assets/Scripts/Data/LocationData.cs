using UnityEngine;

namespace InUniverse.InResto
{
    [CreateAssetMenu(fileName = "NewLocation", menuName = "InResto/Location Data")]
    public class LocationData : ScriptableObject
    {
        [Header("Identity")]
        public int locationID;
        public string locationName;
        public string locationDescription;

        [Header("Visuals")]
        public Sprite backgroundSprite; // Gambar statis lokasi (Warteg, Resto, dsb)
        public Sprite emptyRoomOverlay; // Opsional: jika butuh layer tambahan

        [Header("Economy Settings")]
        public double unlockCost;
        public double baseIncomeMultiplier = 1.0;
        public double upgradeCostMultiplier = 1.15; // Berapa persen harga naik tiap level

        [Header("Initial Pillar States")]
        public int startingDapurLevel = 1;
        public int startingMakanLevel = 1;
        public int startingKasirLevel = 1;

        [Header("Ambience")]
        public AudioClip backgroundMusic;
        public Color uiThemeColor = Color.white;
    }
}
