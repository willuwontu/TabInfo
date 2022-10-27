using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Photon.Pun;
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

        public static Player[] LocalPlayers(this PlayerManager playerManager)
        {
            if (playerManager.players == null || playerManager.players.Count() == 0)
            {
                return new Player[0];
            }

            return playerManager.players.Where(player => PhotonNetwork.OfflineMode || player.data.view.IsMine).ToArray();
        }
    }
}