using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnboundLib;

namespace TabInfo.Utils
{
    public class PlayerFrame : MonoBehaviour
    {
        public TeamFrame teamFrame;
        public PlayerCardBar cardBar;
        public Player player;
        private GameObject _cardBar;
        public GameObject CardBar
        {
            get
            {
                if (!(this._cardBar != null))
                {
                    this._cardBar = this.transform.Find("Player Cards/Scroll View/Viewport/Content").gameObject;
                }
                return this._cardBar;
            }
        }
        private GameObject _cardBarFrame;
        public GameObject CardBarFrame
        {
            get
            {
                if (!(this._cardBarFrame != null))
                {
                    this._cardBarFrame = this.transform.Find("Player Cards").gameObject;
                }
                return this._cardBarFrame;
            }
        }
        private TextMeshProUGUI _nameText;
        public TextMeshProUGUI NameText
        {
            get
            {
                if (!(this._nameText != null))
                {
                    this._nameText = this.transform.Find("Player Header/Player Name").gameObject.GetComponent<TextMeshProUGUI>();
                }
                return this._nameText;
            }
        }
        private bool toggled = true;
        private Button _button;
        public Button Button
        {
            get
            {
                if (!(this._button != null))
                {
                    this._button = this.transform.Find("Player Header").gameObject.GetComponent<Button>();
                }
                return _button;
            }
        }
        private GameObject _statHolder;
        public GameObject StatHolder
        {
            get
            {
                if (!(this._statHolder != null))
                {
                    this._statHolder = this.transform.Find("Player Stats").gameObject;
                }
                return this._statHolder;
            }
        }

        private void OnHeaderClicked()
        {
            this.toggled = !this.toggled;

            this.CardBarFrame.SetActive(toggled);
            this.StatHolder.SetActive(toggled);
        }

        List<StatSection> statSections = new List<StatSection>();
        private void Start()
        {
            this.cardBar = this.CardBar.AddComponent<PlayerCardBar>();
            this.cardBar.playerFrame = this;
            this.cardBar.player = this.player;
            this.Button.onClick.AddListener(OnHeaderClicked);

            foreach (var category in TabInfoManager.Categories.Values.OrderBy(c => c.priority).ThenBy(c => c.name.ToLower()))
            {
                var sectionObj = Instantiate(TabInfoManager.statSectionTemplate, this.StatHolder.transform);
                var section = sectionObj.AddComponent<StatSection>();
                section.playerFrame = this;
                section.category = category;
                this.statSections.Add(section);
            }
        }

        private void Update()
        {
            if (this.statSections.Count() != TabInfoManager.Categories.Values.Count)
            {
                var extraCategories = this.statSections.Select(section => section.category).Except(TabInfoManager.Categories.Values).ToArray();
                var extraSections = this.statSections.Where(section => extraCategories.Contains(section.category)).ToArray();
                foreach (var section in extraSections) { UnityEngine.GameObject.Destroy(section.gameObject); }
                var missingCategories = TabInfoManager.Categories.Values.Except(this.statSections.Select(section => section.category)).ToArray();
                foreach (var category in missingCategories)
                {
                    var sectionObj = Instantiate(TabInfoManager.statSectionTemplate, this.StatHolder.transform);
                    var section = sectionObj.AddComponent<StatSection>();
                    section.playerFrame = this;
                    section.category = category;
                    this.statSections.Add(section);
                }

                var sectionOrder = TabInfoManager.Categories.Values.OrderBy(c => c.priority).ThenBy(c => c.name.ToLower()).ToArray();
                for (int i = 0; i< sectionOrder.Length; i++)
                {
                    this.statSections.Where(section => section.category == sectionOrder[i]).First().transform.SetSiblingIndex(i);
                }
            }

            foreach (var section in this.statSections)
            {
                if (section.gameObject.activeSelf != section.category.Stats.Values.Any(stat => stat.displayCondition(player)))
                {
                    section.gameObject.SetActive(section.category.Stats.Values.Any(stat => stat.displayCondition(player)));
                }
            }
        }
    }
}
