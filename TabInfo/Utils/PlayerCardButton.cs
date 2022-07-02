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
            this.Text.text = CardInitials(card);
        }
        internal string CardInitials(CardInfo card)
        {
            string text = card.cardName;
            text = text.Substring(0, 2);
            string text2 = text[0].ToString().ToUpper();
            if (text.Length > 1)
            {
                string str = text[1].ToString().ToLower();
                text = text2 + str;
            }
            else
            {
                text = text2;
            }
            return text;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {

        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }
    }
}
