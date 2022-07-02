using System;
using HarmonyLib;
using UnityEngine;
using TabInfo.Utils;
using TabInfo.Extensions;

namespace TabInfo.Patches
{
    [HarmonyPatch(typeof(GeneralInput))]
    class GeneralInput_Patch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static void BlockPlayerInput(GeneralInput __instance, CharacterData ___data)
        {
            try
            {
                if (___data.playerActions.GetAdditionalData().toggleTab)
                {
                    if (___data.playerActions.GetAdditionalData().toggleTab.WasPressed)
                    {
                        TabInfoManager.tabFrame.toggled = !TabInfoManager.tabFrame.toggled;
                        TabInfoManager.tabFrame.gameObject.SetActive(TabInfoManager.tabFrame.toggled);
                    } 
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
    }
}