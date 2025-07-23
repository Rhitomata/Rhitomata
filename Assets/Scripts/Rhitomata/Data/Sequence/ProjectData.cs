using System.Collections.Generic;
using Newtonsoft.Json;
using Rhitomata.Timeline;
using UnityEngine;
using Keyframe = Rhitomata.Timeline.Keyframe;

namespace Rhitomata.Data {
    public class ProjectData {
        public string name { get; set; } = "Untitled";
        public string author { get; set; } = "Unknown";
        
        public string musicName { get; set; } = "None";
        public string musicAuthor { get; set; } = "Unknown";
        public string musicPath { get; set; } = "audio.ogg";
        
        public decimal difficulty { get; set; } = 10.0m;

        public List<ModifyPoint> points { get; set; } = new() { };
        public List<BPMInfo> bpms { get; set; } = new() { new() };
        public List<SpeedInfo> speeds { get; set; } = new() { new() };
        public List<DirectionInfo> directions { get; set; } = new() { new() };
        
        public List<SpriteData> sprites { get; set; } = new();
        public List<SpriteMetadata> spritesMetadata { get; set; } = new();
        
        [JsonIgnore] public string directoryPath { get; set; }
        [JsonIgnore] public string filePath { get; set; }

        public ProjectData() { }
        public ProjectData(string name, string author, string musicAuthor, string musicName) {
            this.name = name;
            this.author = author;
            this.musicAuthor = musicAuthor;
            this.musicName = musicName;
        }
        
        public int GetModifyPointIndexAtTime(float time) {
            if (points.Count == 0) return -1;
            for (int i = 0; i < points.Count; i++) {
                if (time < points[i].time) {
                    if (i == 0) return -1;
                    return i - 1;
                }
            }
            return points.Count - 1;
        }

        public ModifyPoint GetModifyPointAtTime(float time) {
            for (int i = 0; i < points.Count; i++) {
                if (time < points[i].time) {
                    if (i == 0) return null;
                    return points[i - 1];
                }
            }
            if (points.Count == 0) return null;
            return points[^1];
        }

        public DirectionInfo GetDirectionAtTime(float time) {
            for (int i = 0; i < directions.Count; i++) {
                if (time < directions[i].time) {
                    if (i == 0) return directions[0];
                    return directions[i - 1];
                }
            }
            return directions[^1];
        }

        public int GetDirectionIndexAtTime(float time) {
            for (int i = 0; i < directions.Count; i++) {
                if (time < directions[i].time) {
                    if (i == 0) return 0;
                    return i - 1;
                }
            }
            return directions.Count - 1;
        }

        public int GetBPMIndexAtTime(float time) {
            for (int i = 0; i < bpms.Count; i++) {
                if (time < bpms[i].time) {
                    if (i == 0) return 0;
                    return i - 1;
                }
            }
            return bpms.Count - 1;
        }

        public BPMInfo GetBPMAtTime(float time) {
            for (int i = 0; i < bpms.Count; i++) {
                if (time < bpms[i].time) {
                    if (i == 0) return bpms[0];
                    return bpms[i - 1];
                }
            }
            return bpms[^1];
        }

        public int GetSpeedIndexAtTime(float time) {
            for (int i = 0; i < speeds.Count; i++) {
                if (time < speeds[i].time) {
                    if (i == 0) return 0;
                    return i - 1;
                }
            }
            return speeds.Count - 1;
        }

        public SpeedInfo GetSpeedAtTime(float time) {
            for (int i = 0; i < speeds.Count; i++) {
                if (time < speeds[i].time) {
                    if (i == 0) return speeds[0];
                    return speeds[i - 1];
                }
            }
            return speeds[^1];
        }

        public float GetTranslationAroundTimeInefficient(float start, float end) {
            var startSpeed = GetSpeedAtTime(start);
            var speedIndex = speeds.IndexOf(startSpeed);
            var endTime = end > speeds[speedIndex + 1].time ? speeds[speedIndex + 1].time : end;

            float translation = (endTime - start) * startSpeed.speed;
            return translation + GetTranslationAroundTimeInefficient(speeds[speedIndex + 1].time, end);
        }

        // TODO: Move all of this on project manager
        #region Weird Calculations
        public float GetTranslationAroundTime(float start, float end) {
            float translation = 0f;
            float currentTime = start;

            while (currentTime < end) {
                var currentSpeed = GetSpeedAtTime(currentTime);
                int speedIndex = speeds.IndexOf(currentSpeed);

                float nextSpeedChangeTime = (speedIndex + 1 < speeds.Count) ? speeds[speedIndex + 1].time : float.MaxValue;
                float segmentEndTime = System.Math.Min(end, nextSpeedChangeTime);

                translation += (segmentEndTime - currentTime) * currentSpeed.speed;
                currentTime = segmentEndTime;
            }

            return translation;
        }

        /// <summary>
        /// Returns null if another point already exists at the exact same time, this also automatically inserts the point accordingly
        /// </summary>
        public ModifyPoint CreateItem(float time) {
            if (points.Exists(val => val.time == time))
                return null;

            ModifyPoint point = new() {
                time = time
            };

            var previousPoint = GetModifyPointAtTime(time);
            if (previousPoint != null) {
                var nextRotationIndex = previousPoint.rotationIndex + 1;
                var currentDirection = GetDirectionAtTime(previousPoint.time);
                if (nextRotationIndex >= currentDirection.directions.Count)
                    nextRotationIndex = 0;
                point.eulerAngles = currentDirection.directions[nextRotationIndex];
                point.rotationIndex = nextRotationIndex;
                point.position = previousPoint.position + (GetTranslationAroundTime(previousPoint.time, time) * previousPoint.forward);
            } else {
                var direction = GetDirectionAtTime(time);
                point.eulerAngles = direction.directions[1];
                point.rotationIndex = 1;
                point.position = GetTranslationAroundTime(0, time) * ModifyPoint.EulerDegreesToForward(directions[0].directions[0]);
            }
            points.Insert(points.IndexOf(previousPoint) + 1, point);
            return point;
        }

        public void RemoveItem(ModifyPoint point) {
            if (point != null) {
                var time = point.time;
                DestroyItem(point);
                points.Remove(point);

                var adjustedPointIndex = GetModifyPointIndexAtTime(time);
                AdjustAllPointFromIndex(adjustedPointIndex);
            }
        }

        public void DestroyItem(ModifyPoint point) {
            if (point != null) {
                Object.Destroy(point.keyframe.gameObject);
                Object.Destroy(point.indicator.gameObject);
                Object.Destroy(point.tail.gameObject);
            }
        }

        /// <summary>
        /// Adjusts all points after the given index
        /// </summary>
        public void AdjustAllPointFromIndex(int index) {
            if (points.Count == 0) return;

            var startIndex = index;
            if (index == -1) {
                AdjustAllPointFromIndex(0);
                return;
            }

            if (index == 0) {
                var point = points[index];
                var direction = GetDirectionAtTime(point.time);
                point.eulerAngles = direction.directions[1];
                point.rotationIndex = 1;
                point.position = GetTranslationAroundTime(0, point.time) * ModifyPoint.EulerDegreesToForward(directions[0].directions[0]);
                point.tail.transform.localPosition = point.position;
                point.indicator.transform.localPosition = point.position;
                if (point.hasPassed)
                    point.tail.AdjustStretch(Vector3.zero, point.position);

                if (points.Count > 1)
                    AdjustAllPointFromIndex(1);
            } else {
                var previousPoint = points[startIndex - 1];
                for (int i = startIndex; i < points.Count; i++) {
                    var currentPoint = points[i];
                    var nextRotationIndex = previousPoint.rotationIndex + 1;
                    var currentDirection = GetDirectionAtTime(previousPoint.time);
                    if (nextRotationIndex >= currentDirection.directions.Count)
                        nextRotationIndex = 0;
                    currentPoint.eulerAngles = currentDirection.directions[nextRotationIndex];
                    currentPoint.rotationIndex = nextRotationIndex;
                    currentPoint.position = previousPoint.position + (GetTranslationAroundTime(previousPoint.time, currentPoint.time) * previousPoint.forward);
                    currentPoint.tail.transform.localPosition = currentPoint.position;
                    currentPoint.indicator.transform.localPosition = currentPoint.position;
                    if (previousPoint.hasPassed)
                        currentPoint.tail.AdjustStretch(previousPoint.position, currentPoint.position);
                    previousPoint = currentPoint;
                }
            }
        }

        /// <summary>
        /// Adjusts all points after the given point
        /// </summary>
        public void AdjustAllPointFromPoint(ModifyPoint point) => AdjustAllPointFromIndex(points.IndexOf(point));

        public Vector3 GetPositionForTime(float time) {
            var point = GetModifyPointAtTime(time);
            if (point == null)
                return GetTranslationAroundTime(0, time) * ModifyPoint.EulerDegreesToForward(directions[0].directions[0]);
            return point.position + (GetTranslationAroundTime(point.time, time) * point.forward);
        }
        #endregion
    }

    [System.Serializable]
    public class ModifyPoint {
        public float time { get; set; }
        public Vector3 position { get; set; }

        [JsonIgnore]
        private Vector3 _rotation { get; set; } = new Vector3(0, 0, 0);
        [JsonProperty("rotation")]
        public Vector3 eulerAngles {
            get => _rotation;
            set {
                _rotation = value;
                forward = EulerDegreesToForward(_rotation);
            }
        }

        public int rotationIndex;

        [JsonIgnore] public Keyframe keyframe;
        [JsonIgnore] public Vector3 forward { get; private set; } = new Vector3(0, 0, 1f);
        [JsonIgnore] public Tail tail { get; set; }
        [JsonIgnore] public Indicator indicator { get; set; }
        [JsonIgnore] public bool isInstantiated;
        [JsonIgnore] public bool hasPassed;

        public static Vector3 EulerDegreesToForward(Vector3 rotation) {
            return Quaternion.Euler(rotation) * Vector3.forward;
        }
    }
}