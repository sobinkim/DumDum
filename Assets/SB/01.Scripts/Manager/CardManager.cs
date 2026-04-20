using System.Collections.Generic;
using Core.EventBus;
using SB._01.Scripts.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace SB._01.Scripts.Manager
{
    public class CardManager : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private List<EmotionIcon> emotionIcons = new List<EmotionIcon>();

        private Dictionary<EmotionType, Image> emotionIconDict;

        private void Awake()
        {
            InitDictionary();
            Bus<RequestCreateCardEvent>.OnEvent += RequestCreateCard;
        }

        private void OnDestroy()
        {
            Bus<RequestCreateCardEvent>.OnEvent -= RequestCreateCard;
        }

        public void Initialize(Entity entity)
        {
        }

        private void InitDictionary()
        {
            emotionIconDict = new Dictionary<EmotionType, Image>();

            foreach (var icon in emotionIcons)
            {
                if (!emotionIconDict.ContainsKey(icon.emotionType))
                {
                    emotionIconDict.Add(icon.emotionType, icon.iconImage);
                }
                else
                {
                    Debug.LogWarning($"중복된 EmotionType 존재: {icon.emotionType}");
                }
            }
        }

        private void RequestCreateCard(RequestCreateCardEvent evt)
        {
            CardData cardData = evt.cardData;
            Card newCard = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<Card>();

            Image iconImage = GetCardImage(cardData.emotionType);
            newCard.SettingCard(cardData, iconImage);
        }

        private Image GetCardImage(EmotionType emotionType)
        {
            if (emotionIconDict.TryGetValue(emotionType, out var image))
            {
                return image;
            }

            Debug.LogWarning($"해당 EmotionType({emotionType})에 맞는 아이콘이 없음");
            return null;
        }
    }
}