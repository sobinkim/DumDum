using System;
using Core.EventBus;
using UnityEngine;
using UnityEngine.UI;

namespace SB._01.Scripts.Panel
{
    public class MainPanelComponent : MonoBehaviour, IEntityComponent
    {
        private Entity _entity;
        private Button _mainPanelButton;
        private void Awake()
        {
            _mainPanelButton = GetComponent<Button>();

            Bus<MainPanelButtonEnable>.OnEvent += ButtonClickEnable;
        }

        private void OnDestroy()
        {
        }

        private void ButtonClickEnable(MainPanelButtonEnable evt)
        {
            _mainPanelButton.enabled = evt.enable;
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