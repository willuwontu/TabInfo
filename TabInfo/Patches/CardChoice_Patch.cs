using HarmonyLib;
using TabInfo.Utils;

namespace TabInfo.Patches
{
    [HarmonyPatch(typeof(CardChoice))]
    class CardChoice_Patch
    {
        [HarmonyPatch("DoPlayerSelect")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        static bool Prefix()
        {
            return !TabInfoManager.IsLockingInput;
        }
    }
}