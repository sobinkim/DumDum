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
            DisablePanel(new WorrySubmittedEvent());
            Bus<WorryEnterEvent>.OnEvent += ShowPanelHandle;
            Bus<WorrySubmittedEvent>.OnEvent += DisablePanel;
            Bus<WorrySubmitFailEvent>.OnEvent += SubmitFail;
        }

        private void OnDestroy()
        {
            Bus<WorrySubmittedEvent>.OnEvent -= DisablePanel;
            Bus<WorryEnterEvent>.OnEvent -= ShowPanelHandle;
            Bus<WorrySubmitFailEvent>.OnEvent -= SubmitFail;
        }

        public void Initialize(Entity entity)
        {
            _entity = entity;
            _selectEmotionManager = _entity.GetCompo<SelectEmotionManagerComponent>();
        }

        private void ShowPanelHandle(WorryEnterEvent evt)
        {
            if(isActive)
                return;
            
            isActive = true;
            gameObject.SetActive(true);
        }

        private void SubmitFail(WorrySubmitFailEvent evt)
        {
        }

        private void DisablePanel(WorrySubmittedEvent evt)
        {
            isActive = false;
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
            }
        }
    }
}