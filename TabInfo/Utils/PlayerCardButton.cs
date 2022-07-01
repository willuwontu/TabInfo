using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace TabInfo.Utils
{
    public class PlayerCardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public PlayerCardBar cardBar;
        public CardInfo card;
        private TextMeshProUGUI _text = null;
        public TextMeshProUGUI Text
        {
            get
            {
                if (this._text is null)
                {
                    this._text = this.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
                }
                return _text;
            }
        }
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
