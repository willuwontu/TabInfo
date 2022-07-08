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
using UnboundLib.Networking;
using HarmonyLib;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using TabInfo.Utils;
using Jotunn.Utils;
using Sonigon;
using TabInfo.Extensions;

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
        public const string Version = "0.0.0"; // What version are we on (major.minor.patch)?

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
            Unbound.RegisterClientSideMod(ModId);
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
                TabInfoManager.cardHolderTemplate = TabInfo.instance.Assets.LoadAsset<GameObject>("Card Holder");

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
        }
        private void OnJoinedRoomAction()
        {

        }

        private void OnLeftRoomAction()
        {
            TabInfoManager.tabFrame.toggled = false;
            TabInfoManager.tabFrame.gameObject.SetActive(TabInfoManager.tabFrame.toggled);
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
            TabInfoManager.tabFrame.toggled = false;
            TabInfoManager.tabFrame.gameObject.SetActive(TabInfoManager.tabFrame.toggled);
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
    }
}
