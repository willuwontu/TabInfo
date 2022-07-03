using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnboundLib;

namespace TabInfo.Utils
{
    public class PlayerCardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public PlayerCardBar cardBar;
        public CardInfo card;
        private TextMeshProUGUI _text = null;
        private GameObject displayedCard;
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
            if (displayedCard != null)
            {
                UnityEngine.GameObject.Destroy(displayedCard);
            }
            displayedCard = Instantiate(TabInfoManager.cardHolderTemplate, TabInfoManager.canvas.transform);
            displayedCard.transform.position = this.gameObject.transform.position;
            var cardObj = Instantiate(this.card.gameObject, displayedCard.transform);
            var cardVis = cardObj.GetComponentInChildren<CardVisuals>();
            cardVis.firstValueToSet = true;
            cardObj.transform.localPosition = Vector3.zero;
            Collider2D[] componentsInChildren = displayedCard.GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].enabled = false;
            }
            cardObj.GetComponentInChildren<Canvas>().sortingLayerName = "MostFront";
            cardObj.GetComponentInChildren<GraphicRaycaster>().enabled = false;
            cardObj.GetComponentInChildren<SetScaleToZero>().enabled = false;
            cardObj.GetComponentInChildren<SetScaleToZero>().transform.localScale = Vector3.one * 1.15f;
            this.ExecuteAfterFrames(1, () => { 
                cardObj.transform.localScale = Vector3.one * 25f;
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (displayedCard != null)
            {
                UnityEngine.GameObject.Destroy(displayedCard);
            }
        }
        private void OnDisable()
        {
            if (displayedCard != null)
            {
                UnityEngine.GameObject.Destroy(displayedCard);
            }
        }
    }
}
