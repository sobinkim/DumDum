using System;
using Core.EventBus;
using SB._01.Scripts.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB._01.Scripts.Panel
{
    public class WorrInputPanelComponent : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private TMP_InputField inputField;
        private Entity _entity;
        private SelectEmotionManagerComponent _selectEmotionManager;
        [SerializeField] private bool isActive = false;
        
        private void Awake()
        {
            Bus<WorryEnterEvent>.OnEvent += ShowPanelHandle;
            Bus<WorrySubmittedEvent>.OnEvent += DisablePanel;
        }

        private void OnDestroy()
        {
            Bus<WorrySubmittedEvent>.OnEvent -= DisablePanel;
            Bus<WorryEnterEvent>.OnEvent -= ShowPanelHandle;
 
        }

        public void Initialize(Entity entity)
        {
            _entity = entity;
            _selectEmotionManager = _entity.GetCompo<SelectEmotionManagerComponent>();
            DisablePanel(new WorrySubmittedEvent());
        }

        private void ShowPanelHandle(WorryEnterEvent evt)
        {
            if (isActive)
            {
                print("이미 켜져있어");
                return;
            }
              
            Bus<ShowFocusPanel>.Raise(new ShowFocusPanel());
            Bus<ShowSpeechBubbleEvent>.Raise(new ShowSpeechBubbleEvent());
            
            isActive = true;
            gameObject.SetActive(true);
            
  
        }

      

        private void DisablePanel(WorrySubmittedEvent evt)
        {
            isActive = false;
            Bus<HideFocusPanel>.Raise(new HideFocusPanel());
            Bus<HideSpeechBubbleEvent>.Raise(new HideSpeechBubbleEvent());
            gameObject.SetActive(false);
        }

        public void WorrySubmitted()
        {
            WorryData newWorryData = new WorryData();

            string inputContent = inputField.text;
            EmotionType inputEmotionIcon = _selectEmotionManager.GetEmotionType();

            newWorryData.content = inputContent;
            newWorryData.emotionIcon = inputEmotionIcon;


            if (inputContent.Length == 0 || inputEmotionIcon == EmotionType.Default)
            {
                Bus<WorrySubmitFailEvent>.Raise(new WorrySubmitFailEvent());
            }
            else
            {
                Bus<WorrySubmittedEvent>.Raise(new WorrySubmittedEvent());
                Bus<ShowThoughtReframePanelEvent>.Raise(new ShowThoughtReframePanelEvent());
            }
        }
    }
}