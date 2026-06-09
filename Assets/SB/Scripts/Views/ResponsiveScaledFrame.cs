using UnityEngine;

namespace SB.App.Views
{
    public sealed class ResponsiveScaledFrame : MonoBehaviour
    {
        [SerializeField] private RectTransform frame;
        [SerializeField] private Vector2 referenceSize = new Vector2(375f, 667f);

        private void Awake()
        {
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void LateUpdate()
        {
            Apply();
        }

        public void SetFrame(RectTransform targetFrame, Vector2 targetReferenceSize)
        {
            frame = targetFrame;
            referenceSize = targetReferenceSize;
            Apply();
        }

        private void Apply()
        {
            if (frame == null)
                frame = transform as RectTransform;

            RectTransform parent = frame.parent as RectTransform;
            if (frame == null || parent == null)
                return;

            float widthScale = parent.rect.width / referenceSize.x;
            float heightScale = parent.rect.height / referenceSize.y;
            float fitScale = Mathf.Min(widthScale, heightScale);

            frame.anchorMin = new Vector2(0.5f, 0.5f);
            frame.anchorMax = new Vector2(0.5f, 0.5f);
            frame.pivot = new Vector2(0.5f, 0.5f);
            frame.sizeDelta = referenceSize;
            frame.anchoredPosition = Vector2.zero;
            frame.localScale = new Vector3(fitScale, fitScale, 1f);
        }
    }
}
