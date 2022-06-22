using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TabInfo.Utils
{
    public class PlayerFrame : MonoBehaviour
    {
        public PlayerCardBar cardBar;

        public GameObject CardButtonHolder
        {
            get
            {
                return this.transform.Find("Player Cards/Scroll View/Viewport/Content").gameObject;
            }
        }
    }
}
