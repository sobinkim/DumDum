using UnityEngine;
using UnityEngine.EventSystems;

namespace SB.App.Views
{
    public sealed class UIButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float hoverScale = 1.012f;
        [SerializeField] private float pressedScale = 0.985f;
        [SerializeField] private float speed = 6f;

        private Vector3 _baseScale = Vector3.one;
        private Vector3 _targetScale = Vector3.one;
        private bool _isHovering;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _targetScale = _baseScale;
        }

        private void OnEnable()
        {
            _baseScale = transform.localScale;
            _targetScale = _baseScale;
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * speed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            _targetScale = _baseScale * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            _targetScale = _baseScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _targetScale = _baseScale * pressedScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _targetScale = _isHovering ? _baseScale * hoverScale : _baseScale;
        }
    }
}
