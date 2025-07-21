using DynamicPanels;
using UnityEngine;

namespace Rhitomata {
    public class GameUI : MonoBehaviour {
        [Header("References")]
        public References references;
        public DynamicPanelsCanvas dynamicCanvas;

        [Header("Timeline")]
        public RectTransform timelinePanel;
        public Vector2 timelineMinSize = new(200f, 300f);
        [HideInInspector] public Panel dynamicTimelinePanel;
        [HideInInspector] public PanelTab dynamicTimelinePanelTab;

        [Header("Sprites")]
        public RectTransform spritePanel;
        public Vector2 spriteMinSize = new(200f, 200f);
        [HideInInspector] public Panel dynamicSpritePanel;
        [HideInInspector] public PanelTab dynamicSpritePanelTab;

        private void Awake() {
            // Sprite
            dynamicSpritePanel = PanelUtils.CreatePanelFor(spritePanel, dynamicCanvas);
            dynamicSpritePanel.DockToRoot(Direction.Right);
            dynamicSpritePanelTab = PanelUtils.GetAssociatedTab(spritePanel);
            dynamicSpritePanelTab.MinSize = spriteMinSize;
            dynamicSpritePanelTab.Icon = null;
            dynamicSpritePanelTab.Label = "Sprites";

            // Timeline
            dynamicTimelinePanel = PanelUtils.CreatePanelFor(timelinePanel, dynamicCanvas);
            dynamicTimelinePanel.OnResized.AddListener(OnTimelinePanelResized);
            dynamicTimelinePanel.DockToRoot(Direction.Bottom);
            dynamicTimelinePanelTab = PanelUtils.GetAssociatedTab(timelinePanel);
            dynamicTimelinePanelTab.MinSize = timelineMinSize;
            dynamicTimelinePanelTab.Icon = null;
            dynamicTimelinePanelTab.Label = "Timeline";
        }

        private void OnTimelinePanelResized() {
            // TODO: Put the actual timeline view here and also adjust all the UI elements when resizing
        }
    }
}
