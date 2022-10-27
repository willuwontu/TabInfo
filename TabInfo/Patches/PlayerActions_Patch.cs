using HarmonyLib;
using System;
using System.Reflection;
using InControl;
using TabInfo.Extensions;

namespace TabInfo.Patches
{
    [HarmonyPatch(typeof(PlayerActions))]
    class PlayerActions_Patch
    {
        [HarmonyPatch(typeof(PlayerActions))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] { })]
        [HarmonyPostfix]
        private static void CreateAction(PlayerActions __instance)
        {
            __instance.GetAdditionalData().toggleTab = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, __instance, new object[] { "(TabInfo) Toggle Tab" });
        }
        [HarmonyPostfix]
        [HarmonyPatch("CreateWithControllerBindings")]
        private static void SetControllerBinding(ref PlayerActions __result)
        {
            
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerActions), "CreateWithKeyboardBindings")]
        private static void SetKeyboardBinding(ref PlayerActions __result)
        {
            __result.GetAdditionalData().toggleTab.AddDefaultBinding(Key.Tab);
        }

    }
}
