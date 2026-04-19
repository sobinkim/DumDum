using System;
using Core.EventBus;
using UnityEngine;

namespace SB._01.Scripts.Panel
{
    public class WorryInputFaillPanelComponent : MonoBehaviour, IEntityComponent
    {
        private void Awake()
        {
            DisablePanel();
            Bus<SubmitFailEvent>.OnEvent += ShowPanel;
        }

        public void Initialize(Entity entity)
        {
        }

        public void DisablePanel()
        {
            gameObject.SetActive(false);
        }

        private void ShowPanel(SubmitFailEvent evt)
        {
            gameObject.SetActive(true);
        }
    }
}