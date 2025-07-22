using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Rhitomata.UI
{
    public class Window : MonoBehaviour
    {
        [SerializeField]  private bool disallowOnPlaymode;
        [SerializeField]  private bool hideOnStart;
        [SerializeField]  private bool centerOnShow;
        [SerializeField]  private UnityEvent beforeHideOnStart;

        // Events
        public event Action onHide;
        public event Action onPostHide;
        public event Action onDestroy;

        private RectTransform rectTransform => transform as RectTransform;
        private DragHandler _draggable;
        private CanvasGroup _group;
        private Canvas _canvas;
        
        private const float TRANSITION_DURATION = 0.4f;

        private void Start()
        {
            rectTransform.anchoredPosition = new(0, 0);
            Initialize();

            if (!hideOnStart) return;
            
            beforeHideOnStart?.Invoke();
            gameObject.SetActive(false);
        }

        private void Initialize()
        {
            _canvas = _canvas ? _canvas : GetComponentInParent<Canvas>();
            _group = _group ? _group : GetComponent<CanvasGroup>();
            _draggable = GetComponentInChildren<DragHandler>(true);
        }

        public void Hide()
        {
            Initialize();
            if (Application.isPlaying) {
                transform.localScale = Vector3.one;
                transform.DOScale(0.8f, TRANSITION_DURATION).SetEase(Ease.OutCubic);
                _group.alpha = 1f;
                _group.DOFade(0f, TRANSITION_DURATION).OnComplete(() => {
                    gameObject.SetActive(false);
                    onPostHide?.Invoke();
                });
            } else {
                transform.localScale = new(0.8f, 0.8f, 0.8f);
                if (!_group)
                    _group = GetComponent<CanvasGroup>();
                _group.alpha = 0f;
                gameObject.SetActive(false);
                onPostHide?.Invoke();
            }
            onHide?.Invoke();
        }

        public void Show()
        {
            if (References.Instance.manager.state == State.Play && disallowOnPlaymode)
                return;

            Initialize();
            gameObject.SetActive(true);

            if (centerOnShow)
                rectTransform.anchoredPosition = new(0, 0);

            _draggable.SetAsFront();

            transform.localScale = Vector3.one * 0.8f;
            transform.DOScale(1f, TRANSITION_DURATION).SetEase(Ease.OutCubic);
            _group.alpha = 0f;
            _group.DOFade(1f, TRANSITION_DURATION);
        }

        private void OnDestroy()
        {
            onDestroy?.Invoke();
        }
    }
}