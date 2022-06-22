using UnityEngine;
using UnityEngine.EventSystems;

namespace TabInfo.Utils
{
    public class PlayerCardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public PlayerCardBar cardBar;
        public CardInfo card;

        private void Start()
        {
            this.cardBar = this.gameObject.GetComponentInParent<PlayerCardBar>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {

        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }
    }
}
