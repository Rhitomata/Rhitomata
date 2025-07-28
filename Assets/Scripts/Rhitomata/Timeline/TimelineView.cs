using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Rhitomata.Useful;
using TMPro;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Rhitomata.Timeline {
    public class TimelineView : MonoBehaviour {
        #region Variables
        public References references;

        [Header("Peek")]
        public float cursorTime { get; private set; }
        public DraggableHandle currentTimeHandle;
        public RectTransform[] currentTimeCursors;
        /// <summary>
        /// <para>Limit on how far the user is supposed to be able to view in the timeline (in seconds)</para>
        /// </summary>
        public Limit peekLimit = new(-5f, 5f);
        public TMP_Text timeText;
        public TMP_Text barBeatText;

        [Header("Viewport")]
        public RectTransform viewportBounds;
        public RectTransform headersParent;
        public RectTransform lanesParent;
        public LaneView laneView;

        [Header("Lanes")] 
        public List<TimelineLane> lanes = new();
        
        [Header("Keyframes")]
        public GameObject keyframePrefab;

        [Header("Scrollbar")]
        public Scrollbar verticalScrollbar;
        public HorizontalAdjustableScrollbar horizontalScrollbar;

        /// <summary>
        /// <para>Limit on the current scrollbar range of what's on the screen (in seconds)</para>
        /// </summary>
        [Header("Debug")]
        public Limit visibleRange;

        public const float LANE_HEIGHT = 50f;
        #endregion

        private void Start() {
            DeleteAllKeyframes();

            verticalScrollbar.onValueChanged.AddListener(SetVertical);
            horizontalScrollbar.onAnyChanged.AddListener(OnHorizontalChanged);
            currentTimeHandle.onDragDelta.AddListener(OnCurrentTimeDragged);
            
            UpdatePeekLimit();
            UpdateVerticalSlider();
            OnHorizontalChanged();

            UpdateCurrentTimeCursor();
        }

        private void Update() {
            if (InputManager.IsEditingOnInputField()) return;

            if (transform.hasChanged) {
                transform.hasChanged = false;
                OnResized();
            }
        }

        /// <summary>
        /// Call this whenever you want to seek to a specific time
        /// </summary>
        public void Seek(float time, bool stuckCursorToCenter = false) {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (time == cursorTime) return;

            cursorTime = time;

            // TODO: handle negative time better
            var bpmInfo = references.manager.project.GetBPMAtTime(time);
            var timeSinceBpmChange = time - bpmInfo.time;
            var bpm = bpmInfo.bpm;
            var beat = (int)((bpm * timeSinceBpmChange) / 60f);
            var localBeat = (int)(beat % bpmInfo.divisionNumerator) + 1;
            var bar = (int)(beat / bpmInfo.divisionNumerator) + 1;

            barBeatText.text = $"{bar}│{localBeat}";

            timeText.text = FormatTime(time);
            UpdateCurrentTimeCursor();
            if (stuckCursorToCenter) {
                CenterViewToCursor();
            }
        }

        private void CenterViewToCursor() {
            horizontalScrollbar.value = (cursorTime - peekLimit.min - (visibleRange.length / 2f)) / (peekLimit.length - visibleRange.length);
        }
        
        #region Keyframes
        public T CreatePredefinedKeyframe<T, T1>(float toTime, T1 lane) where T : Keyframe where T1 : TimelineLane {
            var keyframe = Instantiate(keyframePrefab, lane.keyframesParent).AddComponent<T>();
            keyframe.lane = lane;
            keyframe.SetTime(toTime);
            return keyframe;
        }

        // TODO: Call this before loading a new project
        public void DeleteAllKeyframes() {
            foreach (var lane in lanes) {
                foreach (var keyframe in lane.keyframes) {
                    Destroy(keyframe.gameObject);
                }
                lane.keyframes.Clear();
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Gets called when the dynamic panel is resized OR from Update when the RectTransform changes size (eg. when the window resolution changes)
        /// </summary>
        public void OnResized() {
            UpdateVerticalSlider();
            OnHorizontalChanged();

            UpdateCurrentTimeCursor();
        }
        
        private void OnHorizontalChanged() {
            visibleRange.min = Mathf.Lerp(peekLimit.min, peekLimit.max, horizontalScrollbar.minRange);
            visibleRange.max = Mathf.Lerp(peekLimit.min, peekLimit.max, horizontalScrollbar.maxRange);

            var keyframes = GetComponentsInChildren<Keyframe>();// TODO: Use lists instead of GetComponentsInChildren
            foreach (var keyframe in keyframes) {
                var x = GetX(keyframe.time);
                keyframe.SetX(x);
            }

            UpdateCurrentTimeCursor();
            //UpdateLanesParent();
        }

        private void OnCurrentTimeDragged(Vector2 delta) {
            var deltaSeconds = delta.x * (visibleRange.length / viewportBounds.rect.width);
            cursorTime += deltaSeconds;
            cursorTime = Mathf.Clamp(cursorTime, peekLimit.min, peekLimit.max);
            UpdateCurrentTimeCursor();
        }
        #endregion

        #region Updates
        public void UpdateLanesParent() {
            // My brain isn't mathing btw
            var posSeconds = -visibleRange.min;
            var x = posSeconds * (viewportBounds.rect.width / visibleRange.length);
            lanesParent.anchoredPosition = new Vector2(x, lanesParent.anchoredPosition.y);
            
            // I'm sure this will cause problems.. uhm
        }

        public void UpdateCurrentTimeCursor() {
            var pos = currentTimeHandle.rectTransform.anchoredPosition;
            var posSeconds = cursorTime - visibleRange.min;
            pos.x = posSeconds * (lanesParent.rect.width / visibleRange.length);

            SetCursorsX(pos.x);
        }

        private void UpdateVerticalSlider() {
            if (lanesParent.rect.height < viewportBounds.rect.height) {
                verticalScrollbar.size = 1f;
                verticalScrollbar.SetValueWithoutNotify(0);

                return;
            }

            verticalScrollbar.size = viewportBounds.rect.height / lanesParent.rect.height;
            verticalScrollbar.SetValueWithoutNotify(Mathf.InverseLerp(0, lanesParent.rect.height - viewportBounds.rect.height, lanesParent.anchoredPosition.y));
        }

        // TODO: This will probably be called whenever a music is loaded
        public void UpdatePeekLimit() {
            peekLimit.min = -5f;
            peekLimit.max = references.music.clip.length + 5f;
        }
        #endregion

        #region Accessors
        /// <summary>
        /// Get the X in pixels based on time
        /// </summary>
        public float GetX(float time) {
            return time * (lanesParent.rect.width / visibleRange.length);
        }

        /// <summary>
        /// Get time based on pixels
        /// </summary>
        public float GetTime(float x) {
            return x * visibleRange.length / lanesParent.rect.width;
        }

        private void SetVertical(float value) {
            var pos = lanesParent.anchoredPosition;
            pos.y = Mathf.Lerp(0, lanesParent.rect.height - viewportBounds.rect.height, value);
            lanesParent.anchoredPosition = pos;

            pos.x = headersParent.anchoredPosition.x;
            headersParent.anchoredPosition = pos;
        }

        private void SetCursorsX(float to) {
            foreach (var cursor in currentTimeCursors) {
                var pos = cursor.anchoredPosition;
                pos.x = to;
                cursor.anchoredPosition = pos;
            }
        }
        #endregion
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