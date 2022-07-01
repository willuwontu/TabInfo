using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TabInfo.Utils
{
    class StatSection : MonoBehaviour
    {
        public PlayerFrame playerFrame;
        public StatCategory category;
        private TextMeshProUGUI _title = null;
        public TextMeshProUGUI Title
        {
            get
            {
                if (this._title is null)
                {
                    this._title = this.transform.Find("Stat Section Header/Section Name").GetComponent<TextMeshProUGUI>();
                }
                return this._title;
            }
        }
        private GameObject _statHolder = null;
        public GameObject StatHolder
        {
            get
            {
                if (this._statHolder is null)
                {
                    this._statHolder = this.transform.Find("Stat Section Stats Holder").gameObject;
                }
                return this._statHolder;
            }
        }
        private Button _button = null;
        public Button Button
        {
            get
            {
                if (this._button is null)
                {
                    this._button = this.transform.Find("Stat Section Header").GetComponent<Button>();
                }
                return this._button;
            }
        }
        private bool toggled = true;
        private void OnHeaderClicked()
        {
            this.toggled = !this.toggled;

            this.StatHolder.SetActive(toggled);
        }

        private List<StatObject> statObjects = new List<StatObject>();

        public void Start()
        {
            this.Button.onClick.AddListener(OnHeaderClicked);

            foreach (var stat in this.category.Stats.Values.OrderBy(stat => stat.name))
            {
                var statObj = Instantiate(TabInfoManager.statObjectTemplate, this.StatHolder.transform);
                var child = statObj.AddComponent<StatObject>();
                child.stat = stat;
                child.section = this;
                statObjects.Add(child);
            }
        }

        public void Update()
        {
            if (this.statObjects.Count() != this.category.Stats.Values.Count())
            {
                var extraStats = this.statObjects.Select(stat => stat.stat).Except(this.category.Stats.Values).ToArray();
                var extraStatObjs = this.statObjects.Where(stat => extraStats.Contains(stat.stat)).ToArray();
                foreach (var stat in extraStatObjs) { UnityEngine.GameObject.Destroy(stat.gameObject); }
                var missingStats = this.category.Stats.Values.Except(this.statObjects.Select(stat => stat.stat)).ToArray();
                foreach (var stat in missingStats)
                {
                    var statObj = Instantiate(TabInfoManager.statObjectTemplate, this.StatHolder.transform);
                    var child = statObj.AddComponent<StatObject>();
                    child.stat = stat;
                    child.section = this;
                    statObjects.Add(child);
                }

                var statOrder = this.category.Stats.Values.OrderBy(stat => stat.name).ToArray();
                for (int i = 0; i < statOrder.Length; i++)
                {
                    statObjects.Where(statObj => statObj.stat == statOrder[i]).First().gameObject.transform.SetSiblingIndex(i);
                }
            }

            foreach (var statObj in statObjects)
            {
                if (statObj.gameObject.activeSelf != statObj.stat.displayCondition(this.playerFrame.player))
                {
                    statObj.gameObject.SetActive(statObj.stat.displayCondition(this.playerFrame.player));
                }
            }
        }
    }
}
