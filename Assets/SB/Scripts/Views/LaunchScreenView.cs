using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SB.App.Views
{
    public sealed class LaunchScreenView : UIScreenView, IPointerClickHandler
    {
        public event Action ContinueRequested;

        public void OnPointerClick(PointerEventData eventData)
        {
            ContinueRequested?.Invoke();
        }
    }
}
