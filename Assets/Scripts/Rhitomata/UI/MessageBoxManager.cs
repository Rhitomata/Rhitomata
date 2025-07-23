using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

namespace Rhitomata.UI
{
    public class MessageBoxManager : MonoBehaviour
    {
        public static MessageBoxManager self;
        private Tween _tween;
        private bool _tryClose;

        public List<MessageBoxInstance> instances = new();
        public GameObject messageBoxPrefab;
        public GameObject messageButtonPrefab;
        public Transform messageBoxParent;
        public GameObject blockPanel;

        private void Awake()
        {
            self = this;
        }

        private void OnDestroy()
        {
            if (self == this) self = null;
        }

        public void RemoveInstance(MessageBoxInstance instance)
        {
            if (instances.Contains(instance))
                instances.Remove(instance);

            if (!instances.Exists(val => val.blockInteraction))
                Unblock();
            else
                Block();
        }

        /// <summary>
        /// Create a message box that appears from the center of the screen
        /// </summary>
        /// <param name="message">The text in the center</param>
        /// <param name="title">The text in the top bar</param>
        /// <param name="icon">The icon of the message box (UNSUPPORTED)</param>
        /// <param name="buttons">The buttons with actions</param>
        /// <param name="showCloseButton">Is the dialog can be closed? Useful for loading message boxes</param>
        /// <returns>The instance of the message box</returns>
        public MessageBoxInstance CreateMessageBox(string message, string title = "Info", Sprite icon = null, MessageBoxButton[] buttons = null, bool showCloseButton = true, bool blockInteraction = true)
        {
            var messageBox = Instantiate(messageBoxPrefab, messageBoxParent);
            var window = messageBox.GetComponent<Window>();
            var topBar = messageBox.transform.Find("Top");
            topBar.Find("Title").GetComponent<TMP_Text>().text = title;
            topBar.Find("Close").GetComponent<Button>().onClick.AddListener(() => {
                window.onPostHide += () => Destroy(messageBox);
                window.Hide();
            });
            topBar.Find("Close").gameObject.SetActive(showCloseButton);

            var content = messageBox.transform.Find("Content").Find("Content Wrapper");
            content.Find("Text").GetComponent<TMP_Text>().text = message;

            var buttonContainer = messageBox.transform.Find("Content").Find("Buttons");

            if (buttons == null && showCloseButton)
                buttons = new[] { new MessageBoxButton("OK", null, true) };

            if (buttons == null)
                buttons = new MessageBoxButton[] { };

            var butts = new List<MessageBoxInstanceButton>();
            foreach (var button in buttons)
            {
                var butt = Instantiate(messageButtonPrefab, buttonContainer);
                var i = button;
                var b = butt.GetComponent<Button>();
                b.onClick.AddListener(() => i.onClick?.Invoke());
                if (i.close)
                {
                    b.onClick.AddListener(() => {
                        window.onPostHide += () => Destroy(messageBox);
                        window.Hide();
                    });
                }
                butt.transform.Find("Text").GetComponent<TMP_Text>().text = i.text;
                butts.Add(new MessageBoxInstanceButton(ref i, b));
            }
            var instance = new MessageBoxInstance(messageBox);
            instance.blockInteraction = blockInteraction;
            if (blockInteraction)
                Block();
            window.Show();
            return instance;
        }

        public void Block()
        {
            if (blockPanel.activeSelf) return;

            _tryClose = false;
            _tween?.Kill();

            blockPanel.SetActive(true);
            _tween = blockPanel.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).SetEase(Ease.InOutQuad);
        }

        public void Unblock()
        {
            if (!blockPanel.activeSelf) return;
            if (_tryClose) return;

            _tryClose = true;
            _tween?.Kill();

            _tween = blockPanel.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).SetEase(Ease.InOutQuad).OnComplete(() => {
                blockPanel.SetActive(false);
                _tryClose = false;
            });
        }
    }

    /// <summary>
    /// The instance of a message box
    /// </summary>
    public class MessageBoxInstance
    {
        public GameObject gameObject;
        string _message;
        public string message
        {
            get => _message;
            set
            {
                _message = value;
                _messageText.text = value;
            }
        }
        string _title;
        public string title
        {
            get => _title;
            set
            {
                _title = value;
                _titleText.text = value;
            }
        }
        public bool showCloseButton
        {
            get => _closeButton.gameObject.activeSelf;
            set => _closeButton.gameObject.SetActive(value);
        }
        public bool blockInteraction;
        public List<MessageBoxInstanceButton> buttons = new List<MessageBoxInstanceButton>();

        Transform _topBar;
        TextMeshProUGUI _titleText;
        Button _closeButton;
        Transform _content;
        TextMeshProUGUI _messageText;
        Transform _buttonContainer;
        Window _window;

        public MessageBoxInstance(GameObject gameObject)
        {
            this.gameObject = gameObject;
            _topBar = gameObject.transform.Find("Top");
            _window = gameObject.GetComponent<Window>();
            _titleText = _topBar.Find("Title").GetComponent<TextMeshProUGUI>();
            _title = _titleText.text;
            _closeButton = _topBar.Find("Close").GetComponent<Button>();
            _content = gameObject.transform.Find("Content").Find("Content Wrapper");
            _messageText = _content.Find("Text").GetComponent<TextMeshProUGUI>();
            _message = _messageText.text;
            _buttonContainer = gameObject.transform.Find("Content").Find("Buttons");

            _window.onHide += () => MessageBoxManager.self.RemoveInstance(this);
            _window.onDestroy += () =>
            {
                if (MessageBoxManager.self)
                    MessageBoxManager.self.RemoveInstance(this);
            };
        }

        public void AddButton(MessageBoxButton info)
        {
            var obj = Object.Instantiate(MessageBoxManager.self.messageButtonPrefab, _buttonContainer);
            var button = obj.GetComponent<Button>();
            button.onClick.AddListener(() => info.onClick?.Invoke());

            if (info.close)
                button.onClick.AddListener(() => Object.Destroy(gameObject));

            obj.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = info.text;
            buttons.Add(new MessageBoxInstanceButton(ref info, button));
        }

        public void Close(bool destroy = true)
        {
            if (destroy)
                _window.onPostHide += () => Object.Destroy(gameObject);
            _window.Hide();
        }
    }

    /// <summary>
    /// A wrapper class for MessageBoxManager to make it similar with System.Windows.Forms
    /// </summary>
    public static class MessageBox
    {
        /// <summary>
        /// Create a message box that appears from the center of the screen
        /// </summary>
        /// <param name="message">The text in the center</param>
        /// <param name="title">The text in the top bar</param>
        /// <param name="icon">The icon of the message box (UNSUPPORTED)</param>
        /// <param name="buttons">The buttons with actions</param>
        /// <param name="showCloseButton">Is the dialog can be closed? Useful for loading message boxes</param>
        /// <returns>The instance of the message box</returns>
        public static MessageBoxInstance Show(string message, string title = "Info", Sprite icon = null, MessageBoxButton[] buttons = null, bool showCloseButton = true, bool blockInteraction = true)
        {
            if (MessageBoxManager.self != null)
            {
                var messageBox = MessageBoxManager.self.CreateMessageBox(message, title, icon, buttons, showCloseButton, blockInteraction);
                if (messageBox.buttons.Count > 0) {
                    messageBox.buttons[0].Select();
                }
                return messageBox;
            }
            return null;
        }

        public static MessageBoxInstance Show(string message)
        {
            if (MessageBoxManager.self != null)
            {
                var messageBox = MessageBoxManager.self.CreateMessageBox(message, Application.productName, null, new MessageBoxButton[] { new MessageBoxButton("OK", null, true) }, true);
                if (messageBox.buttons.Count > 0) {
                    messageBox.buttons[0].Select();
                }
                return messageBox;
            }
            return null;
        }

        public static MessageBoxInstance ShowError(string message)
        {
            if (MessageBoxManager.self != null)
            {
                var messageBox = MessageBoxManager.self.CreateMessageBox(message, Application.productName + ": Error", null, new MessageBoxButton[] { new MessageBoxButton("OK", null, true) }, true);
                if (messageBox.buttons.Count > 0) {
                    messageBox.buttons[0].Select();
                }
                return messageBox;
            }
            return null;
        }
    }

    public class MessageBoxInstanceButton
    {
        Button button;
        MessageBoxButton info;

        string _text;
        public string text
        {
            get => _text;
            set
            {
                _text = value;
                _buttonText.text = value;
            }
        }
        private bool _close;
        public bool close
        {
            get => _close;
            set
            {
                _close = value;
                info.close = value;
            }
        }
        public System.Action onClick
        {
            get => info.onClick;
            set => info.onClick = value;
        }

        TextMeshProUGUI _buttonText;

        public MessageBoxInstanceButton(ref MessageBoxButton info, Button button)
        {
            this.info = info;
            this.button = button;
            _buttonText = button.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _text = _buttonText.text;
            _close = info.close;
        }

        public void Select() {
            button.Select();
        }
    }

    public class MessageBoxButton
    {
        public string text;
        public bool close;
        public System.Action onClick;

        public MessageBoxButton() { }
        /// <summary>
        /// A button for MessageBox
        /// </summary>
        /// <param name="text">The text inside the button</param>
        /// <param name="onClick">An action that happens when the button is clicked</param>
        /// <param name="close">Close the message box on click</param>
        public MessageBoxButton(string text, System.Action onClick, bool close = true)
        {
            this.text = text;
            this.onClick = onClick;
            this.close = close;
        }
    }
}