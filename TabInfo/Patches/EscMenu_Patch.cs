using HarmonyLib;
using InControl;
using UnityEngine;
using TabInfo.Utils;

namespace ItemShops.Patches
{
    [HarmonyPatch(typeof(EscapeMenuHandler))]
    class EscMenu_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(EscapeMenuHandler.ToggleEsc))]
        static bool CheckShopStatus()
        {
            // Check to see if a shop is open
            var shopOpen = TabInfoManager.IsLockingInput;
            if (shopOpen)
            {
                TabFrame _ = TabInfoManager.TabFrame;
                TabInfoManager.TabFrame.gameObject.SetActive(false);
                TabInfoManager.TabFrame.toggled = false;
            }
            return !shopOpen;
        }
    }
}
