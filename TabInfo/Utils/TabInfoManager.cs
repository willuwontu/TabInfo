using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace TabInfo.Utils
{
    public static class TabInfoManager
    {
        static Dictionary<string, StatCategory> _categories = new Dictionary<string, StatCategory>();

        public static ReadOnlyDictionary<string, StatCategory> Categories { get => new ReadOnlyDictionary<string, StatCategory>(_categories); }

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

            StatCategory result = new StatCategory(name, priority);
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
            basicStats = new StatCategory("Basic Stats", -1);

            _categories.Add(basicStats.name.ToLower(), basicStats);
            basicStats.RegisterStat("HP", (value) => true, (player) => string.Format("{0:F0}/{1:F0}", player.data.health, player.data.maxHealth));
            basicStats.RegisterStat("Damage", (value) => true, (player) => string.Format("{0:F0}", player.data.weaponHandler.gun.damage * player.data.weaponHandler.gun.bulletDamageMultiplier * 55f));
            basicStats.RegisterStat("Block Cooldown", (value) => true, (player) => string.Format("{0:F2}s", player.data.block.Cooldown()));
            basicStats.RegisterStat("Reload Time", (value) => true, (player) => string.Format("{0:F2}s", (float) player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>().InvokeMethod("ReloadTime")));
            basicStats.RegisterStat("Ammo", (value) => true, (player) => string.Format("{0:F0}", player.data.weaponHandler.gun.GetComponentInChildren<GunAmmo>().maxAmmo));
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

        public static int RoundsToWin { get => (int)UnboundLib.GameModes.GameModeManager.CurrentHandler.Settings["roundsToWinGame"]; }
        public static int PointsToWin { get => (int)UnboundLib.GameModes.GameModeManager.CurrentHandler.Settings["pointsToWinRound"]; }
        public static int CurrentRound { get; internal set; }
        public static int CurrentPoint { get; internal set; }

        public static bool IsLockingInput { get { if (tabFrame != null) { return tabFrame.gameObject.activeSelf; } return false; } }
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
                TabInfoManager.tabFrame.toggled = !TabInfoManager.tabFrame.toggled;
                TabInfoManager.tabFrame.gameObject.SetActive(TabInfoManager.tabFrame.toggled);
            }
        }
    }

    public class StatCategory
    {
        public readonly string name;
        public readonly int priority;

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

        internal StatCategory(string name, int priority)
        {
            this.name = name;
            this.priority = priority;
        }
    }
    
    public class Stat
    {
        public readonly string name;

        internal StatCategory category;
        internal Func<Player, string> displayValue;
        internal Func<Player, bool> displayCondition;

        internal Stat(string name, StatCategory category, Func<Player, bool> condition, Func<Player, string> value)
        {
            this.name = name;
            this.category = category;
            this.displayCondition = condition;
            this.displayValue = value;
        }
    }
}
