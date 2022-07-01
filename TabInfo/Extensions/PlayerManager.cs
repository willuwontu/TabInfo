using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TabInfo.Extensions
{
    public static class PlayerManagerExtension
    {
        public static Player GetPlayerWithID(this PlayerManager playerManager, int playerID)
        {
            return (Player)typeof(PlayerManager).InvokeMember("GetPlayerWithID",
                BindingFlags.Instance | BindingFlags.InvokeMethod |
                BindingFlags.NonPublic, null, playerManager, new object[] { playerID });
        }
    }
}