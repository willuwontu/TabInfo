using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace TabInfo.Utils
{
    class StatObject : MonoBehaviour
    {
        public StatSection section;
        public Stat stat;
        private TextMeshProUGUI _statName = null;
        public TextMeshProUGUI StatName
        {
            get
            {
                if (this._statName is null)
                {
                    this._statName = this.transform.Find("Name").GetComponent<TextMeshProUGUI>();
                }
                return this._statName;
            }
        }
        private TextMeshProUGUI _statValue = null;
        public TextMeshProUGUI StatValue
        {
            get
            {
                if (this._statValue is null)
                {
                    this._statValue = this.transform.Find("Value").GetComponent<TextMeshProUGUI>();
                }
                return this._statValue;
            }
        }
        private void Update()
        {
            this.StatName.text = this.stat.name + ":";
            this.StatValue.text = this.stat.DisplayValue(this.section.playerFrame.player);
        }
    }
}
