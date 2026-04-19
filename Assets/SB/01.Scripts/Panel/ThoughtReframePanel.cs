using System;
using Core.EventBus;
using UnityEngine;

namespace SB._01.Scripts.Panel
{
    public class ThoughtReframePanel : MonoBehaviour,IEntityComponent
    {
        private void Awake()
        {
            Bus<ShowThoughtReframePanelEvent>.OnEvent += ShowPanel;
            gameObject.SetActive(false);
      
        }

        private void OnDestroy()
        {
            Bus<ShowThoughtReframePanelEvent>.OnEvent -= ShowPanel;
        }

        public void Initialize(Entity entity)
        {
            
        }

        public void AcceptanceButton()
        {
            Bus<AcceptanceButton_ThoughtReframePanelEvent>.Raise(new AcceptanceButton_ThoughtReframePanelEvent());
            gameObject.SetActive(false);
        }
        
        public void RejectionButton()
        {
            gameObject.SetActive(false);
            Bus<RejectionButton_ThoughtReframePanelEvent>.Raise(new RejectionButton_ThoughtReframePanelEvent());
        }

        private void ShowPanel(ShowThoughtReframePanelEvent evt)
        {
            gameObject.SetActive(true);
        }
    }
}