using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Rhitomata.UI {
    [RequireComponent(typeof(CanvasGroup))]
    public class Window : MonoBehaviour {
        [SerializeField] private bool disallowOnPlaymode;
        [SerializeField] private bool hideOnAwake;
        [SerializeField] private bool centerOnShow;
        [SerializeField] private UnityEvent beforeHideOnStart;

        // Events
        public event Action onShow;
        public event Action onHide;
        public event Action onPostHide;
        public event Action onDestroy;

        private RectTransform rectTransform => transform as RectTransform;
        private DragHandler _draggable;
        private CanvasGroup _group;
        private Canvas _canvas;

        private Sequence _tweenSequence;
        [HideInInspector] public bool isShown;

        private const float TRANSITION_DURATION = 0.15f;
        private const float TRANSITION_SCALE = 0.9f;

        private void Awake() {
            rectTransform.anchoredPosition = new(0, 0);

            if (hideOnAwake) {
                beforeHideOnStart?.Invoke();
                Hide(true);
            } else {
                Show(true);
            }
        }

        private void Initialize() {
            _canvas = _canvas ? _canvas : GetComponentInParent<Canvas>();
            _group = _group ? _group : GetComponent<CanvasGroup>();
            _draggable = _draggable ? _draggable : GetComponentInChildren<DragHandler>(true);
        }

        public void Show() => Show(false);
        public void Show(bool instant = false) {
            if (isShown || References.Instance.manager.state == State.Play && disallowOnPlaymode) return;

            Initialize();
            isShown = true;
            _group.interactable = true;
            gameObject.SetActive(true);

            if (centerOnShow)
                rectTransform.anchoredPosition = new(0, 0);

            _draggable.SetAsFront();

            if (Application.isPlaying && !instant) {
                _tweenSequence?.Kill();
                _tweenSequence = DOTween.Sequence()
                    .Join(transform.DOScale(1f, TRANSITION_DURATION).SetEase(Ease.OutCubic))
                    .Join(_group.DOFade(1f, TRANSITION_DURATION));
            } else {
                transform.localScale = Vector3.one;
                _group.alpha = 1f;
            }
            onShow?.Invoke();
        }

        public void Hide() => Hide(false);
        public void Hide(bool instant = false) {
            if (!isShown) return;

            Initialize();

            isShown = false;
            _group.interactable = false;

            if (Application.isPlaying && !instant) {
                _tweenSequence?.Kill();
                _tweenSequence = DOTween.Sequence()
                    .Join(transform.DOScale(TRANSITION_SCALE, TRANSITION_DURATION).SetEase(Ease.OutCubic))
                    .Join(_group.DOFade(0f, TRANSITION_DURATION))
                    .OnComplete(() => {
                        gameObject.SetActive(false);
                        onPostHide?.Invoke();
                    });
            } else {
                transform.localScale = Vector3.one * TRANSITION_SCALE;
                _group.alpha = 0f;
                gameObject.SetActive(false);
                onPostHide?.Invoke();
            }
            onHide?.Invoke();
        }

        private void OnEnable() {
            if (!isShown) {
                Debug.LogWarning($"The window on {name} was shown but Show() wasn't called!");
                Show(true);
            }
        }

        private void OnDisable() {
            if (isShown) {
                Debug.LogWarning($"The window on {name} was hidden but Hide() wasn't called!");
                Hide(true);
            }
        }

        private void OnDestroy() {
            onDestroy?.Invoke();
        }
    }
}