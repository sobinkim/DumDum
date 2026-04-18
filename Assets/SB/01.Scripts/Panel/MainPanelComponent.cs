using System;
using Core.EventBus;
using UnityEngine;

namespace SB._01.Scripts.Panel
{
    public class MainPanelComponent : MonoBehaviour, IEntityComponent
    {
        private Entity _entity;

        private void Awake()
        {
        }

        private void OnDestroy()
        {
        }

        public void Initialize(Entity entity)
        {
            _entity = entity;
        }

        public void ShowWorryInputPanel()
        {
            Bus<WorryEnterEvent>.Raise(new WorryEnterEvent());
        }
    }
}