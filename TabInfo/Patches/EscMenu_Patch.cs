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
                TabInfoManager.tabFrame.gameObject.SetActive(false);
                TabInfoManager.tabFrame.toggled = false;
            }
            return !shopOpen;
        }
    }
}
