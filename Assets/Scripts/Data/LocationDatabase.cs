using UnityEngine;

namespace InUniverse.InResto
{
    /// <summary>
    /// Database statis 10 lokasi InResto — GDD §6.1 "Location Progression"
    ///
    /// Dipakai oleh ProgressionManager untuk lookup data lokasi tanpa perlu
    /// assign ScriptableObject di Inspector (lebih mudah di-setup untuk prototype).
    /// Dalam proyek final, bisa diganti dengan array LocationData[] di Inspector.
    /// </summary>
    public static class LocationDatabase
    {
        public struct LocationInfo
        {
            public int    Id;
            public string Name;
            public string Description;
            public int    MaxPillarLevel;      // Max level semua pilar di lokasi ini
            public double ExpandCost;          // Biaya pindah ke sini dari lokasi sebelumnya
            public double RevenueMultiplier;   // Multiplier income di lokasi ini
            public string VNPanelTrigger;      // Panel VN yang di-unlock saat arrive
            public Color  UIThemeColor;        // Warna tema UI di lokasi ini
        }

        // ─── 10 Lokasi sesuai GDD §6.1 Table ─────────────────────────────
        public static readonly LocationInfo[] All = new LocationInfo[]
        {
            new LocationInfo
            {
                Id               = 1,
                Name             = "Warteg Gang Sempit",
                Description      = "Modal Rp 200 ribu. Kompor minyak tanah. Mimpi sebesar langit.",
                MaxPillarLevel   = 20,
                ExpandCost       = 0,          // Starting location
                RevenueMultiplier= 1.0,
                VNPanelTrigger   = "0.1",      // Act 0 awal
                UIThemeColor     = new Color(0.55f, 0.35f, 0.15f), // Coklat tua
            },
            new LocationInfo
            {
                Id               = 2,
                Name             = "Warteg Pinggir Jalan",
                Description      = "Meja kayu 2 buah, kursi plastik 6, dan semangat yang tak padam.",
                MaxPillarLevel   = 25,
                ExpandCost       = 500_000_000,
                RevenueMultiplier= 5.0,
                VNPanelTrigger   = "2.1",
                UIThemeColor     = new Color(0.65f, 0.45f, 0.20f),
            },
            new LocationInfo
            {
                Id               = 3,
                Name             = "Warung Rakyat",
                Description      = "Orang-orang mulai kenal nama lo. Antrean mulai panjang.",
                MaxPillarLevel   = 35,
                ExpandCost       = 50_000_000_000,
                RevenueMultiplier= 10.0,
                VNPanelTrigger   = "2.13",
                UIThemeColor     = new Color(0.70f, 0.50f, 0.20f),
            },
            new LocationInfo
            {
                Id               = 4,
                Name             = "Warung Keluarga",
                Description      = "AC pertama. Meja kayu yang bagus. Nenek ikut bangga.",
                MaxPillarLevel   = 50,
                ExpandCost       = 1_000_000_000_000,
                RevenueMultiplier= 25.0,
                VNPanelTrigger   = "4.1",
                UIThemeColor     = new Color(0.80f, 0.55f, 0.20f),
            },
            new LocationInfo
            {
                Id               = 5,
                Name             = "Restoran Sederhana",
                Description      = "Daftar menu dicetak di laminasi. Seragam karyawan pertama.",
                MaxPillarLevel   = 70,
                ExpandCost       = 20_000_000_000_000,
                RevenueMultiplier= 50.0,
                VNPanelTrigger   = "4.5",
                UIThemeColor     = new Color(0.85f, 0.60f, 0.25f),
            },
            new LocationInfo
            {
                Id               = 6,
                Name             = "Restoran Kelas Menengah",
                Description      = "Reservasi sudah diperlukan. Majalah kuliner mulai melirik.",
                MaxPillarLevel   = 90,
                ExpandCost       = 400_000_000_000_000,
                RevenueMultiplier= 100.0,
                VNPanelTrigger   = "5.1",
                UIThemeColor     = new Color(0.90f, 0.65f, 0.25f),
            },
            new LocationInfo
            {
                Id               = 7,
                Name             = "Fine Dining",
                Description      = "Meja marble. Chandelier. Michelin star pertama Indonesia?",
                MaxPillarLevel   = 120,
                ExpandCost       = 8_000_000_000_000_000,
                RevenueMultiplier= 250.0,
                VNPanelTrigger   = "5.4",
                UIThemeColor     = new Color(0.95f, 0.80f, 0.30f),
            },
            new LocationInfo
            {
                Id               = 8,
                Name             = "Franchise Nasional",
                Description      = "30 cabang. Nama lo ada di billboard tol. Nenek nangis bangga.",
                MaxPillarLevel   = 150,
                ExpandCost       = 160_000_000_000_000_000,
                RevenueMultiplier= 500.0,
                VNPanelTrigger   = "6.4",
                UIThemeColor     = new Color(0.98f, 0.85f, 0.35f),
            },
            new LocationInfo
            {
                Id               = 9,
                Name             = "Cabang Internasional – Tokyo",
                Description      = "Orang Jepang antri 2 jam buat makan Nasi Galau lo. Sugoi.",
                MaxPillarLevel   = 200,
                ExpandCost       = 3_200_000_000_000_000_000,
                RevenueMultiplier= 1000.0,
                VNPanelTrigger   = "7.2",
                UIThemeColor     = new Color(1.0f, 0.90f, 0.40f),
            },
            new LocationInfo
            {
                Id               = 10,
                Name             = "World Domination",
                Description      = "66 cabang. 5 negara. Dari gang sempit ke seluruh dunia. Terima kasih, Nek.",
                MaxPillarLevel   = 999,        // Infinite grind
                ExpandCost       = 0,          // Final location, no expand
                RevenueMultiplier= 2500.0,
                VNPanelTrigger   = "7.8",      // Ending panel
                UIThemeColor     = new Color(1.0f, 0.95f, 0.50f),
            },
        };

        /// <summary>Get lokasi berdasarkan 0-based index (index 0 = Lokasi 1).</summary>
        public static LocationInfo Get(int index)
        {
            index = Mathf.Clamp(index, 0, All.Length - 1);
            return All[index];
        }

        /// <summary>Apakah index ini lokasi terakhir?</summary>
        public static bool IsLastLocation(int index) => index >= All.Length - 1;

        /// <summary>Total jumlah lokasi.</summary>
        public static int Count => All.Length;
    }
}
