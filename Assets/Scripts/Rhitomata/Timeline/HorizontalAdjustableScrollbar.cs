using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Rhitomata {
    public class HorizontalAdjustableScrollbar : MonoBehaviour {
        [Header("Bounds")]
        [SerializeField] private RectTransform bounds;

        [Header("Handles")]
        [SerializeField] private DraggableHandle peekStart;
        [SerializeField] private DraggableHandle peekRange;
        [SerializeField] private DraggableHandle peekEnd;

        [Header("Events")]
        public UnityEvent onAnyChanged;
        public AdjustableEvent onValueChanged;
        public AdjustableEvent onSizeChanged;

        [Header("Properties")]
        [SerializeField, Range(0f, 1f)] private float _value;
        [SerializeField, Range(0f, 1f)] private float _size = 0.2f;

        public float value {
            get => _value;
            set {
                if (_value == value) return;

                _value = value;
                UpdateValue();
                onValueChanged?.Invoke(value);
                onAnyChanged?.Invoke();
            }
        }

        public float size {
            get => _size;
            set {
                if (_size == value) return;

                _size = value;
                UpdateSize();
                onSizeChanged?.Invoke(value);
                onAnyChanged?.Invoke();
            }
        }

        public float minRange => peekStart.rectTransform.anchoredPosition.x / bounds.rect.width;
        public float maxRange => peekEnd.rectTransform.anchoredPosition.x / bounds.rect.width;

        private void Start() {
            peekStart.onStartDrag.AddListener(OnStartRangeBegin);
            peekRange.onStartDrag.AddListener(OnRangeBegin);
            peekEnd.onStartDrag.AddListener(OnEndRangeBegin);

            peekStart.onDrag.AddListener(OnStartRangeDragged);
            peekRange.onDrag.AddListener(OnRangeDragged);
            peekEnd.onDrag.AddListener(OnEndRangeDragged);

            UpdateSize();
        }

        public void OnBackgroundDown(BaseEventData eventData) {
            AdjustValueBasedOnMouse();
        }

        public void OnBackgroundDrag(BaseEventData eventData) {
            AdjustValueBasedOnMouse();
        }

        private void AdjustValueBasedOnMouse() {
            var pos = GetLocalMousePosition();
            _value = Mathf.Clamp01((pos.x - (sizePx / 2f)) / valuePixelWidth);

            UpdateValue();
            onValueChanged?.Invoke(_value);
            onAnyChanged?.Invoke();
        }

        #region New Coordinate Implementation
        // --- INITIAL STATE ---
        private Vector2 _initialCursorPosition; // Already relative to the bounds
        private Vector2 _initialDraggablePosition;

        private void OnStartRangeBegin(PointerEventData eventData) {
            _initialCursorPosition = GetLocalMousePosition();
            _initialDraggablePosition = peekStart.rectTransform.anchoredPosition;
        }

        private void OnRangeBegin(PointerEventData eventData) {
            _initialCursorPosition = GetLocalMousePosition();
            _initialDraggablePosition = peekRange.rectTransform.anchoredPosition;
        }

        private void OnEndRangeBegin(PointerEventData arg0) {
            _initialCursorPosition = GetLocalMousePosition();
            _initialDraggablePosition = peekEnd.rectTransform.anchoredPosition;
        }

        // --- WHEN DRAGGED ---
        private void OnStartRangeDragged(PointerEventData eventData) {
            var pixelsDifference = GetLocalMousePosition() - _initialCursorPosition;

            // So we're keeping the peek end but changing the value and the size
            var rangeEndPx = valuePx + sizePx;
            var newStartPx = Mathf.Clamp(_initialDraggablePosition.x + pixelsDifference.x, 0f, rangeEndPx - 1f);
            var newWidthPx = rangeEndPx - newStartPx;

            _size = newWidthPx / pixelWidth;
            _value = newStartPx / (pixelWidth - newWidthPx);

            UpdateSize();
            onSizeChanged?.Invoke(_size);
            onValueChanged?.Invoke(_value);
            onAnyChanged?.Invoke();
        }

        private void OnRangeDragged(PointerEventData eventData) {
            if (size >= 1f) {
                _value = 0f;
                return;
            }

            var pixelsDifference = GetLocalMousePosition() - _initialCursorPosition;

            _value = (_initialDraggablePosition.x + pixelsDifference.x) / valuePixelWidth;
            _value = Mathf.Clamp01(_value);

            UpdateValue();
            onValueChanged?.Invoke(_value);
            onAnyChanged?.Invoke();
        }

        private void OnEndRangeDragged(PointerEventData eventData) {
            var pixelsDifference = GetLocalMousePosition() - _initialCursorPosition;

            var newEndPx = Mathf.Clamp(_initialDraggablePosition.x + pixelsDifference.x, valuePx + 1f, pixelWidth);
            var newSize = (newEndPx - valuePx) / pixelWidth;
            var newValue = valuePx / (pixelWidth - newSize * pixelWidth);

            _size = Mathf.Clamp01(newSize);
            _value = Mathf.Clamp01(newValue);

            UpdateSize();
            onSizeChanged?.Invoke(_size);
            onValueChanged?.Invoke(_value);
            onAnyChanged?.Invoke();
        }
        #endregion

        #region Helpers
        private float sizePx => _size * pixelWidth;
        private float valuePx => _value * valuePixelWidth;

        private float valuePixelWidth => bounds.rect.width - peekRange.rectTransform.rect.width;
        private float pixelWidth => bounds.rect.width;

        private Vector2 GetLocalMousePosition() {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(bounds, Input.mousePosition, null, out var mousePosRelative);
            mousePosRelative.y = bounds.rect.height - mousePosRelative.y; // Invert down - top to top - down
            return mousePosRelative;
        }
        #endregion

        #region UI Updates
        public void UpdateValue() {
            if (_size >= 1) {
                _value = 0f;
            }

            var pos = peekStart.rectTransform.anchoredPosition;
            pos.x = Mathf.Lerp(0, bounds.rect.width - peekRange.rectTransform.rect.width, _value);
            peekStart.rectTransform.anchoredPosition = pos;
            peekRange.rectTransform.anchoredPosition = pos;

            pos.x += peekRange.rectTransform.rect.width;
            peekEnd.rectTransform.anchoredPosition = pos;
        }

        public void UpdateSize() {
            if (_size >= 1) {
                _value = 0f;
            }

            var newSize = peekRange.rectTransform.sizeDelta;
            newSize.x = Mathf.Lerp(0, bounds.rect.width, _size);
            peekRange.rectTransform.sizeDelta = newSize;
            UpdateValue();
        }
        #endregion

        #region Old Relative Implementation
        public void OnEndRangeMoved(Vector2 delta) {
            var totalWidth = bounds.rect.width;

            var startPx = _value * (totalWidth - _size * totalWidth);
            var endPx = startPx + _size * totalWidth + delta.x;

            endPx = Mathf.Clamp(endPx, startPx + 1f, totalWidth);

            var newSize = (endPx - startPx) / totalWidth;
            var newValue = startPx / (totalWidth - newSize * totalWidth);

            _size = Mathf.Clamp01(newSize);
            _value = Mathf.Clamp01(newValue);

            UpdateSize();
            onSizeChanged?.Invoke(_size);
            onValueChanged?.Invoke(_value);
            onAnyChanged?.Invoke();
        }
        public void OnStartRangeMoved(Vector2 delta) {
            var totalWidth = bounds.rect.width;

            var rangeStartPx = _value * (totalWidth - _size * totalWidth);
            var rangeWidthPx = _size * totalWidth;
            var rangeEndPx = rangeStartPx + rangeWidthPx;

            var newStartPx = Mathf.Clamp(rangeStartPx + delta.x, 0f, rangeEndPx - 1f);
            var newWidthPx = rangeEndPx - newStartPx;

            _size = newWidthPx / totalWidth;
            _value = newStartPx / (totalWidth - newWidthPx);

            UpdateSize();
            onSizeChanged?.Invoke(_size);
            onValueChanged?.Invoke(_value);
            onAnyChanged?.Invoke();
        }

        public void OnRangeMoved(Vector2 delta) {
            var deltaXLocal = delta.x / (bounds.rect.width - peekRange.rectTransform.rect.width);
            _value += deltaXLocal;
            _value = Mathf.Clamp01(_value);
            UpdateValue();

            onValueChanged?.Invoke(_value);
            onAnyChanged?.Invoke();
        }
        #endregion
        [Serializable]
        public class AdjustableEvent : UnityEvent<float> { }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HorizontalAdjustableScrollbar))]
    public class HorizontalAdjustableScrollbarEditor : Editor {
        private SerializedProperty _valueProp;
        private SerializedProperty _sizeProp;

        private void OnEnable() {
            _valueProp = serializedObject.FindProperty("_value");
            _sizeProp = serializedObject.FindProperty("_size");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "_value", "_size");

            // Cache old values before drawing sliders
            float oldValue = _valueProp.floatValue;
            float oldSize = _sizeProp.floatValue;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
            EditorGUILayout.Slider(_valueProp, 0f, 1f, new GUIContent("Value"));
            EditorGUILayout.Slider(_sizeProp, 0f, 1f, new GUIContent("Size"));

            serializedObject.ApplyModifiedProperties();

            // Get the target and call update methods if values changed
            HorizontalAdjustableScrollbar scrollbar = (HorizontalAdjustableScrollbar)target;

            if (!Mathf.Approximately(oldValue, _valueProp.floatValue)) {
                scrollbar.UpdateValue();
                scrollbar.onValueChanged?.Invoke(scrollbar.value);
                scrollbar.onAnyChanged?.Invoke();
            }

            if (!Mathf.Approximately(oldSize, _sizeProp.floatValue)) {
                scrollbar.UpdateSize();
                scrollbar.onSizeChanged?.Invoke(scrollbar.size);
                scrollbar.onAnyChanged?.Invoke();
            }
        }
    }
#endif
}