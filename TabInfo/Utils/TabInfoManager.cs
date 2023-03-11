using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;
using TabInfo.Extensions;
using System.Numerics;

namespace TabInfo.Utils
{
    public static class TabInfoManager
    {
        private static Dictionary<string, StatCategory> _categories = new Dictionary<string, StatCategory>();

        public static ReadOnlyDictionary<string, StatCategory> Categories { get => new ReadOnlyDictionary<string, StatCategory>(_categories); }

        public static StatCategory RegisterCategory(string name, int priority, bool toggle)
        {
            if (_categories.Keys.Contains(name.ToLower()))
            {
                throw new ArgumentException("Category name must be unique.");
            }

            if (priority < 0)
            {
                throw new ArgumentException("Category priority cannot be less than 0.");
            }

            StatCategory result = new StatCategory(name, priority, toggle);
            _categories.Add(name.ToLower(), result);
            return result;
        }
        public static StatCategory RegisterCategory(string name, int priority)
        {
            if (_categories.Keys.Contains(name.ToLower()))
            {
                throw new ArgumentException("Category name must be unique.");
            }

            if (priority < 0)
            {
                throw new ArgumentException("Category priority cannot be less than 0.");
            }

            StatCategory result = new StatCategory(name, priority, true);
            _categories.Add(name.ToLower(), result);
            return result;
        }

        public static Stat RegisterStat(StatCategory category, string name, Func<Player, bool> displayCondition, Func<Player, string> displayValue)
        {
            if (category.Stats.ContainsKey(name.ToLower()))
            {
                throw new ArgumentException("Stat Names must be unique for the category that they're in.");
            }

            Stat result = category.RegisterStat(name, displayCondition, displayValue);
            return result;
        }

        public static readonly StatCategory basicStats;

        static TabInfoManager()
        {
            basicStats = new StatCategory("Basic Stats", -1, true);

            _categories.Add(basicStats.name.ToLower(), basicStats);
            basicStats.RegisterStat("HP", (value) => true, (player) => string.Format("{0:F0}/{1:F0}", player.data.health, player.data.maxHealth));
            basicStats.RegisterStat("Damage", (value) => true, (player) => string.Format("{0:F0}", player.data.weaponHandler.gun.damage * player.data.weaponHandler.gun.bulletDamageMultiplier * 55f));
            basicStats.RegisterStat("Block Cooldown", (value) => true, (player) => string.Format("{0:F2}s", player.data.block.Cooldown()));
            basicStats.RegisterStat("Reload Time", (value) => true, (player) => string.Format("{0:F2}s", (float)player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>().InvokeMethod("ReloadTime")));
            basicStats.RegisterStat("Ammo", (value) => true, (player) => string.Format("{0:F0}", player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>().maxAmmo));
            basicStats.RegisterStat("Movespeed", (value) => true, (player) => string.Format("{0:F2}", player.data.stats.movementSpeed));
        }

        internal static GameObject canvas;
        internal static GameObject tabFrameTemplate;
        internal static GameObject teamFrameTemplate;
        internal static GameObject playerFrameTemplate;
        internal static GameObject cardButtonTemplate;
        internal static GameObject statSectionTemplate;
        internal static GameObject statObjectTemplate;
        internal static GameObject cardHolderTemplate;

        internal static TabFrame tabFrame = null;

        internal static TabFrame TabFrame
        {
            get
            {
                if (tabFrame == null)
                {
                    var tabFrameObj = GameObject.Instantiate(TabInfoManager.tabFrameTemplate, TabInfoManager.canvas.transform);
                    TabInfoManager.tabFrame = tabFrameObj.AddComponent<TabFrame>();
                    tabFrameObj.SetActive(false);
                }

                return tabFrame;
            }
        }
        public static int RoundsToWin
        {
            get
            {
                try
                {
                    return (int)UnboundLib.GameModes.GameModeManager.CurrentHandler.Settings["roundsToWinGame"];
                }
                catch
                {
                    return 0;
                }
            }
        }
        public static int PointsToWin
        {
            get
            {
                try
                {
                    return (int)UnboundLib.GameModes.GameModeManager.CurrentHandler.Settings["pointsToWinRound"];
                }
                catch
                {
                    return 0;
                }
            }
        }
        public static int CurrentRound { get; internal set; }
        public static int CurrentPoint { get; internal set; }

        public static bool IsLockingInput { get { if (TabFrame != null) { return TabFrame.gameObject.activeSelf; } return false; } }
        private static List<string> hiddenGameModes = new List<string>();
        /// <summary>
        /// Registers a gamemode that the UI will hidden during and cannot be opened.
        /// </summary>
        /// <param name="gameModeID">The ID of the gamemode to register. This is the same ID used to register it with unbound.</param>
        public static void RegisterHiddenGameMode(string gameModeID)
        {
            TabInfoManager.hiddenGameModes.Add(gameModeID);
        }

        public static void ToggleTabFrame()
        {
            if (!hiddenGameModes.Contains(UnboundLib.GameModes.GameModeManager.CurrentHandlerID))
            {
                TabFrame _ = TabFrame;
                TabInfoManager.TabFrame.toggled = !TabInfoManager.TabFrame.toggled;
                TabInfoManager.TabFrame.gameObject.SetActive(TabInfoManager.TabFrame.toggled);

                if (!TabInfoManager.TabFrame.toggled)
                {
                    UnityEngine.GameObject.Destroy(TabInfoManager.TabFrame.gameObject);
                }
            }
        }
    }

    internal class TabListener : MonoBehaviour
    {
        private void Update()
        {
            if (PlayerManager.instance)
            {
                if (PlayerManager.instance.players != null)
                {
                    if (PlayerManager.instance.players.Count > 0)
                    {
                        if (PlayerManager.instance.LocalPlayers().Length > 0)
                        {
                            if (PlayerManager.instance.LocalPlayers().Any(player => player.data.playerActions.GetAdditionalData().toggleTab.WasPressed))
                            {
                                TabInfoManager.ToggleTabFrame();
                            }
                        }
                    }
                }
            }
        }
    }

    public class StatCategory
    {
        public readonly string name;
        public readonly int priority;
        public bool toggle;


        private Dictionary<string, Stat> _stats = new Dictionary<string, Stat>();

        public ReadOnlyDictionary<string, Stat> Stats
        {
            get => new ReadOnlyDictionary<string, Stat>(_stats);
        }

        internal Stat RegisterStat(string name, Func<Player, bool> condition, Func<Player, string> value)
        {
            if (this._stats.ContainsKey(name.ToLower()))
            {
                throw new ArgumentException("Stat Names must be unique.");
            }

            Stat result = new Stat(name, this, condition, value);
            this._stats.Add(name.ToLower(), result);

            return result;
        }

        internal StatCategory(string name, int priority, bool toggle)
        {
            this.name = name;
            this.priority = priority;
            this.toggle = toggle;
        }
    }

    public class Stat
    {
        public readonly string name;
        public bool toggle;

        internal StatCategory category;
        internal Func<Player, string> displayValue;
        internal Func<Player, bool> displayCondition;

        internal Stat(string name, StatCategory category, Func<Player, bool> condition, Func<Player, string> value, bool toggle = true)
        {
            this.name = name;
            this.category = category;
            this.displayCondition = condition;
            this.displayValue = value;
            this.toggle = toggle;
        }
    }
}
