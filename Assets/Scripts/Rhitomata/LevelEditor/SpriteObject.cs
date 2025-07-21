using UnityEngine;

namespace Rhitomata {
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteObject : MonoBehaviour, ISelectable {
        public bool selected = false;
        public Color baseColor = Color.white;

        private BoxCollider m_boxCollider;
        private BoxCollider boxCollider => m_boxCollider ?? GetComponent<BoxCollider>();
        private SpriteRenderer _spriteRenderer;
        private bool _isHovered = false;

        private void Awake() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            ApplyColor();
        }

        public void Initialize(SpriteUI spriteUI) {
            _spriteRenderer.sprite = spriteUI.GetSprite();
            var bounds = _spriteRenderer.sprite.bounds.size;
            bounds.z = 0.2f;
            boxCollider.size = bounds;
        }

        #region Selection

        public bool IsSelected() => selected;

        public bool OnSelect() {
            selected = true;
            ApplyColor();
            return true;
        }

        public bool OnDeselect() {
            selected = false;
            ApplyColor();
            return true;
        }

        public void OnEnter() {
            _isHovered = true;
            ApplyColor();
        }

        public void OnExit() {
            _isHovered = false;
            ApplyColor();
        }

        private void ApplyColor() {
            if (selected) {
                _spriteRenderer.color = new Color(1f, 0.5f, 0f); // Orange
            } else if (_isHovered) {
                Color blueTint = Color.Lerp(baseColor, Color.cyan, 0.3f);
                _spriteRenderer.color = blueTint;
            } else {
                _spriteRenderer.color = baseColor;
            }
        }

        #endregion Selection
    }
}