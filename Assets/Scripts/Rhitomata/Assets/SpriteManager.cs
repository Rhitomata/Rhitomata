using UnityEngine;
using UnityEngine.UI;

namespace Rhitomata.Assets {
    public class SpriteManager : InstanceManager<SpriteUI> {
        public RectTransform spritePanel;
        public GridLayoutGroup spriteGridLayout;
        public float xMinSize = 100f;

        private Vector2 _previousPanelSize;

        private void Update() {
            var currentSize = spritePanel.rect.size;
            if (currentSize == _previousPanelSize) return;

            AdjustCellSize(currentSize.x);
            _previousPanelSize = currentSize;
        }

        private void AdjustCellSize(float panelWidth) {
            var spacingX = spriteGridLayout.spacing.x;
            var padding = spriteGridLayout.padding.left + spriteGridLayout.padding.right;

            var availableWidth = panelWidth - padding;
            var cellsPerRow = Mathf.FloorToInt((availableWidth + spacingX) / (xMinSize + spacingX));
            cellsPerRow = Mathf.Max(1, cellsPerRow);

            var cellWidth = (availableWidth - (spacingX * (cellsPerRow - 1))) / cellsPerRow;
            var cellHeight = spriteGridLayout.cellSize.y;

            spriteGridLayout.cellSize = new Vector2(cellWidth, cellHeight);
        }
    }
}
