using UnityEngine;

namespace TabInfo.Utils
{
    public class PlayerCardBar : MonoBehaviour
    {
        public PlayerFrame playerFrame;

        private void Start()
        {
            this.playerFrame = this.gameObject.GetComponentInParent<PlayerFrame>();
        }
    }
}
