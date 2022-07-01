using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnboundLib.Extensions;

namespace TabInfo.Utils
{
    public class TeamFrame : MonoBehaviour
    {
        public int team;
        private Image _bg = null;
        public Image HeaderBG
        {
            get
            {
                if (this._bg is null)
                {
                    this._bg = this.transform.Find("Team Header").GetComponent<Image>();
                }
                return this._bg;
            }
        }
        private GameObject _playerHolder = null;
        public GameObject PlayerHolder
        {
            get
            {
                if (this._playerHolder is null)
                {
                    this._playerHolder = this.transform.Find("Players").gameObject;
                }
                return this._playerHolder;
            }
        }
        private TextMeshProUGUI _teamName = null;
        public TextMeshProUGUI TeamName
        {
            get
            {
                if (this._teamName is null)
                {
                    this._teamName = this.transform.Find("Team Header/Team Name").gameObject.GetComponent<TextMeshProUGUI>();
                }
                return this._teamName;
            }
        }
        private TextMeshProUGUI _teamScore = null;
        public TextMeshProUGUI TeamScore
        {
            get
            {
                if (this._teamScore is null)
                {
                    this._teamScore = this.transform.Find("Team Header/Score").gameObject.GetComponent<TextMeshProUGUI>();
                }
                return this._teamScore;
            }
        }
        public TextMeshProUGUI Spacer
        {
            get
            {
                return this.transform.Find("Team Header/Spacer").gameObject.GetComponent<TextMeshProUGUI>();
            }
        }
        private List<PlayerFrame> playerFrames = new List<PlayerFrame>();
        private Color[] colors;
        private void Start()
        {
            foreach (var player in PlayerManager.instance.GetPlayersInTeam(this.team).OrderBy(player => player.playerID))
            {
                var playerFrameObj = Instantiate(TabInfoManager.playerFrameTemplate, this.PlayerHolder.transform);
                var playerFrame = playerFrameObj.AddComponent<PlayerFrame>();
                playerFrame.player = player;
                playerFrame.teamFrame = this;
                this.playerFrames.Add(playerFrame);
            }
            if (playerFrames.Count() > 0)
            {
                var colors = playerFrames[0].player.GetTeamColors();
                this.TeamName.text = UnboundLib.Utils.ExtraPlayerSkins.GetTeamColorName(playerFrames[0].player.colorID());
                this.colors = ColorManager.GetContrastingColors(colors.winText, colors.backgroundColor, 3);
                this.HeaderBG.color = this.colors[1];
                this.TeamName.color = this.colors[0];
                this.TeamScore.color = this.colors[0];
                this.Spacer.color = this.colors[0];
            }
        }
        private void Update()
        {
            if (this.playerFrames.Count() != PlayerManager.instance.GetPlayersInTeam(this.team).Count())
            {
                var extraPlayers = this.playerFrames.Select(playerFrame => playerFrame.player).Except(PlayerManager.instance.GetPlayersInTeam(this.team));
                var extraPlayerFrames = this.playerFrames.Where(playerFrame => extraPlayers.Contains(playerFrame.player));
                foreach (var playerFrame in extraPlayerFrames) { UnityEngine.GameObject.Destroy(playerFrame.gameObject); }
                var missingPlayers = PlayerManager.instance.GetPlayersInTeam(this.team).Except(this.playerFrames.Select(playerFrame => playerFrame.player));

                foreach (var player in missingPlayers)
                {
                    var playerFrameObj = Instantiate(TabInfoManager.playerFrameTemplate, this.PlayerHolder.transform);
                    var playerFrame = playerFrameObj.AddComponent<PlayerFrame>();
                    playerFrame.player = player;
                    playerFrame.teamFrame = this;
                    this.playerFrames.Add(playerFrame);
                }

                var playerOrder = PlayerManager.instance.GetPlayersInTeam(this.team).OrderBy(player => player.playerID).ToArray();
                for (int i = 0; i < playerOrder.Length; i++)
                {
                    this.playerFrames.Where(playerFrame => playerFrame.player == playerOrder[i]).First().gameObject.transform.SetSiblingIndex(i);
                }
            }

            var score = UnboundLib.GameModes.GameModeManager.CurrentHandler.GetTeamScore(this.team);
            var roundsToWin = (int)UnboundLib.GameModes.GameModeManager.CurrentHandler.Settings["roundsToWinGame"];
            var pointsToWin = (int)UnboundLib.GameModes.GameModeManager.CurrentHandler.Settings["pointsToWinRound"];
            this.TeamScore.text = $"{score.rounds}/{roundsToWin} Rounds {score.points}/{pointsToWin} Points";
        }
    }
}
