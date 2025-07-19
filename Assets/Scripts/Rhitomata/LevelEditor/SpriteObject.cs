using UnityEngine;

namespace Rhitomata
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteObject : MonoBehaviour, ISelectable
    {
        public bool selected = false;
        public Color baseColor = Color.white;

        private SpriteRenderer spriteRenderer;
        private bool isHovered = false;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            ApplyColor();
        }

        public void Initialize(SpriteUI spriteUI) {
            spriteRenderer.sprite = spriteUI.GetSprite();
        }

        #region Selection

        public bool IsSelected() => selected;

        public bool OnSelect()
        {
            selected = true;
            ApplyColor();
            return true;
        }

        public bool OnDeselect()
        {
            selected = false;
            ApplyColor();
            return true;
        }

        public void OnEnter()
        {
            isHovered = true;
            ApplyColor();
        }

        public void OnExit()
        {
            isHovered = false;
            ApplyColor();
        }

        private void ApplyColor()
        {
            if (selected)
            {
                spriteRenderer.color = new Color(1f, 0.5f, 0f); // Orange
            }
            else if (isHovered)
            {
                Color blueTint = Color.Lerp(baseColor, Color.cyan, 0.3f);
                spriteRenderer.color = blueTint;
            }
            else
            {
                spriteRenderer.color = baseColor;
            }
        }

        #endregion Selection
    }
}