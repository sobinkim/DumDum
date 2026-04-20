using System;
using Core.EventBus;
using SB._01.Scripts.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB._01.Scripts
{
    public class Card : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private string _mainContent;
        [SerializeField] private Image _Icon;
        private CardData _cardData;

        public void SettingCard(CardData cardData, Image icon)
        {
            _cardData = cardData;
            _title.text = cardData.title;
            _mainContent = cardData.mainContents;
            _Icon = icon;
            _cardData.createdDate = DateTime.Now;
        }

        public void ShowCardPanel()
        {
            Bus<ShowCardPanel>.Raise(new ShowCardPanel(_cardData));
        }
    }
}