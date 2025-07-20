using UnityEngine;
using System;


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

        [Header("Properties")]
        [SerializeField, Range(0f, 1f)] private float _value;
        [SerializeField, Range(0f, 1f)] private float _size = 0.2f;

        public float value {
            get => _value;
            set {
                if (_value == value) return;

                _value = value;
                UpdateValue();
            }
        }

        public float size {
            get => _size;
            set {
                if (_size == value) return;

                _size = value;
                UpdateSize();
            }
        }

        private void Start() {
            peekStart.onDragDelta.AddListener(OnStartRangeMoved);
            peekRange.onDragDelta.AddListener(OnRangeMoved);
            peekEnd.onDragDelta.AddListener(OnEndRangeMoved);
        }

        private void OnStartRangeMoved(Vector2 delta) {
            float totalWidth = bounds.rect.width;

            float rangeStartPx = _value * (totalWidth - _size * totalWidth);
            float rangeWidthPx = _size * totalWidth;
            float rangeEndPx = rangeStartPx + rangeWidthPx;

            float newStartPx = Mathf.Clamp(rangeStartPx + delta.x, 0f, rangeEndPx - 1f);
            float newWidthPx = rangeEndPx - newStartPx;

            _size = newWidthPx / totalWidth;
            _value = newStartPx / (totalWidth - newWidthPx);

            UpdateSize();
        }

        private void OnRangeMoved(Vector2 delta) {
            var deltaXLocal = delta.x / (bounds.rect.width - peekRange.rectTransform.rect.width);
            _value += deltaXLocal;
            _value = Mathf.Clamp01(_value);
            UpdateValue();
        }

        private void OnEndRangeMoved(Vector2 delta) {
            float totalWidth = bounds.rect.width;

            float start = _value * (totalWidth - _size * totalWidth);
            float end = start + _size * totalWidth + delta.x;

            end = Mathf.Clamp(end, start + 1f, totalWidth);

            float newSize = (end - start) / totalWidth;
            float newValue = start / (totalWidth - newSize * totalWidth);

            _size = Mathf.Clamp01(newSize);
            _value = Mathf.Clamp01(newValue);

            UpdateSize();
        }

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

            var size = peekRange.rectTransform.sizeDelta;
            size.x = Mathf.Lerp(0, bounds.rect.width, _size);
            peekRange.rectTransform.sizeDelta = size;
            UpdateValue();
        }
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

            EditorGUILayout.Slider(_valueProp, 0f, 1f, new GUIContent("Value"));
            EditorGUILayout.Slider(_sizeProp, 0f, 1f, new GUIContent("Size"));

            serializedObject.ApplyModifiedProperties();

            // Get the target and call update methods if values changed
            HorizontalAdjustableScrollbar scrollbar = (HorizontalAdjustableScrollbar)target;

            if (!Mathf.Approximately(oldValue, _valueProp.floatValue)) {
                scrollbar.UpdateValue();
            }

            if (!Mathf.Approximately(oldSize, _sizeProp.floatValue)) {
                scrollbar.UpdateSize();
            }
        }
    }
#endif
}