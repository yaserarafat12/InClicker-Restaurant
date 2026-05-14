using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace InUniverse.InResto
{
    /// <summary>
    /// Mengelola customer sprites di area makan — GDD §4.3 "Customer Interaction"
    ///
    /// Fitur:
    ///   - Spawn customer sprite sesuai kapasitas pilar Area Makan
    ///   - Customer tap → bubble chat muncul (feedback bottleneck sesuai GDD)
    ///   - Customer reaction berubah berdasarkan kondisi bottleneck saat ini
    ///   - Customer "leave" animation jika antrean penuh (lost revenue visual)
    ///   - Rush Hour: customer makan lebih cepat (animasi speed-up)
    ///
    /// GDD Quote §4.3: "Customer = clickable sprite. Tap → bubble chat muncul.
    ///   Content: happy/satisfied/complain sesuai bottleneck."
    /// </summary>
    public class CustomerManager : MonoBehaviour
    {
        public static CustomerManager Instance { get; private set; }

        // ─── Prefab & Spawn ─────────────────────────────────────────────
        [Header("Customer Setup")]
        [SerializeField] private GameObject customerPrefab;     // Prefab dengan SpriteRenderer + CanvasGroup
        [SerializeField] private Transform  customerContainer;  // Parent semua customer sprites
        [SerializeField] private Transform[] seatPositions;     // Array posisi kursi di scene

        // ─── Bubble Chat ────────────────────────────────────────────────
        [Header("Chat Bubble")]
        [SerializeField] private GameObject bubblePrefab;       // Prefab bubble (Text + background)
        [SerializeField] private float      bubbleDuration = 3f;

        // ─── State ──────────────────────────────────────────────────────
        private List<CustomerInstance> activeCustomers = new List<CustomerInstance>();
        private int                    currentCapacity = 0;

        // ─── Dialog Lines per Kondisi (GDD §4.3) ────────────────────────
        private static readonly string[] HappyLines = {
            "😋 Enak banget! Bumbu neneknya kerasa!",
            "😊 Porsinya pas, harga terjangkau",
            "🤤 Mantul! Besok gw balik lagi",
            "💯 5 bintang! Recommended banget!",
            "😍 Ini sih favorit gw sekarang",
        };

        private static readonly string[] SlowKitchenLines = {
            "😐 Lama sih nunggu makanannya...",
            "⏰ Udah 10 menit nunggu, mana makanannya?",
            "😑 Dapur kewalahan kayaknya",
        };

        private static readonly string[] SlowCashierLines = {
            "😡 Kasirnya lambat banget!",
            "💸 Udah selesai makan, masih antri bayar",
            "🤦 Queue panjang banget di kasir",
        };

        private static readonly string[] FullLines = {
            "😤 Gak ada tempat duduk, gw pergi dulu",
            "🚶 Antrian panjang banget...",
            "❌ Penuh! Kapan tambahin meja nih?",
        };

        // ─── Inner class ─────────────────────────────────────────────────
        public class CustomerInstance
        {
            public GameObject   Go;
            public SpriteRenderer Sprite;
            public int          SeatIndex;
            public bool         IsTapped;
        }

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
            // Subscribe ke capacity changes
            EconomyManager.OnUpgradePerformed += OnUpgrade;
            RushHourManager.OnRushHourStateChanged += OnRushHourChanged;

            RefreshCustomerCount();
        }

        private void OnDestroy()
        {
            EconomyManager.OnUpgradePerformed -= OnUpgrade;
            RushHourManager.OnRushHourStateChanged -= OnRushHourChanged;
        }

        // ─────────────────────────────────────────────────────────────
        //  Spawn / Despawn
        // ─────────────────────────────────────────────────────────────
        private void OnUpgrade()
        {
            // Cek apakah kapasitas berubah setelah upgrade Area Makan
            if (PillarManager.Instance?.makan != null)
                RefreshCustomerCount();
        }

        private void RefreshCustomerCount()
        {
            if (PillarManager.Instance?.makan == null) return;

            int newCapacity = PillarManager.Instance.makan.GetCapacity();
            if (newCapacity == currentCapacity) return;

            int diff = newCapacity - currentCapacity;
            currentCapacity = newCapacity;

            if (diff > 0)
            {
                // Spawn customer baru
                for (int i = 0; i < diff; i++)
                    SpawnCustomer();
            }
            else
            {
                // Despawn customer berlebih
                for (int i = 0; i < -diff && activeCustomers.Count > 0; i++)
                    DespawnLastCustomer();
            }
        }

        private void SpawnCustomer()
        {
            if (customerPrefab == null || customerContainer == null) return;

            int seatIdx = activeCustomers.Count;
            Vector3 pos = seatIdx < seatPositions.Length
                ? seatPositions[seatIdx].position
                : customerContainer.position + new Vector3(Random.Range(-2f, 2f), 0, 0);

            GameObject go = Instantiate(customerPrefab, pos, Quaternion.identity, customerContainer);

            // Entry animation: fade in + bob up
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = 0; cg.DOFade(1f, 0.4f); }
            go.transform.DOPunchPosition(new Vector3(0, 10, 0), 0.3f);

            var customer = new CustomerInstance
            {
                Go        = go,
                Sprite    = go.GetComponent<SpriteRenderer>(),
                SeatIndex = seatIdx,
                IsTapped  = false,
            };
            activeCustomers.Add(customer);

            // Setup tap callback
            var tapHandler = go.AddComponent<CustomerTapHandler>();
            tapHandler.Initialize(customer, this);
        }

        private void DespawnLastCustomer()
        {
            if (activeCustomers.Count == 0) return;

            var last = activeCustomers[activeCustomers.Count - 1];
            activeCustomers.RemoveAt(activeCustomers.Count - 1);

            // Leave animation: slide keluar + fade
            last.Go.transform.DOLocalMoveX(300f, 0.5f).SetEase(Ease.InBack);
            CanvasGroup cg = last.Go.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.DOFade(0f, 0.5f).OnComplete(() => Destroy(last.Go));
            else
                Destroy(last.Go, 0.6f);
        }

        // ─────────────────────────────────────────────────────────────
        //  Tap Handler — dipanggil dari CustomerTapHandler
        // ─────────────────────────────────────────────────────────────
        public void OnCustomerTapped(CustomerInstance customer)
        {
            if (customer.IsTapped) return;
            customer.IsTapped = true;

            string line = GetContextualLine();
            ShowBubble(customer.Go.transform.position, line);

            // Reset flag setelah bubble selesai
            DOVirtual.DelayedCall(bubbleDuration + 0.5f, () =>
            {
                if (customer != null) customer.IsTapped = false;
            });
        }

        /// <summary>Pilih dialog sesuai kondisi bottleneck saat ini — GDD §4.3.</summary>
        private string GetContextualLine()
        {
            if (PillarManager.Instance == null)
                return HappyLines[Random.Range(0, HappyLines.Length)];

            BottleneckSeverity severity;
            string status = PillarManager.Instance.GetBottleneckStatus(out severity);

            if (severity == BottleneckSeverity.Critical)
            {
                // Lihat pilar mana yang bermasalah
                double p = PillarManager.Instance.dapur.GetProductionRate();
                double c = PillarManager.Instance.makan.GetCapacityRate();
                double k = PillarManager.Instance.kasir.GetProcessingRate();
                double mn = System.Math.Min(p, System.Math.Min(c, k));

                if (mn == p) return SlowKitchenLines[Random.Range(0, SlowKitchenLines.Length)];
                if (mn == k) return SlowCashierLines[Random.Range(0, SlowCashierLines.Length)];
                return FullLines[Random.Range(0, FullLines.Length)];
            }

            return HappyLines[Random.Range(0, HappyLines.Length)];
        }

        // ─────────────────────────────────────────────────────────────
        //  Bubble Chat
        // ─────────────────────────────────────────────────────────────
        private void ShowBubble(Vector3 worldPos, string text)
        {
            if (bubblePrefab == null) return;

            GameObject bubble = Instantiate(bubblePrefab, worldPos + Vector3.up * 1.5f, Quaternion.identity, customerContainer);
            Text bubbleText   = bubble.GetComponentInChildren<Text>();
            if (bubbleText != null) bubbleText.text = text;

            CanvasGroup cg = bubble.GetComponent<CanvasGroup>() ?? bubble.AddComponent<CanvasGroup>();
            cg.alpha = 0;
            cg.DOFade(1f, 0.2f);

            // Float upward then fade out
            bubble.transform.DOLocalMoveY(bubble.transform.localPosition.y + 30f, bubbleDuration)
                .SetEase(Ease.OutQuad);
            DOVirtual.DelayedCall(bubbleDuration - 0.5f, () =>
                cg.DOFade(0f, 0.5f).OnComplete(() => Destroy(bubble)));
        }

        // ─────────────────────────────────────────────────────────────
        //  Rush Hour Response
        // ─────────────────────────────────────────────────────────────
        private void OnRushHourChanged(bool isActive)
        {
            // Percepat animasi semua customer saat Rush Hour
            float speed = isActive ? 2.5f : 1.0f;
            foreach (var c in activeCustomers)
            {
                if (c.Go != null)
                    c.Go.GetComponent<Animator>()?.SetFloat("Speed", speed);
            }
        }
    }

    /// <summary>
    /// Helper component yang ditaruh di tiap customer GameObject.
    /// Mendeteksi tap dan meneruskan ke CustomerManager.
    /// </summary>
    public class CustomerTapHandler : MonoBehaviour
    {
        private CustomerManager.CustomerInstance _customer;
        private CustomerManager _manager;

        public void Initialize(CustomerManager.CustomerInstance customer, CustomerManager manager)
        {
            // Akses private nested class butuh workaround — pakai field internal di CustomerManager
            // Disederhanakan: store reference langsung
            _customer = customer;
            _manager = manager;
        }

        private void OnMouseDown()
        {
            // Dipanggil saat sprite diklik di Unity (perlu 2D Physics + Collider)
            _manager?.OnCustomerTapped(_customer);
        }
    }
}
