using System;
using Core.EventBus;
using SB._01.Scripts.Interface;
using TMPro;
using UnityEngine;

namespace SB._01.Scripts.Panel
{
    public class ThoughtReframePanel : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private WorryData _worryData;
        [SerializeField] private TMP_InputField _title_Text;
        [SerializeField] private TMP_InputField _main_Text;
        private CardData cardData = new CardData();

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

        private bool SetCardData()
        {
            cardData.emotionType = _worryData.emotionType;
            cardData.mainContents = _main_Text.text;
            cardData.title = _title_Text.text;

            if (_title_Text.text == String.Empty || _main_Text.text == String.Empty)
                return false;
            else
                return true;
        }

        public void AcceptanceButton()
        {
            Bus<AcceptanceButton_ThoughtReframePanelEvent>.Raise(new AcceptanceButton_ThoughtReframePanelEvent());
            if (RequestCreateCard())
            {
                DisablePanel();
            }
            
        }

        public void RejectionButton()
        {
            Bus<RejectionButton_ThoughtReframePanelEvent>.Raise(new RejectionButton_ThoughtReframePanelEvent());
            DisablePanel();
        }

        private void ShowPanel(ShowThoughtReframePanelEvent evt)
        {
            _worryData = evt.worryData;
            gameObject.SetActive(true);
            Bus<ShowSpeechBubbleEvent>.Raise(new ShowSpeechBubbleEvent());
            Bus<ShowFocusPanel>.Raise(new ShowFocusPanel());
        }

        private void DisablePanel()
        {
            Bus<MainPanelButtonEnable>.Raise(new MainPanelButtonEnable(true));
            Bus<HideFocusPanel>.Raise(new HideFocusPanel());
            Bus<HideSpeechBubbleEvent>.Raise(new HideSpeechBubbleEvent());
            gameObject.SetActive(false);
        }

        private bool RequestCreateCard()
        {
            CardDataClear();
            if (!SetCardData())
            {
                Bus<SubmitFailEvent>.Raise(new SubmitFailEvent());
                return false;
            }

            else
            {
                Bus<RequestCreateCardEvent>.Raise(new RequestCreateCardEvent(cardData));
                return true;
            }
        }

        private void CardDataClear()
        {
            cardData.emotionType = EmotionType.Default;
            cardData.mainContents = null;
            cardData.title = null;
        }
    }
}