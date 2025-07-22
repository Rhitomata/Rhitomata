using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Arphros.Interface
{
    public class Toast : MonoBehaviour
    {
        public static Toast Instance { get; private set; }

        public RectTransform toastArea;
        public GameObject toastPrefab;

        public float offset = -1000;
        public float textColorSpeed = 1;
        public float moveSpeed = 5;
        public List<ToastInstance> instances = new List<ToastInstance>();

        void Awake() => Instance = this;
        void OnDestroy() => Instance = null;

        void Update()
        {
            for (int i = 0; i < instances.Count; i++)
            {
                if (!instances[i].Update())
                {
                    instances.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Shows a toast for 3 seconds
        /// </summary>
        public static void Show(string message) => Show(message, 3, Color.white, Color.white);
        /// <summary>
        /// Shows a toast with a specific delay
        /// </summary>
        public static void Show(string message, float delay) => Show(message, delay, Color.white, Color.white);
        /// <summary>
        /// Shows a toast with a specific delay and custom text color
        /// </summary>
        public static void Show(string message, float delay, Color color) => Show(message, delay, color, color);
        /// <summary>
        /// Shows a toast with a specific delay and custom text color animation
        /// </summary>
        public static void Show(string message, float delay, Color startColor, Color endColor)
        {
            if (!Instance) {
                Debug.LogWarning("Toast Manager not initialized/destroyed");
                return;
            }

            var instance = new ToastInstance(
                Instantiate(Instance.toastPrefab, Instance.toastArea),
                message,
                delay,
                startColor,
                endColor
            );
            Instance.instances.Add(instance);
        }
    }


    public class ToastInstance
    {
        GameObject source;
        RectTransform rectTr;
        CanvasGroup canvasGroup;
        TMP_Text textSrc;

        public string text
        {
            get => textSrc.text;
            set => textSrc.text = value;
        }
        public float timer;
        Color endColor;

        public ToastInstance(GameObject source, string text, float delay) : this(source, text, delay, Color.white, Color.white) { }
        public ToastInstance(GameObject source, string text, float delay, Color startColor, Color endColor)
        {
            this.source = source;
            timer = delay;
            rectTr = source.GetComponent<RectTransform>();
            rectTr.anchoredPosition = new Vector2(0, Toast.Instance.offset);
            canvasGroup = source.GetComponent<CanvasGroup>();
            textSrc = source.GetComponentInChildren<TMP_Text>();
            textSrc.color = startColor;
            this.endColor = endColor;
            this.text = text;
        }

        public bool Update()
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                Object.DestroyImmediate(source);
                return false;
            }

            rectTr.anchoredPosition = Vector2.Lerp(rectTr.anchoredPosition, Vector2.zero, Toast.Instance.moveSpeed * Time.deltaTime);
            textSrc.color = Color.Lerp(textSrc.color, endColor, Toast.Instance.textColorSpeed * Time.deltaTime);
            if (timer < 1)
                canvasGroup.alpha = Mathf.Clamp(timer, 0, 1);
            return true;
        }
    }
}