using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Rhitomata.Timeline {
    public class TimelineView : MonoBehaviour {
        [Header("Audio")]
        public AudioSource source;

        [Header("Peek")]
        public float time;
        public DraggableHandle currentTimeHandle;
        public RectTransform[] currentTimeCursors;
        /// <summary>
        /// Limit on how far the user is supposed to be able to view in the timeline (in seconds)
        /// </summary>
        public Limit peekLimit = new(-5f, 5f);

        [Header("Viewport")]
        public RectTransform viewportBounds;
        public RectTransform timelineParentContent;
        public RectTransform timelineHeaderContent;

        [Header("Scrollbar")]
        public Scrollbar verticalScrollbar;
        public HorizontalAdjustableScrollbar horizontalScrollbar;

        [Header("Debug")]
        /// <summary>
        /// Limit on the current scrollbar range of what's on the screen (in seconds, too)
        /// </summary>
        [SerializeField]
        private Limit visibleRange;
        public Rect rect;
        public Vector2 sizeDelta;
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 anchoredPosition;

        private void Start() {
            verticalScrollbar.onValueChanged.AddListener(OnVerticalChanged);
            horizontalScrollbar.onAnyChanged.AddListener(OnHorizontalChanged);
            currentTimeHandle.onDragDelta.AddListener(OnCurrentTimeDragged);

            UpdatePeekLimit();
            UpdateVerticalSlider();
            OnHorizontalChanged();

            UpdateCurrentTimeCursor();
        }

        private void OnCurrentTimeDragged(Vector2 delta) {
            var deltaSeconds = delta.x * (visibleRange.length / timelineParentContent.rect.width);
            time += deltaSeconds;
            time = Mathf.Clamp(time, peekLimit.min, peekLimit.max);
            UpdateCurrentTimeCursor();
        }

        public void UpdateCurrentTimeCursor() {
            var pos = currentTimeHandle.rectTransform.anchoredPosition;
            var posSeconds = time - visibleRange.min;
            pos.x = posSeconds * (timelineParentContent.rect.width / visibleRange.length);

            ChangeCursorsX(pos.x);
        }

        public void ChangeCursorsX(float to) {
            foreach (var cursor in currentTimeCursors) {
                var pos = cursor.anchoredPosition;
                pos.x = to;
                cursor.anchoredPosition = pos;
            }
        }

        private void OnVerticalChanged(float value) {
            var pos = timelineParentContent.anchoredPosition;
            pos.y = Mathf.Lerp(0, timelineParentContent.rect.height - viewportBounds.rect.height, value);
            timelineParentContent.anchoredPosition = pos;

            pos.x = timelineHeaderContent.anchoredPosition.x;
            timelineHeaderContent.anchoredPosition = pos;
        }

        public void UpdateVerticalSlider() {
            if (timelineParentContent.rect.height < viewportBounds.rect.height) {
                verticalScrollbar.size = 1f;
                verticalScrollbar.SetValueWithoutNotify(0);

                return;
            }

            verticalScrollbar.size = viewportBounds.rect.height / timelineParentContent.rect.height;
            verticalScrollbar.SetValueWithoutNotify(Mathf.InverseLerp(0, timelineParentContent.rect.height - viewportBounds.rect.height, timelineParentContent.anchoredPosition.y));
        }

        private void OnHorizontalChanged() {
            visibleRange.min = Mathf.Lerp(peekLimit.min, peekLimit.max, horizontalScrollbar.minRange);
            visibleRange.max = Mathf.Lerp(peekLimit.min, peekLimit.max, horizontalScrollbar.maxRange);
            UpdateCurrentTimeCursor();
        }

        public void UpdateTimelineView() {

        }

        public void UpdatePeekLimit() {
            peekLimit.min = -5f;
            peekLimit.max = source.clip.length + 5f;
        }
    }

    [Serializable]
    public struct Limit {
        public float min;
        public float max;
        public float length => Mathf.Abs(max - min);

        public Limit(float min, float max) {
            this.min = min;
            this.max = max;
        }

        public static Limit none = new(0, 0);
        public static Limit one = new(0, 1);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Drawing a Vector2-like inspector for the struct <see cref="Limit"/>.
    /// Referenced from: <a href="https://discussions.unity.com/t/making-a-proper-drawer-similar-to-vector3-how/616416">Unity Forum Post</a>
    /// </summary>
    [CustomPropertyDrawer(typeof(Limit))]
    public class LimitDrawer : PropertyDrawer {
        private const float SubLabelSpacing = 4f;
        private const float BottomSpacing = 2f;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            pos.height -= BottomSpacing;

            label = EditorGUI.BeginProperty(pos, label, prop);
            Rect contentRect = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);

            GUIContent[] subLabels = {
                new("Min"),
                new("Max")
            };

            SerializedProperty[] properties = {
                prop.FindPropertyRelative("min"),
                prop.FindPropertyRelative("max")
            };

            DrawMultipleDoubleFields(contentRect, subLabels, properties);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight + BottomSpacing;
        }

        private static void DrawMultipleDoubleFields(Rect pos, GUIContent[] subLabels, SerializedProperty[] props) {
            int propCount = props.Length;
            float totalSpacing = (propCount - 1) * SubLabelSpacing;
            float fieldWidth = (pos.width - totalSpacing) / propCount;

            float originalLabelWidth = EditorGUIUtility.labelWidth;
            int originalIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect fieldRect = new Rect(pos.x, pos.y, fieldWidth, pos.height);

            for (int i = 0; i < propCount; i++) {
                EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(subLabels[i]).x;
                EditorGUI.PropertyField(fieldRect, props[i], subLabels[i]);
                fieldRect.x += fieldWidth + SubLabelSpacing;
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
            EditorGUI.indentLevel = originalIndent;
        }
    }
#endif
}