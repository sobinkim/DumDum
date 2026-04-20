using System;
using Core.EventBus;
using UnityEngine;

namespace SB._01.Scripts.Panel
{
    public class FocusPanel : MonoBehaviour
    {
        private void Awake()
        {
            Bus<ShowFocusPanel>.OnEvent += ShowFocusPanel;
            Bus<HideFocusPanel>.OnEvent += HideFocusPanel;

            gameObject.SetActive(false);
        }

        private void ShowFocusPanel(ShowFocusPanel evt)
        {
            gameObject.SetActive(true);
        }

        private void HideFocusPanel(HideFocusPanel evt)
        {
            gameObject.SetActive(false);
        }
    }
}