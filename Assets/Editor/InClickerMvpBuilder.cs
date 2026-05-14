using System.IO;
using System.Reflection;
using InUniverse.InResto;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace InUniverse.InResto.EditorTools
{
    public static class InClickerMvpBuilder
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";

        public static void Build()
        {
            EnsureFolders();
            CreatePlaceholderSprites();
            var panels = CreateStoryPanels();
            BuildScene(panels);
            ConfigureProject();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("InClicker MVP scene generated.");
        }

        public static void BuildAndroidDevelopment()
        {
            Build();
            Directory.CreateDirectory("Builds/Android");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Android/InClickerRestaurant-dev.apk",
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"Android build result: {report.summary.result} at {options.locationPathName}");
        }

        private static void EnsureFolders()
        {
            foreach (var path in new[]
            {
                "Assets/_IncomingArt",
                "Assets/Art/Characters",
                "Assets/Art/FoodIcons",
                "Assets/Art/Backgrounds",
                "Assets/Art/UI",
                "Assets/Data/VN",
                "Assets/Editor",
                "Assets/Scenes",
                "Assets/Scripts/MVP",
                "Assets/Scripts/ThirdParty"
            })
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void CreatePlaceholderSprites()
        {
            CreateSprite("Assets/Art/Backgrounds/placeholder_warteg_vertical.png", 768, 1365, new Color(0.35f, 0.18f, 0.07f), new Color(0.95f, 0.62f, 0.20f));
            CreateSprite("Assets/Art/UI/button_primary.png", 256, 96, new Color(0.95f, 0.58f, 0.10f), new Color(0.45f, 0.20f, 0.04f));
            CreateSprite("Assets/Art/UI/button_upgrade.png", 256, 96, new Color(0.35f, 0.70f, 0.22f), new Color(0.10f, 0.30f, 0.08f));
            CreateSprite("Assets/Art/UI/button_rush.png", 256, 96, new Color(0.90f, 0.22f, 0.08f), new Color(0.45f, 0.08f, 0.02f));
            CreateSprite("Assets/Art/FoodIcons/placeholder_nasi_galau.png", 512, 512, new Color(0.98f, 0.88f, 0.65f), new Color(0.62f, 0.18f, 0.08f));
        }

        private static void CreateSprite(string path, int width, int height, Color fill, Color accent)
        {
            if (File.Exists(path)) return;

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float t = (float)y / Mathf.Max(1, height - 1);
                    Color c = Color.Lerp(fill * 0.75f, fill, t);
                    if (x < 10 || y < 10 || x > width - 11 || y > height - 11) c = accent;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.SaveAndReimport();
        }

        private static VNChapterData[] CreateStoryPanels()
        {
            return new[]
            {
                CreatePanel("0.1", "Buka Warteg", VNTriggerType.Manual, 0, new[]
                {
                    new VNDialog { speakerName = "Nenek", dialogueText = "Kita mulai dari kecil dulu. Yang penting hangat, jujur, dan bikin orang balik lagi.", position = SpeakerPosition.Left },
                    new VNDialog { speakerName = "Kamu", dialogueText = "Hari ini warteg buka. Besok kita bikin lebih rame.", position = SpeakerPosition.Right }
                }),
                CreatePanel("1.1", "Pelanggan Pertama", VNTriggerType.Revenue, 1000000, new[]
                {
                    new VNDialog { speakerName = "Pelanggan", dialogueText = "Sambalnya serius. Besok gue ajak temen kantor.", position = SpeakerPosition.Left }
                }),
                CreatePanel("1.2", "Mimpi Cabang", VNTriggerType.Revenue, 10000000, new[]
                {
                    new VNDialog { speakerName = "Nenek", dialogueText = "Kalau satu meja bisa penuh, suatu hari satu kota juga bisa kenal.", position = SpeakerPosition.Center }
                })
            };
        }

        private static VNChapterData CreatePanel(string id, string title, VNTriggerType trigger, double value, VNDialog[] dialogs)
        {
            string path = $"Assets/Data/VN/VNPanel_{id.Replace('.', '_')}.asset";
            var panel = AssetDatabase.LoadAssetAtPath<VNChapterData>(path);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<VNChapterData>();
                AssetDatabase.CreateAsset(panel, path);
            }
            panel.panelId = id;
            panel.chapterTitle = title;
            panel.triggerType = trigger;
            panel.triggerValue = value;
            panel.dialogs = dialogs;
            panel.isSkippableOnReplay = true;
            EditorUtility.SetDirty(panel);
            return panel;
        }

        private static void BuildScene(VNChapterData[] panels)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Main";

            var cameraObj = new GameObject("Main Camera");
            var camera = cameraObj.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.backgroundColor = new Color(0.11f, 0.06f, 0.03f);
            cameraObj.tag = "MainCamera";

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            var managers = new GameObject("[MANAGERS]");
            var save = managers.AddComponent<SaveManager>();
            var economy = managers.AddComponent<EconomyManager>();
            var pillarManager = managers.AddComponent<PillarManager>();
            var rush = managers.AddComponent<RushHourManager>();
            var progression = managers.AddComponent<ProgressionManager>();
            var offline = managers.AddComponent<OfflineIncomeManager>();
            var visual = managers.AddComponent<VisualManager>();
            var vn = managers.AddComponent<VNBridge>();
            managers.AddComponent<GameManager>();

            _ = save; _ = economy; _ = progression; _ = offline;

            var pillarRoot = new GameObject("[PILLARS]");
            var dapur = new GameObject("Dapur").AddComponent<DapurController>();
            dapur.transform.SetParent(pillarRoot.transform);
            var makan = new GameObject("Area Makan").AddComponent<MakanController>();
            makan.transform.SetParent(pillarRoot.transform);
            var kasir = new GameObject("Kasir").AddComponent<KasirController>();
            kasir.transform.SetParent(pillarRoot.transform);
            pillarManager.dapur = dapur;
            pillarManager.makan = makan;
            pillarManager.kasir = kasir;

            var canvas = CreateCanvas();
            var root = CreatePanel(canvas.transform, "Root", new Color(0.10f, 0.06f, 0.03f), StretchFull());

            var topBar = CreatePanel(root.transform, "Top Bar", new Color(0.08f, 0.05f, 0.03f, 0.96f), AnchorTop(98));
            var moneyText = CreateText(topBar.transform, "Money Text", "Rp 200.000", 24, TextAnchor.MiddleLeft, new Color(1f, 0.84f, 0.22f), Rect(18, -12, 190, 44, new Vector2(0, 1), new Vector2(0, 1)));
            var bottleneckText = CreateText(topBar.transform, "Bottleneck Text", "Semua lancar", 13, TextAnchor.MiddleRight, new Color(0.86f, 0.92f, 0.78f), Rect(-248, -52, 230, 30, new Vector2(1, 1), new Vector2(1, 1)));

            var energyRoot = CreatePanel(root.transform, "Rush Energy", new Color(0f, 0f, 0f, 0f), Rect(16, -100, -16, 36, new Vector2(0, 1), new Vector2(1, 1)));
            var energySlider = CreateSlider(energyRoot.transform);
            var energyFill = energySlider.fillRect.GetComponent<Image>();

            var scenePanel = CreatePanel(root.transform, "Tap Warteg Scene", new Color(0.33f, 0.16f, 0.06f), Rect(16, 148, -16, -284, new Vector2(0, 0), new Vector2(1, 1)));
            var sceneButton = scenePanel.gameObject.AddComponent<Button>();
            sceneButton.transition = Selectable.Transition.None;
            scenePanel.gameObject.AddComponent<MvpTapArea>();
            CreateText(scenePanel.transform, "Scene Title", "Warteg Gang Sempit", 28, TextAnchor.UpperCenter, new Color(1f, 0.86f, 0.58f), Rect(0, -28, 0, 64, new Vector2(0, 1), new Vector2(1, 1)));
            CreateText(scenePanel.transform, "Scene Hint", "Tap area ini buat isi Rush Hour", 16, TextAnchor.LowerCenter, new Color(1f, 0.72f, 0.38f), Rect(0, 0, 0, 54, new Vector2(0, 0), new Vector2(1, 0)));

            var upgradePanel = CreatePanel(root.transform, "Upgrade Panel", new Color(0.12f, 0.08f, 0.05f, 0.98f), AnchorBottom(266));
            CreateUpgradeSlot(upgradePanel.transform, "Dapur Slot", dapur, 14, -18);
            CreateUpgradeSlot(upgradePanel.transform, "Area Makan Slot", makan, 14, -96);
            CreateUpgradeSlot(upgradePanel.transform, "Kasir Slot", kasir, 14, -174);

            var offlinePanel = CreateOfflinePanel(root.transform);
            var vnOverlay = CreateVnOverlay(root.transform);

            visual.moneyText = moneyText;
            visual.bottleneckText = bottleneckText;

            SetField(rush, "energyBar", energySlider);
            SetField(rush, "energyBarFill", energyFill);
            SetField(rush, "gameplayArea", scenePanel.rectTransform);

            SetField(vn, "allPanels", panels);
            SetField(vn, "vnOverlay", vnOverlay.root.gameObject);
            SetField(vn, "vnAlpha", vnOverlay.group);
            SetField(vn, "backgroundImage", vnOverlay.background);
            SetField(vn, "speakerNameText", vnOverlay.speaker);
            SetField(vn, "dialogueText", vnOverlay.dialogue);
            SetField(vn, "nextButton", vnOverlay.nextButton);
            SetField(vn, "skipButton", vnOverlay.skipButton);

            SetField(offlinePanel.ui, "rootPanel", offlinePanel.group);
            SetField(offlinePanel.ui, "darkOverlay", offlinePanel.darkOverlay);
            SetField(offlinePanel.ui, "brankasContainer", offlinePanel.box);
            SetField(offlinePanel.ui, "durationText", offlinePanel.durationText);
            SetField(offlinePanel.ui, "amountText", offlinePanel.amountText);
            SetField(offlinePanel.ui, "claimNormalButton", offlinePanel.claimButton);
            SetField(offlinePanel.ui, "claimNormalText", offlinePanel.claimButtonText);
            SetField(offlinePanel.ui, "claimDoubleButton", offlinePanel.doubleButton);
            SetField(offlinePanel.ui, "claimDoubleText", offlinePanel.doubleButtonText);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        private static Canvas CreateCanvas()
        {
            var canvasObj = new GameObject("Canvas", typeof(RectTransform));
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390, 844);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static Image CreatePanel(Transform parent, string name, Color color, RectSpec spec)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            Apply(rect, spec);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(Transform parent, string name, string value, int size, TextAnchor anchor, Color color, RectSpec spec)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            Apply(rect, spec);
            var text = go.GetComponent<Text>();
            text.text = value;
            text.font = GetFont();
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(10, size - 8);
            text.resizeTextMaxSize = size;
            return text;
        }

        private static Slider CreateSlider(Transform parent)
        {
            var sliderObj = new GameObject("Energy Slider", typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(parent, false);
            Apply(sliderObj.GetComponent<RectTransform>(), StretchFull());
            var bg = CreatePanel(sliderObj.transform, "Background", new Color(0.18f, 0.16f, 0.14f), StretchFull());
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            Apply(fillArea.GetComponent<RectTransform>(), Rect(3, 3, -3, -3, Vector2.zero, Vector2.one));
            var fill = CreatePanel(fillArea.transform, "Fill", new Color(0.20f, 0.62f, 1f), StretchFull());
            var slider = sliderObj.GetComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 0;
            slider.targetGraphic = bg;
            slider.fillRect = fill.rectTransform;
            return slider;
        }

        private static void CreateUpgradeSlot(Transform parent, string name, PillarBase pillar, float left, float top)
        {
            var card = CreatePanel(parent, name, new Color(0.18f, 0.12f, 0.07f), Rect(left, top, -14, 66, new Vector2(0, 1), new Vector2(1, 1)));
            var slot = card.gameObject.AddComponent<PillarUpgradeSlot>();
            slot.targetPillar = pillar;
            slot.canvasGroup = card.gameObject.AddComponent<CanvasGroup>();
            slot.pillarNameText = CreateText(card.transform, "Name", pillar.pillarName, 16, TextAnchor.MiddleLeft, Color.white, Rect(12, -8, 156, 24, new Vector2(0, 1), new Vector2(0, 1)));
            slot.levelText = CreateText(card.transform, "Level", "Lv. 1", 12, TextAnchor.MiddleLeft, new Color(0.80f, 0.72f, 0.62f), Rect(12, -33, 130, 22, new Vector2(0, 1), new Vector2(0, 1)));
            slot.costText = CreateText(card.transform, "Cost", "Rp", 13, TextAnchor.MiddleRight, new Color(1f, 0.84f, 0.28f), Rect(-170, -20, 82, 32, new Vector2(1, 1), new Vector2(1, 1)));
            var buttonImage = CreatePanel(card.transform, "Upgrade Button", new Color(0.30f, 0.70f, 0.22f), Rect(-82, -11, 70, 44, new Vector2(1, 1), new Vector2(1, 1)));
            var button = buttonImage.gameObject.AddComponent<Button>();
            slot.upgradeButton = button;
            slot.buttonImage = buttonImage;
            CreateText(buttonImage.transform, "Button Text", "UP", 15, TextAnchor.MiddleCenter, Color.white, StretchFull());
        }

        private static OfflinePanelRefs CreateOfflinePanel(Transform parent)
        {
            var root = CreatePanel(parent, "Offline Claim Popup", new Color(0f, 0f, 0f, 0.72f), StretchFull());
            var group = root.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0;
            group.interactable = false;
            group.blocksRaycasts = false;
            var ui = root.gameObject.AddComponent<OfflineClaimUI>();
            var box = CreatePanel(root.transform, "Claim Box", new Color(0.25f, 0.13f, 0.05f), Rect(30, -245, -30, 300, new Vector2(0, 1), new Vector2(1, 1))).rectTransform;
            var duration = CreateText(box, "Duration", "Warteg lo jalan offline", 16, TextAnchor.MiddleCenter, Color.white, Rect(18, -28, -18, 48, new Vector2(0, 1), new Vector2(1, 1)));
            var amount = CreateText(box, "Amount", "Rp 0", 26, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.25f), Rect(18, -96, -18, 64, new Vector2(0, 1), new Vector2(1, 1)));
            var claim = CreatePanel(box, "Claim Normal", new Color(0.35f, 0.70f, 0.22f), Rect(20, 24, -20, 48, Vector2.zero, new Vector2(1, 0)));
            var claimButton = claim.gameObject.AddComponent<Button>();
            var claimText = CreateText(claim.transform, "Text", "KLAIM", 16, TextAnchor.MiddleCenter, Color.white, StretchFull());
            var dbl = CreatePanel(box, "Claim Double", new Color(0.95f, 0.58f, 0.10f), Rect(20, 82, -20, 54, Vector2.zero, new Vector2(1, 0)));
            var dblButton = dbl.gameObject.AddComponent<Button>();
            var dblText = CreateText(dbl.transform, "Text", "KLAIM 2X", 15, TextAnchor.MiddleCenter, Color.white, StretchFull());
            return new OfflinePanelRefs { ui = ui, group = group, darkOverlay = root, box = box, durationText = duration, amountText = amount, claimButton = claimButton, claimButtonText = claimText, doubleButton = dblButton, doubleButtonText = dblText };
        }

        private static VnRefs CreateVnOverlay(Transform parent)
        {
            var root = CreatePanel(parent, "VN Overlay", new Color(0.03f, 0.02f, 0.02f, 0.94f), StretchFull());
            root.gameObject.SetActive(false);
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var bg = CreatePanel(root.transform, "Background", new Color(0.22f, 0.12f, 0.05f), Rect(0, 0, 0, -230, Vector2.zero, Vector2.one));
            var speaker = CreateText(root.transform, "Speaker", "Nenek", 18, TextAnchor.MiddleLeft, new Color(1f, 0.82f, 0.34f), Rect(28, 166, -28, 34, Vector2.zero, new Vector2(1, 0)));
            var dialogue = CreateText(root.transform, "Dialogue", "Dialog", 18, TextAnchor.UpperLeft, Color.white, Rect(28, 54, -28, 112, Vector2.zero, new Vector2(1, 0)));
            var next = CreatePanel(root.transform, "Next Button", new Color(0.95f, 0.58f, 0.10f), Rect(-132, 20, 110, 42, new Vector2(1, 0), new Vector2(1, 0)));
            var nextButton = next.gameObject.AddComponent<Button>();
            CreateText(next.transform, "Text", "Next", 15, TextAnchor.MiddleCenter, Color.white, StretchFull());
            var skip = CreatePanel(root.transform, "Skip Button", new Color(0.24f, 0.18f, 0.14f), Rect(22, -56, 90, 36, new Vector2(0, 1), new Vector2(0, 1)));
            var skipButton = skip.gameObject.AddComponent<Button>();
            CreateText(skip.transform, "Text", "Skip", 13, TextAnchor.MiddleCenter, Color.white, StretchFull());
            return new VnRefs { root = root.rectTransform, group = group, background = bg, speaker = speaker, dialogue = dialogue, nextButton = nextButton, skipButton = skipButton };
        }

        private static Font GetFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            field?.SetValue(target, value);
            if (target is Object obj) EditorUtility.SetDirty(obj);
        }

        private static void ConfigureProject()
        {
            PlayerSettings.productName = "InClicker - Restaurant";
            PlayerSettings.companyName = "InUniverse";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.inuniverse.inclickerrestaurant");
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        }

        private static RectSpec StretchFull() => Rect(0, 0, 0, 0, Vector2.zero, Vector2.one);
        private static RectSpec AnchorTop(float height) => Rect(0, -height, 0, height, new Vector2(0, 1), new Vector2(1, 1));
        private static RectSpec AnchorBottom(float height) => Rect(0, 0, 0, height, Vector2.zero, new Vector2(1, 0));
        private static RectSpec Rect(float left, float bottom, float right, float top, Vector2 min, Vector2 max) => new RectSpec(left, bottom, right, top, min, max);

        private static void Apply(RectTransform rect, RectSpec spec)
        {
            rect.anchorMin = spec.anchorMin;
            rect.anchorMax = spec.anchorMax;
            rect.offsetMin = new Vector2(spec.left, spec.bottom);
            rect.offsetMax = new Vector2(spec.right, spec.top);
        }

        private struct RectSpec
        {
            public float left, bottom, right, top;
            public Vector2 anchorMin, anchorMax;
            public RectSpec(float left, float bottom, float right, float top, Vector2 anchorMin, Vector2 anchorMax)
            {
                this.left = left; this.bottom = bottom; this.right = right; this.top = top;
                this.anchorMin = anchorMin; this.anchorMax = anchorMax;
            }
        }

        private struct VnRefs
        {
            public RectTransform root;
            public CanvasGroup group;
            public Image background;
            public Text speaker;
            public Text dialogue;
            public Button nextButton;
            public Button skipButton;
        }

        private struct OfflinePanelRefs
        {
            public OfflineClaimUI ui;
            public CanvasGroup group;
            public Image darkOverlay;
            public RectTransform box;
            public Text durationText;
            public Text amountText;
            public Button claimButton;
            public Text claimButtonText;
            public Button doubleButton;
            public Text doubleButtonText;
        }
    }
}
