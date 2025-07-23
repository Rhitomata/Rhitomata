using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Rhitomata.Useful;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Rhitomata.Timeline {
    public class TimelineView : MonoBehaviour {
        public References references;

        [Header("Peek")]
        public float cursorTime;
        public DraggableHandle currentTimeHandle;
        public RectTransform[] currentTimeCursors;
        /// <summary>
        /// Limit on how far the user is supposed to be able to view in the timeline (in seconds)
        /// </summary>
        public Limit peekLimit = new(-5f, 5f);

        [Header("Viewport")]
        public RectTransform viewportBounds;
        public RectTransform headerHolder;
        public RectTransform laneHolder;
        public RectTransform scrollingRect; // Holds keyframes & stuff
        public LaneView laneView;

        [Header("Lanes")] 
        public List<TimelineLane> lanes = new();
        
        [Header("Keyframes")]
        public GameObject keyframePrefab;

        [Header("Scrollbar")]
        public Scrollbar verticalScrollbar;
        public HorizontalAdjustableScrollbar horizontalScrollbar;

        /// <summary>
        /// Limit on the current scrollbar range of what's on the screen (in seconds)
        /// </summary>
        [Header("Debug")]
        public Limit visibleRange;

        public const float LANE_HEIGHT = 50f;

        private void Start() {
            DeleteAllKeyframes();

            verticalScrollbar.onValueChanged.AddListener(OnVerticalChanged);
            horizontalScrollbar.onAnyChanged.AddListener(OnAnyHorizontalChanged);
            currentTimeHandle.onDragDelta.AddListener(OnCurrentTimeDragged);
            
            UpdatePeekLimit();
            UpdateVerticalSlider();
            OnAnyHorizontalChanged();

            UpdateCurrentTimeCursor();
        }

        public void OnResized() {
            UpdatePeekLimit();
            UpdateVerticalSlider();
            OnAnyHorizontalChanged();

            UpdateCurrentTimeCursor();
        }

        public void CreateKeyframeAtCursor() {
            var mousePosRelative = GetLocalPoint(scrollingRect, Input.mousePosition);
            var rowIndex = (int)(-mousePosRelative.y / LANE_HEIGHT);
            var time = GetTime(mousePosRelative.x);

            CreateKeyframe(time, lanes[rowIndex]);
        }

        private Keyframe CreateKeyframe(float toTime, TimelineLane lane) {
            return lane.CreateKeyframe(toTime);
        }
        
        private Keyframe CreateKeyframe(float toTime, int rowIndex) {
            var keyframe = Instantiate(keyframePrefab, scrollingRect).AddComponent<Keyframe>();
            keyframe.Initialize(toTime, rowIndex);
            return keyframe;

            // TODO: Add to a list that hasn't been made yet
        }
        
        public Keyframe CreatePredefinedKeyframe(float toTime, TimelineLane lane) {
            var keyframe = Instantiate(keyframePrefab, scrollingRect).AddComponent<Keyframe>();
            keyframe.lane = lane;
            keyframe.Initialize(toTime, lane.centerHeight);
            return keyframe;
        }
        
        public T CreatePredefinedKeyframe<T, T1>(float toTime, T1 lane) where T : Keyframe where T1 : TimelineLane {
            var keyframe = Instantiate(keyframePrefab, scrollingRect).AddComponent<T>();
            keyframe.lane = lane;
            keyframe.Initialize(toTime, lane.centerHeight);
            return keyframe;
        }

        private void DeleteAllKeyframes() {
            foreach (Transform t in scrollingRect) {
                Destroy(t.gameObject);
            }
        }

        private void Update() {
            if (InputManager.IsEditingOnInputField()) return;
            
            if (Input.GetKeyDown(KeyCode.R)) DeleteAllKeyframes();
            if (Input.GetKeyDown(KeyCode.I)) CreateKeyframeAtCursor();
        }

        /// <summary>
        /// Get the X position of a time based on the current zoom level.
        /// </summary>
        public float GetX(float time) {
            return time * (laneHolder.rect.width / visibleRange.length);
        }

        /// <summary>
        /// Get the time of an X position based on the current zoom level.
        /// </summary>
        /// <param name="x"></param>
        public float GetTime(float x) {
            return x * visibleRange.length / laneHolder.rect.width;
        }

        private void OnVerticalChanged(float value) {
            var pos = laneHolder.anchoredPosition;
            pos.y = Mathf.Lerp(0, laneHolder.rect.height - viewportBounds.rect.height, value);
            laneHolder.anchoredPosition = pos;

            pos.x = headerHolder.anchoredPosition.x;
            headerHolder.anchoredPosition = pos;

            pos.x = scrollingRect.anchoredPosition.x;
            scrollingRect.anchoredPosition = pos;
        }

        private void OnAnyHorizontalChanged() {
            visibleRange.min = Mathf.Lerp(peekLimit.min, peekLimit.max, horizontalScrollbar.minRange);
            visibleRange.max = Mathf.Lerp(peekLimit.min, peekLimit.max, horizontalScrollbar.maxRange);

            var keyframes = GetComponentsInChildren<Keyframe>();// TODO: Use lists instead of GetComponentsInChildren
            foreach (var keyframe in keyframes) {
                var x = GetX(keyframe.time);
                keyframe.SetX(x);
            }

            UpdateCurrentTimeCursor();
            UpdateKeyframeHolder();
        }

        public void UpdateKeyframeHolder() {
            // Move keyframe holder (scrollingRect)
            var posSeconds = -visibleRange.min;
            var x = posSeconds * (viewportBounds.rect.width / visibleRange.length);
            scrollingRect.anchoredPosition = new Vector2(x, scrollingRect.anchoredPosition.y);
        }

        private void OnCurrentTimeDragged(Vector2 delta) {
            var deltaSeconds = delta.x * (visibleRange.length / viewportBounds.rect.width);
            cursorTime += deltaSeconds;
            cursorTime = Mathf.Clamp(cursorTime, peekLimit.min, peekLimit.max);
            UpdateCurrentTimeCursor();
        }

        public void UpdateCurrentTimeCursor() {
            var pos = currentTimeHandle.rectTransform.anchoredPosition;
            var posSeconds = cursorTime - visibleRange.min;
            pos.x = posSeconds * (laneHolder.rect.width / visibleRange.length);

            ChangeCursorsX(pos.x);
        }

        public void ChangeCursorsX(float to) {
            foreach (var cursor in currentTimeCursors) {
                var pos = cursor.anchoredPosition;
                pos.x = to;
                cursor.anchoredPosition = pos;
            }
        }

        public void UpdateVerticalSlider() {
            if (laneHolder.rect.height < viewportBounds.rect.height) {
                verticalScrollbar.size = 1f;
                verticalScrollbar.SetValueWithoutNotify(0);

                return;
            }

            verticalScrollbar.size = viewportBounds.rect.height / laneHolder.rect.height;
            verticalScrollbar.SetValueWithoutNotify(Mathf.InverseLerp(0, laneHolder.rect.height - viewportBounds.rect.height, laneHolder.anchoredPosition.y));
        }

        public void UpdatePeekLimit() {
            peekLimit.min = -5f;
            peekLimit.max = references.music.clip.length + 5f;
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
        private const float SUB_LABEL_SPACING = 4f;
        private const float BOTTOM_SPACING = 2f;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            pos.height -= BOTTOM_SPACING;

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
            return EditorGUIUtility.singleLineHeight + BOTTOM_SPACING;
        }

        private static void DrawMultipleDoubleFields(Rect pos, GUIContent[] subLabels, SerializedProperty[] props) {
            var propCount = props.Length;
            var totalSpacing = (propCount - 1) * SUB_LABEL_SPACING;
            var fieldWidth = (pos.width - totalSpacing) / propCount;

            var originalLabelWidth = EditorGUIUtility.labelWidth;
            var originalIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var fieldRect = new Rect(pos.x, pos.y, fieldWidth, pos.height);

            for (var i = 0; i < propCount; i++) {
                EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(subLabels[i]).x;
                EditorGUI.PropertyField(fieldRect, props[i], subLabels[i]);
                fieldRect.x += fieldWidth + SUB_LABEL_SPACING;
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
            EditorGUI.indentLevel = originalIndent;
        }
    }
#endif
}