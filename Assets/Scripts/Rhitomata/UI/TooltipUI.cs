using UnityEngine;
using TMPro;

namespace Arphros.Interface
{
    public class TooltipUI : MonoBehaviour
    {
        public static TooltipUI Instance
        {
            get => _Instance = _Instance ?? FindFirstObjectByType<TooltipUI>();
            set => _Instance = value;
        }

        private static TooltipUI _Instance;

        public bool hideOnStart = true;

        [Header("Components")]
        [SerializeField]
        private RectTransform rect;
        [SerializeField]
        private GameObject viewObject;
        [SerializeField]
        private RectTransform canvasRect;
        [SerializeField]
        private RectTransform backgroundRect;
        [SerializeField]
        private TMP_Text textLabel;

        [Header("Layout")]
        public Vector2 padding = new Vector2(8, 8);
        public float edgeTolerance = 5f;

        private void Start()
        {
            _Instance = this;

            if(hideOnStart)
                _Hide();
        }

        private void OnDestroy()
        {
            _Instance = null;
        }

        private void Update()
        {
            Vector2 anchoredPosition = Input.mousePosition / canvasRect.localScale.x;

            if (anchoredPosition.x + backgroundRect.rect.width > (canvasRect.rect.width - edgeTolerance))
                anchoredPosition.x = canvasRect.rect.width - backgroundRect.rect.width - edgeTolerance;

            if (anchoredPosition.y + backgroundRect.rect.height > (canvasRect.rect.height - edgeTolerance))
                anchoredPosition.y = canvasRect.rect.height - backgroundRect.rect.height - edgeTolerance;

            rect.anchoredPosition = anchoredPosition;
        }

        private void SetTooltipText(string text)
        {
            textLabel.SetText(text);
            textLabel.ForceMeshUpdate();

            Vector2 textSize = textLabel.GetRenderedValues(false);
            backgroundRect.sizeDelta = textSize + padding;
        }

        private void _Show(string text)
        {
            viewObject.SetActive(true);
            SetTooltipText(text);
        }

        private void _Hide()
        {
            viewObject.SetActive(false);
        }

        public static void Show(string text)
        {
            Instance?._Show(text);
        }

        public static void Hide()
        {
            Instance?._Hide();
        }
    }
}