using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using UnboundLib;
using UnboundLib.Utils.UI;
using UnboundLib.GameModes;
using UnboundLib.Extensions;
using UnboundLib.Cards;
using UnboundLib.Utils;
using UnboundLib.Networking;
using HarmonyLib;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using TabInfo.Utils;
using Jotunn.Utils;
using Sonigon;

namespace TabInfo
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    //[BepInDependency("com.willuwontu.rounds.customstatextension", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class TabInfo : BaseUnityPlugin
    {
        private const string ModId = "com.willuwontu.rounds.tabinfo";
        private const string ModName = "Tab Info";
        public const string Version = "1.0.0"; // What version are we on (major.minor.patch)?

        public const string ModInitials = "TI";

        public static TabInfo instance { get; private set; }
        public MonoBehaviourPun photonCoordinator { get; private set; }

        public AssetBundle Assets { get; private set; }

        public List<AudioClip> click;
        public List<AudioClip> hover;
        public List<SoundEvent> clickSounds = new List<SoundEvent>();
        public List<SoundEvent> hoverSounds = new List<SoundEvent>();

        void Awake()
        {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start()
        {
            Unbound.RegisterCredits(ModName, new string[] { "willuwontu" }, new string[] { "github", "Ko-Fi" }, new string[] { "https://github.com/willuwontu/wills-wacky-cards", "https://ko-fi.com/willuwontu" });

            instance = this;

            Assets = AssetUtils.LoadAssetBundleFromResources("tabinfo", typeof(TabInfo).Assembly);

            { // Button Sounds

                click = Assets.LoadAllAssets<AudioClip>().ToList().Where(clip => clip.name.Contains("UI_Button_Click")).ToList();
                hover = Assets.LoadAllAssets<AudioClip>().ToList().Where(clip => clip.name.Contains("UI_Button_Hover")).ToList();

                try
                {
                    foreach (var sound in click)
                    {
                        SoundContainer soundContainer = ScriptableObject.CreateInstance<SoundContainer>();
                        soundContainer.setting.volumeIntensityEnable = true;
                        soundContainer.setting.volumeDecibel = 10f;
                        soundContainer.audioClip[0] = sound;
                        SoundEvent soundEvent = ScriptableObject.CreateInstance<SoundEvent>();
                        soundEvent.soundContainerArray[0] = soundContainer;
                        clickSounds.Add(soundEvent);
                    }

                    foreach (var sound in hover)
                    {
                        SoundContainer soundContainer = ScriptableObject.CreateInstance<SoundContainer>();
                        soundContainer.setting.volumeIntensityEnable = true;
                        soundContainer.setting.volumeDecibel = 10f;
                        soundContainer.audioClip[0] = sound;
                        SoundEvent soundEvent = ScriptableObject.CreateInstance<SoundEvent>();
                        soundEvent.soundContainerArray[0] = soundContainer;
                        hoverSounds.Add(soundEvent);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }

            { // Load Frame assets
                TabInfoManager.canvas = Instantiate(TabInfo.instance.Assets.LoadAsset<GameObject>("Info Canvas"));
                RectTransform rect = TabInfoManager.canvas.GetComponent<RectTransform>();
                rect.localScale = Vector3.one;
                TabInfoManager.canvas.GetComponent<Canvas>().worldCamera = Camera.current;
                DontDestroyOnLoad(TabInfoManager.canvas);

                TabInfoManager.tabFrameTemplate = TabInfo.instance.Assets.LoadAsset<GameObject>("Info Container");
                TabInfoManager.teamFrameTemplate = TabInfo.instance.Assets.LoadAsset<GameObject>("Team Frame");
                TabInfoManager.playerFrameTemplate = TabInfo.instance.Assets.LoadAsset<GameObject>("Player Frame");
                TabInfoManager.cardButtonTemplate = TabInfo.instance.Assets.LoadAsset<GameObject>("Card Button");
                TabInfoManager.statSectionTemplate = TabInfo.instance.Assets.LoadAsset<GameObject>("Stat Section");
                TabInfoManager.statObjectTemplate = TabInfo.instance.Assets.LoadAsset<GameObject>("Stat Object");

                var tabFrameObj = Instantiate(TabInfoManager.tabFrameTemplate, TabInfoManager.canvas.transform);
                TabInfoManager.tabFrame = tabFrameObj.AddComponent<TabFrame>();
                tabFrameObj.SetActive(false);
            }

            GameModeManager.AddHook(GameModeHooks.HookGameEnd, GameEnd);
            GameModeManager.AddHook(GameModeHooks.HookGameStart, GameStart);
            GameModeManager.AddHook(GameModeHooks.HookBattleStart, BattleStart);
            GameModeManager.AddHook(GameModeHooks.HookPlayerPickStart, PlayerPickStart);
            GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, PlayerPickEnd);
            GameModeManager.AddHook(GameModeHooks.HookPointStart, PointStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, PointEnd);
            GameModeManager.AddHook(GameModeHooks.HookPickStart, PickStart);
            GameModeManager.AddHook(GameModeHooks.HookPickEnd, PickEnd);
            GameModeManager.AddHook(GameModeHooks.HookRoundStart, RoundStart);
            GameModeManager.AddHook(GameModeHooks.HookRoundEnd, RoundEnd);

            var networkEvents = gameObject.AddComponent<NetworkEventCallbacks>();
            networkEvents.OnJoinedRoomEvent += OnJoinedRoomAction;
            networkEvents.OnLeftRoomEvent += OnLeftRoomAction;

            //this.ExecuteAfterSeconds(5, () => { CreateColorTester(); });

        }
        private void OnJoinedRoomAction()
        {

        }

        private void OnLeftRoomAction()
        {

        }

        public static class WaitFor
        {
            public static IEnumerator Frames(int frameCount)
            {
                if (frameCount <= 0)
                {
                    throw new ArgumentOutOfRangeException("frameCount", "Cannot wait for less that 1 frame");
                }

                while (frameCount > 0)
                {
                    frameCount--;
                    yield return null;
                }
            }
        }

        IEnumerator RoundStart(IGameModeHandler gm)
        {
            TabInfoManager.CurrentRound += 1;
            TabInfoManager.CurrentPoint = 0;
            yield break;
        }

        IEnumerator RoundEnd(IGameModeHandler gm)
        {
            yield break;
        }

        IEnumerator PointStart(IGameModeHandler gm)
        {
            TabInfoManager.CurrentPoint += 1;
            yield break;
        }

        IEnumerator PointEnd(IGameModeHandler gm)
        {
            yield break;
        }

        IEnumerator PlayerPickStart(IGameModeHandler gm)
        {
            yield break;
        }

        IEnumerator PlayerPickEnd(IGameModeHandler gm)
        {

            yield break;
        }

        IEnumerator PickStart(IGameModeHandler gm)
        {
            yield break;
        }

        IEnumerator PickEnd(IGameModeHandler gm)
        {
            yield break;
        }

        IEnumerator BattleStart(IGameModeHandler gm)
        {
            yield break;
        }
        IEnumerator GameStart(IGameModeHandler gm)
        {
            TabInfoManager.CurrentRound = 0;
            TabInfoManager.CurrentPoint = 0;
            yield break;
        }

        IEnumerator GameEnd(IGameModeHandler gm)
        {
            yield break;
        }

        void DestroyAll<T>() where T : UnityEngine.Object
        {
            var objects = GameObject.FindObjectsOfType<T>();
            for (int i = objects.Length - 1; i >= 0; i--)
            {
                UnityEngine.Debug.Log($"Attempting to Destroy {objects[i].GetType().Name} number {i}");
                UnityEngine.Object.Destroy(objects[i]);
            }
        }

        private GameObject CreateColorTester()
        {
            // The Base Game Object
            var cTest = new GameObject();
            cTest.name = "Color Tester Canvas";
            DontDestroyOnLoad(cTest);
            var canvas = cTest.AddComponent<Canvas>();
            var scaler = cTest.AddComponent<CanvasScaler>();
            var caster = cTest.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
            canvas.sortingOrder = 1;
            var camera = cTest.AddComponent<Camera>();
            canvas.worldCamera = camera;
            //canvas.renderMode = RenderMode.ScreenSpaceCamera;
            camera.enabled = false;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // The background
            var background = new GameObject();
            {
                background.transform.parent = cTest.transform;
                background.name = "Background";
                var backImage = background.AddComponent<Image>();
                backImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                var rect = background.GetOrAddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 1);
                rect.offsetMin = new Vector2(20f, 20f);
                rect.offsetMax = new Vector2(-20f, -20f);
            }

            // The scroll area, so we can see all the colors
            var scrollView = new GameObject();
            var viewport = new GameObject();
            var content = new GameObject();
            {
                scrollView.name = "Scroll View";
                scrollView.transform.parent = background.transform;
                var rect = scrollView.GetOrAddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 1);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                var scrollScroll = scrollView.GetOrAddComponent<ScrollRect>();
                scrollScroll.inertia = false;
                scrollScroll.movementType = ScrollRect.MovementType.Clamped;
                scrollScroll.horizontal = false;
                scrollScroll.scrollSensitivity = 50f;

                // Scrollbar
                {

                }

                viewport.transform.parent = scrollView.transform;
                viewport.name = "Viewport";
                var viewportRect = viewport.GetOrAddComponent<RectTransform>();
                scrollScroll.viewport = viewportRect;

                content.transform.parent = viewport.transform;
                content.name = "Content";
                var contentRect = content.GetOrAddComponent<RectTransform>();
                scrollScroll.content = contentRect;
            }

            // Viewport Settings
            {
                var rect = viewport.GetOrAddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 1);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                var image = viewport.AddComponent<Image>();
                image.color = new Color(1f, 1f, 1f, 1f / 255f);
                var mask = viewport.AddComponent<Mask>();
            }

            // Content Settings
            {
                var rect = content.GetOrAddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 1);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                var grid = content.AddComponent<GridLayoutGroup>();
                grid.padding = new RectOffset(20, 20, 10, 10);
                grid.cellSize = new Vector2(500f, 500f);
                grid.spacing = new Vector2(10f, 10f);
                grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
                grid.startAxis = GridLayoutGroup.Axis.Horizontal;
                grid.childAlignment = TextAnchor.MiddleCenter;
                grid.constraint = GridLayoutGroup.Constraint.Flexible;

                var fitter = content.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var image = content.AddComponent<Image>();
                image.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
            }

            void CreateTestGrid(GameObject parent, PlayerSkin skin, string teamName)
            {
                var container = new GameObject();
                container.transform.parent = parent.transform;
                container.name = teamName;
                Color[] colors = new Color[] { };
                try
                {
                    colors = new Color[] { skin.color, skin.backgroundColor, skin.winText, skin.particleEffect };
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log("PlayerSkin is null");
                    UnityEngine.Debug.LogException(e);
                }
                string[] colorTypes = new string[] { "Color", "Background", "Win", "Particle" };

                // Add a Vertical Layout Group
                {
                    var VLG = container.AddComponent<VerticalLayoutGroup>();
                    VLG.padding = new RectOffset(0, 0, 0, 0);
                    VLG.spacing = 0f;
                    VLG.childAlignment = TextAnchor.UpperCenter;
                    VLG.childControlHeight = false;
                    VLG.childControlWidth = true;
                    VLG.childForceExpandHeight = false;
                    VLG.childForceExpandWidth = true;
                }

                RectTransform CreateHeadingBox(GameObject parent, Color color, string colorType)
                {
                    // Box container is a Vertical Layout Group for its children
                    var box = new GameObject();
                    box.transform.parent = parent.transform;
                    box.name = colorType + " Color";
                    var rect = box.GetOrAddComponent<RectTransform>();
                    {
                        var VLG = box.AddComponent<VerticalLayoutGroup>();
                        VLG.padding = new RectOffset(0, 0, 0, 0);
                        VLG.spacing = 0f;
                        VLG.childAlignment = TextAnchor.MiddleCenter;
                        VLG.childControlHeight = false;
                        VLG.childControlWidth = true;
                        VLG.childForceExpandHeight = false;
                        VLG.childForceExpandWidth = true;
                    }


                    // The first item is the contrast of that Color with White
                    var whiteContrast = new GameObject();
                    whiteContrast.transform.parent = box.transform;
                    whiteContrast.name = "White";
                    {
                        var tempRect = whiteContrast.GetOrAddComponent<RectTransform>();
                        tempRect.sizeDelta = new Vector2(100f, 100f * 1f / 3f);
                        var tempText = whiteContrast.AddComponent<TextMeshProUGUI>();
                        tempText.enableAutoSizing = true;
                        tempText.autoSizeTextContainer = false;
                        tempText.alignment = TextAlignmentOptions.Center;
                        tempText.fontSizeMax = 36f;
                        tempText.fontSizeMin = 10;
                        tempText.enableWordWrapping = false;
                        tempText.overflowMode = TextOverflowModes.Truncate;
                        tempText.color = Color.white;

                        tempText.text = string.Format("{0:F1} : 1", ColorManager.ColorContrast(Color.white, color));
                    }

                    // The second item is the name of the color
                    var colorName = new GameObject();
                    colorName.transform.parent = box.transform;
                    colorName.name = "Name";
                    {
                        var tempRect = colorName.GetOrAddComponent<RectTransform>();
                        tempRect.sizeDelta = new Vector2(100f, 100f * 1f / 3f);
                        var tempText = colorName.AddComponent<TextMeshProUGUI>();
                        tempText.enableAutoSizing = true;
                        tempText.autoSizeTextContainer = false;
                        tempText.alignment = TextAlignmentOptions.Center;
                        tempText.fontSizeMax = 36f;
                        tempText.fontSizeMin = 10;
                        tempText.enableWordWrapping = false;
                        tempText.overflowMode = TextOverflowModes.Truncate;
                        tempText.color = color;

                        tempText.text = colorType;
                    }

                    // The lastt item is the contrast of that Color with black
                    var blackContrast = new GameObject();
                    blackContrast.transform.parent = box.transform;
                    blackContrast.name = "Black";
                    {
                        var tempRect = blackContrast.GetOrAddComponent<RectTransform>();
                        tempRect.sizeDelta = new Vector2(100f, 33f);
                        var tempText = blackContrast.AddComponent<TextMeshProUGUI>();
                        tempText.enableAutoSizing = true;
                        tempText.autoSizeTextContainer = false;
                        tempText.alignment = TextAlignmentOptions.Center;
                        tempText.fontSizeMax = 36f;
                        tempText.fontSizeMin = 10;
                        tempText.enableWordWrapping = false;
                        tempText.overflowMode = TextOverflowModes.Truncate;
                        tempText.color = Color.black;

                        tempText.text = string.Format("{0:F1} : 1", ColorManager.ColorContrast(Color.black, color));
                        tempRect.sizeDelta = new Vector2(100f, 33f);
                    }

                    whiteContrast.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 33f);
                    colorName.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 33f);

                    return rect;
                }

                RectTransform CreateColorBox(GameObject parent, Color textColor, string textColorType, Color color, string colorColorType)
                {
                    var colors = ColorManager.GetContrastingColors(textColor, color, 3.5f);

                    if (ColorManager.RelativeLuminance(textColor) < ColorManager.RelativeLuminance(color))
                    {
                        colors = new Color[] { colors[1], colors[0] };
                    }

                    // Box container is a Vertical Layout Group for its children
                    var box = new GameObject();
                    box.transform.parent = parent.transform;
                    box.name = textColorType + " on " + colorColorType;
                    var rect = box.GetOrAddComponent<RectTransform>();
                    {
                        var VLG = box.AddComponent<VerticalLayoutGroup>();
                        VLG.padding = new RectOffset(0, 0, 0, 0);
                        VLG.spacing = 0f;
                        VLG.childAlignment = TextAnchor.MiddleCenter;
                        VLG.childControlHeight = true;
                        VLG.childControlWidth = true;
                        VLG.childForceExpandHeight = true;
                        VLG.childForceExpandWidth = true;
                    }

                    var image = box.AddComponent<Image>();
                    image.color = colors[1];


                    // The first item is the contrast of that Color with White
                    var colorContrast = new GameObject();
                    colorContrast.transform.parent = box.transform;
                    colorContrast.name = "Contrast";
                    {
                        var text = colorContrast.AddComponent<TextMeshProUGUI>();
                        text.enableAutoSizing = true;
                        text.autoSizeTextContainer = false;
                        text.alignment = TextAlignmentOptions.Center;
                        text.fontSizeMax = 36f;
                        text.fontSizeMin = 10;
                        text.enableWordWrapping = false;
                        text.overflowMode = TextOverflowModes.Truncate;
                        text.color = colors[0];

                        text.text = string.Format("{0:F1} : 1", ColorManager.ColorContrast(colors[0], colors[1]));
                    }

                    return rect;
                }

                // Create the Header row object
                var headerRow = new GameObject();
                headerRow.transform.parent = container.transform;
                headerRow.name = "Header Row";
                {
                    var rect = headerRow.GetOrAddComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(500f, 100f);
                }

                // Populate the Header Row
                {
                    var titleBox = new GameObject();
                    titleBox.transform.parent = headerRow.transform;
                    titleBox.name = "Team";
                    {
                        var rect = titleBox.GetOrAddComponent<RectTransform>();
                        rect.anchorMin = new Vector2(0, 0);
                        rect.anchorMax = new Vector2(0.2f, 1);
                        rect.offsetMin = Vector2.zero;
                        rect.offsetMax = Vector2.zero;

                        var VLG = titleBox.AddComponent<VerticalLayoutGroup>();
                        VLG.padding = new RectOffset(0, 0, 0, 0);
                        VLG.spacing = 0f;
                        VLG.childAlignment = TextAnchor.MiddleCenter;
                        VLG.childControlHeight = false;
                        VLG.childControlWidth = true;
                        VLG.childForceExpandHeight = false;
                        VLG.childForceExpandWidth = true;
                    }

                    var titleName = new GameObject();
                    titleName.transform.parent = titleBox.transform;
                    titleName.name = "Name";
                    {
                        var rect = titleName.GetOrAddComponent<RectTransform>();
                        rect.anchorMin = new Vector2(0, 0);
                        rect.anchorMax = new Vector2(1f, 1);
                        rect.offsetMin = Vector2.zero;
                        rect.offsetMax = Vector2.zero;

                        var text = titleName.AddComponent<TextMeshProUGUI>();
                        text.text = teamName;
                        text.enableAutoSizing = true;
                        text.autoSizeTextContainer = false;
                        text.fontSizeMax = 36f;
                        text.fontSizeMin = 10f;


                    }

                    for (int i = 0; i < colors.Length; i++)
                    {
                        try
                        {
                            var rect = CreateHeadingBox(headerRow, colors[i], colorTypes[i]);
                            rect.anchorMin = new Vector2(0.2f * (i + 1), 0);
                            rect.anchorMax = new Vector2(0.2f * (i + 2), 1);
                            rect.offsetMin = Vector2.zero;
                            rect.offsetMax = Vector2.zero;
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }

                //Create the other rows
                {
                    for (int i = 0; i < colors.Length; i++)
                    {
                        var row = new GameObject();
                        row.transform.parent = container.transform;
                        row.name = colorTypes[i] + " Row";
                        {
                            var rect = row.GetOrAddComponent<RectTransform>();
                            rect.sizeDelta = new Vector2(500f, 100f);
                        }

                        try
                        {
                            var rowHeader = CreateHeadingBox(row, colors[i], colorTypes[i]);
                            rowHeader.anchorMin = new Vector2(0, 0);
                            rowHeader.anchorMax = new Vector2(0.2f, 1);
                            rowHeader.offsetMin = Vector2.zero;
                            rowHeader.offsetMax = Vector2.zero;
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }

                        for (int i2 = 0; i2 < colors.Length; i2++)
                        {
                            try
                            {
                                var rect = CreateColorBox(row, colors[i2], colorTypes[i2], colors[i], colorTypes[i]);
                                rect.anchorMin = new Vector2(0.2f * (i2 + 1), 0);
                                rect.anchorMax = new Vector2(0.2f * (i2 + 2), 1);
                                rect.offsetMin = Vector2.zero;
                                rect.offsetMax = Vector2.zero;
                            }
                            catch (Exception e)
                            {
                                UnityEngine.Debug.LogException(e);
                            }
                        }
                    }
                }
            }

            PlayerSkin[] playerSkins = (PlayerSkin[]) (typeof(ExtraPlayerSkins).GetField("extraSkinBases", BindingFlags.Default | BindingFlags.Static | BindingFlags.NonPublic).GetValue(null));

            for (int i = 0; i < playerSkins.Length; i++)
            {
                try
                {
                    CreateTestGrid(content, playerSkins[i], ExtraPlayerSkins.GetTeamColorName(i));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }

            return cTest;
        }
    }
}
