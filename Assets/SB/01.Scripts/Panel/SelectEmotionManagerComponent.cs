using System;
using Core.EventBus;
using SB._01.Scripts.Interface;
using UnityEngine;
using UnityEngine.Serialization;

namespace SB._01.Scripts.Panel
{
    public class SelectEmotionManagerComponent : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private EmotionType _currentEmotionType;

        private void Awake()
        {
            Bus<WorrySubmittedEvent>.OnEvent += ResetCurrentEmotionType;
        }

        public void Initialize(Entity entity)
        {
        }

        public EmotionType GetEmotionType()
        {
            return _currentEmotionType;
        }

        public void SetEmotionType(EmotionType emotionType)
        {
            _currentEmotionType = emotionType;
        }

        private void ResetCurrentEmotionType(WorrySubmittedEvent evt)
        {
            _currentEmotionType = EmotionType.Default;
        }
    }
}