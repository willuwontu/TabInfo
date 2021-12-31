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
using UnboundLib.Cards;
using UnboundLib.Utils;
using UnboundLib.Networking;
using HarmonyLib;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

namespace TabInfo
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.willuwontu.rounds.customstatextension", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class TabInfo : BaseUnityPlugin
    {
        private const string ModId = "com.willuwontu.rounds.tabinfo";
        private const string ModName = "Tab Info";
        public const string Version = "1.0.0"; // What version are we on (major.minor.patch)?

        public const string ModInitials = "WWC";
        public const string CurseInitials = "Curse";
        public const string TestingInitials = "Testing";

        public static TabInfo instance { get; private set; }

        void Awake()
        {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start()
        {
            Unbound.RegisterCredits(ModName, new string[] { "willuwontu" }, new string[] { "github", "Ko-Fi" }, new string[] { "https://github.com/willuwontu/wills-wacky-cards", "https://ko-fi.com/willuwontu" });

            instance = this;
            instance.gameObject.name = "TabInfo";

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
            if (!PhotonNetwork.OfflineMode)
            {
                ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
                if (customProperties.ContainsKey("Ping"))
                {
                    customProperties["Ping"] = PhotonNetwork.GetPing();
                }
                else
                {
                    customProperties.Add("Ping", PhotonNetwork.GetPing());
                }
                PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties, null, null);
            }
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

        float RelativeLuminance(Color color)
        {
            float ColorPartValue(float part)
            {
                return part <= 0.03928f ? part / 12.92f : Mathf.Pow((part + 0.055f) / 1.055f, 2.4f);
            }
            var r = ColorPartValue(color.r);
            var g = ColorPartValue(color.g);
            var b = ColorPartValue(color.b);

            var l = 0.2126f * r + 0.7152f * g + 0.0722f * b;
            return l;
        }

        private float ColorContrast(Color a, Color b)
        {
            float result = 0f;
            var La = RelativeLuminance(a) + 0.05f;
            var Lb = RelativeLuminance(b) + 0.05f;

            result = Mathf.Max(La, Lb) / Mathf.Min(La, Lb);

            return result;
        }

        private float ColorContrast(float luminanceA, Color b)
        {
            float result = 0f;
            var La = luminanceA + 0.05f;
            var Lb = RelativeLuminance(b) + 0.05f;

            result = Mathf.Max(La, Lb) / Mathf.Min(La, Lb);

            return result;
        }

        private float ColorContrast(Color a, float luminanceB)
        {
            float result = 0f;
            var La = RelativeLuminance(a) + 0.05f;
            var Lb = luminanceB + 0.05f;

            result = Mathf.Max(La, Lb) / Mathf.Min(La, Lb);

            return result;
        }

        /// <summary>
        /// Checks to see if a pair of colors contrast enough to be readable and returns a modified set if not.
        /// </summary>
        /// <param name="backgroundColor">The background color for the text to go on.</param>
        /// <param name="textColor">The intended text color.</param>
        /// <returns>A pair of contrasting colors, the lightest as the first color.</returns>
        public Color[] GetContrastingColors(Color backgroundColor, Color textColor, float ratio)
        {
            Color[] colors = new Color[2];

            var backL = RelativeLuminance(backgroundColor);
            var textL = RelativeLuminance(textColor);

            if (textL > backL)
            {
                colors[0] = textColor;
                colors[1] = backgroundColor;
            }
            else
            {
                colors[1] = textColor;
                colors[0] = backgroundColor;
            }

            // See if we have good enough contrast already
            if (!(ColorContrast(backgroundColor, textColor) < ratio))
            {
                return colors;
            }

            Color.RGBToHSV(colors[0], out var lightH, out var lightS, out var lightV);
            Color.RGBToHSV(colors[1], out var darkH, out var darkS, out var darkV);

            // If the darkest color can be darkened enough to have enough contrast after brightening the color.
            if (ColorContrast(Color.HSVToRGB(darkH, darkS, 0f), Color.HSVToRGB(lightH, lightS, 1f)) >= ratio)
            {
                var lightDiff = 1f - lightV;
                var darkDiff = darkV;

                var lightRatio = 0.01f * (lightDiff / (lightDiff + darkDiff));
                var darkRatio = 0.01f * (darkDiff / (lightDiff + darkDiff));

                while (ColorContrast(Color.HSVToRGB(lightH, lightS, lightV), Color.HSVToRGB(darkH, darkS, darkV)) < ratio)
                {
                    lightV += lightRatio;
                    darkV -= darkRatio;
                }

                colors[0] = Color.HSVToRGB(lightH, lightS, lightV);
                colors[1] = Color.HSVToRGB(darkH, darkS, darkV);
            }
            // Fall back to using white.
            else
            {
                colors[0] = Color.white;

                var lightL = RelativeLuminance(colors[0]);

                while (ColorContrast(lightL, Color.HSVToRGB(darkH, darkS, darkV)) < ratio)
                {
                    darkV -= 0.01f;
                }

                colors[1] = Color.HSVToRGB(darkH, darkS, darkV);
            }

            return colors;
        }

        IEnumerator RoundStart(IGameModeHandler gm)
        {
            yield break;
        }

        IEnumerator RoundEnd(IGameModeHandler gm)
        {
            yield break;
        }

        IEnumerator PointStart(IGameModeHandler gm)
        {
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
