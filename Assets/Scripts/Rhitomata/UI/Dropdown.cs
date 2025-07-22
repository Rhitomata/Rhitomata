using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rhitomata {
    [RequireComponent(typeof(VerticalLayoutGroup))]
    [RequireComponent(typeof(RectTransform))]
    public class Dropdown : MonoBehaviour, IDeselectHandler {
        [SerializeField] private bool dynamicHeight;
        private const float TRANSITION_TIME = 0.05f;

        private bool _isInitialized;
        private bool _isShown;
        private float _height;
        private RectTransform _rectTransform;
        private VerticalLayoutGroup _verticalLayoutGroup;
        private ContentSizeFitter _contentSizeFitter;
        private CanvasGroup _canvasGroup;

        private void Awake() {
            if (!_isInitialized) gameObject.SetActive(false);
        }

        private void Initialize() {
            if (_isInitialized) return;

            _rectTransform = GetComponent<RectTransform>();
            _verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
            _contentSizeFitter = GetComponent<ContentSizeFitter>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (!dynamicHeight) {
                _contentSizeFitter.enabled = false;
                _height = CalculateHeight();
            }

            _isInitialized = true;
        }

        public void ToggleVisibility() {
            if (_isShown)
                Hide();
            else
                Show();
        }

        public void Show() {
            Initialize();
            _isShown = true;
            _canvasGroup.interactable = true;
            EventSystem.current.SetSelectedGameObject(gameObject);

            if (dynamicHeight) _height = CalculateHeight();

            gameObject.SetActive(true);

            _rectTransform.DOKill();
            _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, 0);
            _rectTransform.DOSizeDelta(new Vector2(_rectTransform.sizeDelta.x, _height), TRANSITION_TIME);
        }

        public void HideInstant() {
            Initialize();
            _isShown = false;
            _canvasGroup.interactable = false;

            _rectTransform.DOKill();
            _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, 0);
            gameObject.SetActive(false);
        }

        public void Hide() {
            Initialize();
            _isShown = false;
            _canvasGroup.interactable = false;

            _rectTransform.DOKill();
            _rectTransform.DOSizeDelta(new Vector2(_rectTransform.sizeDelta.x, 0), TRANSITION_TIME).onComplete += () => {
                gameObject.SetActive(false);
            };
        }

        private float CalculateHeight() {
            var height = 0f;
            foreach (RectTransform t in transform) {
                height += t.sizeDelta.y + _verticalLayoutGroup.spacing;
            }
            height -= _verticalLayoutGroup.spacing;
            return height;
        }

        public void OnDeselect(BaseEventData eventData) {
            StartCoroutine(OnDeselectIE());
        }

        private IEnumerator OnDeselectIE() {
            yield return new WaitForEndOfFrame();

            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected && selected.transform.IsChildOf(transform.parent)) yield break;// note that we're checking transform.parent instead of transform

            Hide();
        }

        private void OnDisable() {
            HideInstant();
        }
    }
}
