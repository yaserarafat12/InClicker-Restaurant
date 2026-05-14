using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using System;

namespace InUniverse.InResto
{
    /// <summary>
    /// Menangani penyimpanan data menggunakan SQLite.
    /// Memastikan data aman biarpun aplikasi tertutup tiba-tiba.
    ///
    /// CHANGELOG v2:
    /// - Tambah kolom last_login_time di PlayerData (untuk OfflineIncomeManager)
    /// - Tambah SaveLastLoginTime() / GetLastLoginTime()
    /// - Tambah LoadPillarLevel() — kebalikan SavePillarLevel() yang sudah ada
    /// - Tambah LoadAllPillarsForLocation() — load ketiga pilar sekaligus (efisien, 1 query)
    /// - Semua query pakai parameterized command (cegah SQL injection dari nama pilar)
    /// - Tambah ResetSave() untuk debugging / new game
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private string dbName;
        private string dbPath;

        // ─────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                dbPath = Application.persistentDataPath + "/InRestoData.db";
                dbName = "URI=file:" + dbPath;

                InitializeDatabase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // ─────────────────────────────────────────────
        //  Database Initialization
        // ─────────────────────────────────────────────
        private void InitializeDatabase()
        {
            using (var conn = OpenConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    // Tabel PlayerData — kolom last_login_time ditambahkan (DEFAULT 0 agar backward compatible)
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS PlayerData (
                            id                   INTEGER PRIMARY KEY,
                            balance              REAL    DEFAULT 200000,
                            lifetime_earnings    REAL    DEFAULT 0,
                            current_location_id  INTEGER DEFAULT 1,
                            player_name          TEXT    DEFAULT 'Player',
                            last_login_time      INTEGER DEFAULT 0
                        );";
                    cmd.ExecuteNonQuery();

                    // Migrasi: tambah kolom jika table lama belum punya last_login_time
                    try
                    {
                        cmd.CommandText = "ALTER TABLE PlayerData ADD COLUMN last_login_time INTEGER DEFAULT 0;";
                        cmd.ExecuteNonQuery();
                    }
                    catch { /* Kolom sudah ada, abaikan */ }

                    // Tabel Pillars
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Pillars (
                            location_id  INTEGER,
                            pillar_type  TEXT,
                            level        INTEGER DEFAULT 1,
                            PRIMARY KEY (location_id, pillar_type)
                        );";
                    cmd.ExecuteNonQuery();

                    // Data awal player jika belum ada
                    cmd.CommandText = @"
                        INSERT OR IGNORE INTO PlayerData
                            (id, balance, lifetime_earnings, current_location_id, last_login_time)
                        VALUES (1, 200000, 0, 1, 0);";
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log($"<color=green>InResto DB: Ready at {dbPath}</color>");
        }

        // ─────────────────────────────────────────────
        //  Game State
        // ─────────────────────────────────────────────
        public void SaveGameState(double balance, double lifetime, int locationId)
        {
            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE PlayerData
                    SET balance = @bal, lifetime_earnings = @life, current_location_id = @loc
                    WHERE id = 1;";
                cmd.Parameters.AddWithValue("@bal",  balance);
                cmd.Parameters.AddWithValue("@life", lifetime);
                cmd.Parameters.AddWithValue("@loc",  locationId);
                cmd.ExecuteNonQuery();
            }
        }

        public void LoadGameState(out double balance, out double lifetime, out int locationId)
        {
            balance    = 200000;
            lifetime   = 0;
            locationId = 1;

            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT balance, lifetime_earnings, current_location_id FROM PlayerData WHERE id = 1;";
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        balance    = reader.GetDouble(0);
                        lifetime   = reader.GetDouble(1);
                        locationId = reader.GetInt32(2);
                    }
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Offline Income — Last Login Time
        // ─────────────────────────────────────────────
        public void SaveLastLoginTime(long unixTimestamp)
        {
            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE PlayerData SET last_login_time = @ts WHERE id = 1;";
                cmd.Parameters.AddWithValue("@ts", unixTimestamp);
                cmd.ExecuteNonQuery();
            }
        }

        public long GetLastLoginTime()
        {
            long ts = 0;
            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT last_login_time FROM PlayerData WHERE id = 1;";
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) ts = reader.GetInt64(0);
                }
            }
            return ts;
        }

        // ─────────────────────────────────────────────
        //  Pillar Levels
        // ─────────────────────────────────────────────
        public void SavePillarLevel(int locId, string pillarType, int level)
        {
            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT OR REPLACE INTO Pillars (location_id, pillar_type, level)
                    VALUES (@loc, @type, @level);";
                cmd.Parameters.AddWithValue("@loc",   locId);
                cmd.Parameters.AddWithValue("@type",  pillarType);
                cmd.Parameters.AddWithValue("@level", level);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Load level satu pilar. Kembalikan 1 (default) jika belum pernah disimpan.</summary>
        public int LoadPillarLevel(int locId, string pillarType)
        {
            int level = 1;
            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT level FROM Pillars WHERE location_id = @loc AND pillar_type = @type;";
                cmd.Parameters.AddWithValue("@loc",  locId);
                cmd.Parameters.AddWithValue("@type", pillarType);
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) level = reader.GetInt32(0);
                }
            }
            return level;
        }

        /// <summary>Load ketiga pilar sekaligus untuk satu lokasi — 1 query lebih efisien dari 3 query.</summary>
        public void LoadAllPillarsForLocation(int locId, out int dapurLevel, out int makanLevel, out int kasirLevel)
        {
            dapurLevel = 1;
            makanLevel = 1;
            kasirLevel = 1;

            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT pillar_type, level FROM Pillars WHERE location_id = @loc;";
                cmd.Parameters.AddWithValue("@loc", locId);
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string type = reader.GetString(0);
                        int    lvl  = reader.GetInt32(1);
                        switch (type)
                        {
                            case "Dapur":     dapurLevel = lvl; break;
                            case "AreaMakan": makanLevel = lvl; break;
                            case "Kasir":     kasirLevel = lvl; break;
                        }
                    }
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Debug / QA
        // ─────────────────────────────────────────────
        [ContextMenu("Reset Save Data")]
        public void ResetSave()
        {
            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM PlayerData; DELETE FROM Pillars;";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "INSERT INTO PlayerData (id, balance, lifetime_earnings, current_location_id, last_login_time) VALUES (1, 200000, 0, 1, 0);";
                cmd.ExecuteNonQuery();
            }
            Debug.Log("<color=red>InResto DB: Save data RESET!</color>");
        }

        // ─────────────────────────────────────────────
        //  Private Helper
        // ─────────────────────────────────────────────
        private SqliteConnection OpenConnection()
        {
            var conn = new SqliteConnection(dbName);
            conn.Open();
            return conn;
        }
    }
}
