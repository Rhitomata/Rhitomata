using Rhitomata.Data;
using UnityEngine;

namespace Rhitomata {
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteObject : ObjectSerializer<SpriteData>, ISelectable {
        public bool selected = false;
        public Color baseColor = Color.white;

        private BoxCollider _boxCollider;
        private BoxCollider boxCollider => _boxCollider ?? GetComponent<BoxCollider>();
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer spriteRenderer => _spriteRenderer ?? GetComponent<SpriteRenderer>();
        private bool _isHovered = false;

        private void Awake() {
            ApplyColor();
        }

        public void Initialize(Sprite sprite, int id = -1) {
            spriteRenderer.sprite = sprite;
            var bounds = spriteRenderer.sprite.bounds.size;
            bounds.z = 0.2f;
            boxCollider.size = bounds;

            LevelManager.Register(id, this);
        }

        public void Delete() {
            LevelManager.Remove(this);
            Destroy(gameObject);
        }

        protected override void Deserialize(SpriteData data) {
            var sprite = SpriteManager.GetSprite(data.spriteId);
            transform.localPosition = data.position;
            transform.eulerAngles = data.eulerAngles;
            transform.localScale = data.scale;
            spriteRenderer.color = data.color;
            spriteRenderer.sortingOrder = data.sortingOrder;
            
            Initialize(sprite, data.id);
        }

        protected override SpriteData Serialize() {
            return new() {
                id = instanceId,
                name = name,
                position = transform.localPosition,
                eulerAngles = transform.eulerAngles,
                scale = transform.localScale,
                color = spriteRenderer.color,
                spriteId = SpriteManager.GetIndex(spriteRenderer.sprite),
                sortingOrder = spriteRenderer.sortingOrder
            };
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
                spriteRenderer.color = new Color(1f, 0.5f, 0f); // Orange
            } else if (_isHovered) {
                var blueTint = Color.Lerp(baseColor, Color.cyan, 0.3f);
                spriteRenderer.color = blueTint;
            } else {
                spriteRenderer.color = baseColor;
            }
        }

        #endregion Selection
    }
}