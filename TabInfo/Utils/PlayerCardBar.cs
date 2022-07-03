using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TabInfo.Utils
{
    public class PlayerCardBar : MonoBehaviour
    {
        public PlayerFrame playerFrame;

        private Dictionary<int, PlayerCardButton> cardButtons = new Dictionary<int, PlayerCardButton>();
        private void Start()
        {
            for (int i = 0; i < playerFrame.player.data.currentCards.Count(); i++)
            {
                var card = playerFrame.player.data.currentCards[i];
                var cardButtonObj = Instantiate(TabInfoManager.cardButtonTemplate, this.transform);
                var cardButton = cardButtonObj.AddComponent<PlayerCardButton>();
                cardButton.card = card;
                cardButton.cardBar = this;
                cardButton.Text.text = cardButton.CardInitials(card);
                this.cardButtons.Add(i, cardButton);
            }
        }

        private void Update()
        {
            if (cardButtons.Count() < playerFrame.player.data.currentCards.Count())
            {
                for (int i = cardButtons.Count(); i < playerFrame.player.data.currentCards.Count(); i++)
                {
                    var card = playerFrame.player.data.currentCards[i];
                    var cardButtonObj = Instantiate(TabInfoManager.cardButtonTemplate, this.transform);
                    var cardButton = cardButtonObj.AddComponent<PlayerCardButton>();
                    cardButton.card = card;
                    cardButton.cardBar = this;
                    cardButton.Text.text = cardButton.CardInitials(card);
                    this.cardButtons.Add(i, cardButton);
              
                }
            }
            else if (cardButtons.Count() > playerFrame.player.data.currentCards.Count())
            {
                for (int i = cardButtons.Count(); i >= playerFrame.player.data.currentCards.Count(); i--)
                {
                    var cardButton = this.cardButtons[i];
                    UnityEngine.GameObject.Destroy(cardButton.gameObject);
                    this.cardButtons.Remove(i);
                }
            }

            for (int i = 0; i < playerFrame.player.data.currentCards.Count(); i++)
            {
                var card = playerFrame.player.data.currentCards[i];
                var cardButton = this.cardButtons[i];
                cardButton.card = card;
                cardButton.Text.text = cardButton.CardInitials(card);
            }
        }
    }
}
