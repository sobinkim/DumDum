using System;
using Core.EventBus;
using SB._01.Scripts.Interface;
using UnityEngine;

namespace SB._01.Scripts.Manager
{
    public class CardManager : MonoBehaviour,IEntityComponent
    {
        
        [SerializeField] private GameObject cardPrefab;

        private void Awake()
        {
            Bus<RequestCreateCardEvent>.OnEvent += RequestCreateCard;
        }

        private void OnDestroy()
        {
            Bus<RequestCreateCardEvent>.OnEvent -= RequestCreateCard;
        }

        public void Initialize(Entity entity)
        {
            
        }

        private void RequestCreateCard(RequestCreateCardEvent evt)
        {
            CardData cardData = evt.cardData;
            Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, transform);
        }
        
        
    }
    
    
    
}