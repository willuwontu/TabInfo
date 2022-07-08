using System;
using HarmonyLib;
using UnityEngine;
using TabInfo.Utils;
using TabInfo.Extensions;

namespace TabInfo.Patches
{
    [HarmonyPatch(typeof(Player))]
    class Player_Patch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void GetPlayerInput(Player __instance)
        {
            var actionCatcher = TabInfoManager.canvas.AddComponent<ActionCatcher>();
            actionCatcher.player = __instance;
        }
    }
}