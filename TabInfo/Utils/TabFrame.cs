using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace TabInfo.Utils
{
    public class TabFrame : MonoBehaviour
    {
        private TextMeshProUGUI _title = null;
        public TextMeshProUGUI Title
        {
            get
            {
                if (this._title is null)
                {
                    this._title = this.transform.Find("Title Bar/Title").gameObject.GetComponent<TextMeshProUGUI>();
                }
                return this._title;
            }
        }
        private Button _closeButton = null;
        public Button CloseButton
        {
            get
            {
                if (this._closeButton is null)
                {
                    this._closeButton = this.transform.Find("Title Bar/Close").GetComponent<Button>();
                }
                return this._closeButton;
            }
        }
        private GameObject _teamHolder = null;
        public GameObject TeamHolder
        {
            get
            {
                if (this._teamHolder is null)
                {
                    this._teamHolder = this.transform.Find("Scroll View/Viewport/Content").gameObject;
                }
                return this._teamHolder;
            }
        }
        public CanvasGroup CanvasGroup => this.GetComponent<CanvasGroup>();
        private List<TeamFrame> teamFrames = new List<TeamFrame>();
        internal bool toggled;
        private void Start()
        {
            foreach (var team in PlayerManager.instance.players.Select(player => player.teamID).Distinct().OrderBy(team=> team))
            {
                var teamFrameObj = Instantiate(TabInfoManager.teamFrameTemplate, this.TeamHolder.transform);
                var teamFrame = teamFrameObj.AddComponent<TeamFrame>();
                teamFrame.team = team;
                this.teamFrames.Add(teamFrame);
            }

            this.CloseButton.onClick.AddListener(() => { this.toggled = false; this.gameObject.SetActive(false); });
        }
        private void Update()
        {
            if (teamFrames.Count() != PlayerManager.instance.players.Select(player => player.teamID).Distinct().Count())
            {
                var extraTeams = this.teamFrames.Select(teamFrame => teamFrame.team).Except(PlayerManager.instance.players.Select(player => player.teamID).Distinct());
                var extraTeamFrames = this.teamFrames.Where(teamFrame => extraTeams.Contains(teamFrame.team));
                foreach (var teamFrame in extraTeamFrames) { UnityEngine.GameObject.Destroy(teamFrame); this.teamFrames.Remove(teamFrame); }
                var missingTeams = PlayerManager.instance.players.Select(player => player.teamID).Distinct().Except(this.teamFrames.Select(teamFrame => teamFrame.team));

                foreach (var team in missingTeams)
                {
                    var teamFrameObj = Instantiate(TabInfoManager.teamFrameTemplate, this.TeamHolder.transform);
                    var teamFrame = teamFrameObj.AddComponent<TeamFrame>();
                    teamFrame.team = team;
                    this.teamFrames.Add(teamFrame);
                }

                var teamOrder = PlayerManager.instance.players.Select(player => player.teamID).Distinct().OrderBy(team => team).ToArray();
                for (int i = 0; i < teamOrder.Length; i++)
                {
                    this.teamFrames.Where(teamframe => teamframe.team == teamOrder[i]).First().gameObject.transform.SetSiblingIndex(i);
                }
            }

            this.Title.text = string.Format("{0} - Round {1} - Point {2} - {3} Players", UnboundLib.GameModes.GameModeManager.CurrentHandler.Name, TabInfoManager.CurrentRound, TabInfoManager.CurrentPoint, PlayerManager.instance.players.Count());

            LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
        }
    }
}
