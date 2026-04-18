using SB._01.Scripts.Interface;
using UnityEngine;

namespace SB._01.Scripts.Panel
{
    public class SelectEmotionManagerComponent : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private EmotionType _emotionType;


        public void Initialize(Entity entity)
        {
        }

        public EmotionType GetEmotionType()
        {
            return _emotionType;
        }

        public void SetEmotionType(EmotionType emotionType)
        {
            _emotionType = emotionType;
        }
    }
}