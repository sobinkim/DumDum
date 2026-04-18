using SB._01.Scripts.Interface;
using UnityEngine;

namespace SB._01.Scripts.Panel
{
    public class EmotionIconButton : MonoBehaviour
    {
        private Entity _entity;
        [SerializeField] private SelectEmotionManagerComponent _selectEmotionManager;
        [SerializeField] private EmotionType _emotionType;

        public void SendEmotionType()
        {
            _selectEmotionManager.SetEmotionType(_emotionType);
        }
    }
}