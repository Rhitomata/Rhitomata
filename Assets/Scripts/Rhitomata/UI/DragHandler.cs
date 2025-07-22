using UnityEngine;
using UnityEngine.EventSystems;

namespace Rhitomata.UI
{
    public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private RectTransform panelRectTransform;

        [SerializeField]
        private bool goAsTopOnDrag = true;

        private Vector2 _pointerOffset;
        private RectTransform _canvasRectTransform;
        private bool _clampedToLeft;
        private bool _clampedToRight;
        private bool _clampedToTop;
        private bool _clampedToBottom;

        public void Start()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                _canvasRectTransform = canvas.transform as RectTransform;
                panelRectTransform = panelRectTransform ?? transform as RectTransform;
            }
            _clampedToLeft = false;
            _clampedToRight = false;
            _clampedToTop = false;
            _clampedToBottom = false;
        }

        public void SetAsFront()
        {
            if (goAsTopOnDrag)
                panelRectTransform.SetAsLastSibling();
        }

        #region IBeginDragHandler implementation

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (goAsTopOnDrag)
                panelRectTransform.SetAsLastSibling();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, eventData.position, eventData.pressEventCamera, out _pointerOffset);

        }

        #endregion

        #region IDragHandler implementation

        public void OnDrag(PointerEventData eventData)
        {
            if (panelRectTransform == null)
            {
                return;
            }
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform, eventData.position, eventData.pressEventCamera, out localPointerPosition))
            {
                panelRectTransform.localPosition = localPointerPosition - _pointerOffset;
                ClampToWindow();
                Vector2 clampedPosition = panelRectTransform.localPosition;
                if (_clampedToRight)
                {
                    clampedPosition.x = (_canvasRectTransform.rect.width * 0.5f) - (panelRectTransform.rect.width * (1 - panelRectTransform.pivot.x));
                }
                else if (_clampedToLeft)
                {
                    clampedPosition.x = (-_canvasRectTransform.rect.width * 0.5f) + (panelRectTransform.rect.width * panelRectTransform.pivot.x);
                }

                if (_clampedToTop)
                {
                    clampedPosition.y = (_canvasRectTransform.rect.height * 0.5f) - (panelRectTransform.rect.height * (1 - panelRectTransform.pivot.y));
                }
                else if (_clampedToBottom)
                {
                    clampedPosition.y = (-_canvasRectTransform.rect.height * 0.5f) + (panelRectTransform.rect.height * panelRectTransform.pivot.y);
                }
                panelRectTransform.localPosition = clampedPosition;
            }
        }

        #endregion

        #region IEndDragHandler implementation

        public void OnEndDrag(PointerEventData eventData)
        {

        }

        #endregion

        private void ClampToWindow()
        {
            var canvasCorners = new Vector3[4];
            var panelRectCorners = new Vector3[4];
            _canvasRectTransform.GetWorldCorners(canvasCorners);
            panelRectTransform.GetWorldCorners(panelRectCorners);

            if (panelRectCorners[2].x > canvasCorners[2].x)
            {
                if (!_clampedToRight)
                    _clampedToRight = true;
            }
            else if (_clampedToRight)
            {
                _clampedToRight = false;
            }
            else if (panelRectCorners[0].x < canvasCorners[0].x)
            {
                if (!_clampedToLeft)
                    _clampedToLeft = true;
            }
            else if (_clampedToLeft)
            {
                _clampedToLeft = false;
            }

            if (panelRectCorners[2].y > canvasCorners[2].y)
            {
                if (!_clampedToTop)
                    _clampedToTop = true;
            }
            else if (_clampedToTop)
            {
                _clampedToTop = false;
            }
            else if (panelRectCorners[0].y < canvasCorners[0].y)
            {
                if (!_clampedToBottom)
                    _clampedToBottom = true;
            }
            else if (_clampedToBottom)
            {
                _clampedToBottom = false;
            }
        }
    }
}