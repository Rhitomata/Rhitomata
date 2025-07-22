using System.Collections.Generic;
using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.UI;
using Rhitomata.Data;

namespace Rhitomata {
    public class SpriteManager : InstanceManager<SpriteItem> {
        [Header("Layout")]
        public RectTransform spritePanel;
        public GridLayoutGroup spriteGridLayout;
        public float xMinSize = 100f;

        private Vector2 _previousPanelSize;

        [Header("Instancing")]
        public Transform spriteUIParent;
        public GameObject spriteUIPrefab;
        public List<SpriteItem> placeholders = new();

        private void Start() {
            foreach (var item in placeholders) {
                item.Delete();
            }
            Clear();
        }

        public void BrowseForSprite() {
            var extensions = new[] {
                new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
                new ExtensionFilter("All Files", "*"),
            };
            var result =
                StandaloneFileBrowser.OpenFilePanel("Import sprites", ProjectList.projectsDir, extensions, true);
            if (result == null || result.Length == 0) return; // Cancelled

            foreach (var path in result) {
                ImportSprite(path);
            }
        }

        public void RefreshSpriteItems() {

        }

        public static void Clear() {
            foreach (var item in objects) {
                item.Delete();
            }
        }

        private void ImportSprite(string path) {
            var sprite = CreateSpriteFromPath(path);
            var data = new SpriteMetadata() {
                id = lastInstanceId + 1,
                path = string.IsNullOrWhiteSpace(References.Instance.manager.project.directoryPath) ? 
                    path :
                    Useful.GetRelativePath(path, References.Instance.manager.project.directoryPath)
            };
            CreateSpriteItem(sprite, data);
        }

        private SpriteItem CreateSpriteItem(Sprite sprite, SpriteMetadata metadata) {
            var item = Instantiate(spriteUIPrefab, spriteUIParent).GetComponent<SpriteItem>();
            item.Initialize(sprite, metadata);
            return item;
        }
        
        public static Sprite GetSprite(int id) => objects[id].sprite;
        public static int GetIndex(Sprite sprite) => objects.FindIndex(val => val.sprite == sprite);

        private static Sprite CreateSpriteFromPath(string path) {
            var fileData = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2) {
                name = Path.GetFileName(path)
            };

            if (texture.LoadImage(fileData)) {
                var rect = new Rect(0, 0, texture.width, texture.height);
                var pivot = new Vector2(0.5f, 0.5f);
                var sprite = Sprite.Create(texture, rect, pivot);
                sprite.name = texture.name;
                return sprite;
            }

            Debug.LogError("Failed to create sprite!");
            return null;
        }

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