using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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
    }
}
