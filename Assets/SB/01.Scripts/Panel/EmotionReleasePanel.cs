using System;
using Core.EventBus;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SB._01.Scripts.Panel
{
    public class EmotionReleasePanel : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private Image characterImage;
        [SerializeField] private Image[] speechbubbles;

        private void Awake()
        {
            Bus<HideSpeechBubbleEvent>.OnEvent += HideSpeechBubbles;
            Bus<ShowSpeechBubbleEvent>.OnEvent += ShowSpeechBubbles;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Bus<HideSpeechBubbleEvent>.OnEvent -= HideSpeechBubbles;
            Bus<ShowSpeechBubbleEvent>.OnEvent -= ShowSpeechBubbles;
        }

        public void Initialize(Entity entity)
        {
        }

        private void ShowSpeechBubbles(ShowSpeechBubbleEvent evt)
        {
            gameObject.SetActive(true);
        }

        private void HideSpeechBubbles(HideSpeechBubbleEvent evt)
        {
            gameObject.SetActive(false);
        }
    }
}