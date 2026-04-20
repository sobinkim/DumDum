using System;
using Core.EventBus;
using TMPro;
using UnityEngine;

namespace SB._01.Scripts.Panel
{
    public class CardPanelComponent : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private TMP_Text _mainContent;

        private void Awake()
        {
            Bus<ShowCardPanel>.OnEvent += ShowPanel;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Bus<ShowCardPanel>.OnEvent -= ShowPanel;
        }

        public void Initialize(Entity entity)
        {
        }

        private void ShowPanel(ShowCardPanel evt)
        {
            Bus<ShowFocusPanel>.Raise(new ShowFocusPanel());
            Bus<MainPanelButtonEnable>.Raise(new MainPanelButtonEnable(false));
            gameObject.SetActive(true);
            _dateText.text = evt.cardData.createdDate.ToShortDateString();
            _mainContent.text = evt.cardData.mainContents;
        }

        public void HidePanel()
        {
            Bus<HideFocusPanel>.Raise(new HideFocusPanel());
            Bus<MainPanelButtonEnable>.Raise(new MainPanelButtonEnable(true));
            gameObject.SetActive(false);
        }
    }
}