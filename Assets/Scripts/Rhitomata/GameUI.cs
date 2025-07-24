using DynamicPanels;
using TMPro;
using UnityEngine;

namespace Rhitomata {
    public class GameUI : MonoBehaviour {
        [Header("References")]
        public References references;
        public DynamicPanelsCanvas dynamicCanvas;
        
        [Header("Playmode")]
        public GameObject playmodePanel;
        public TMP_Text scoreLabel;
        public TMP_Text judgementLabel;
        public TMP_Text comboLabel;
        public Panel timingWindowPanel;

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
            var previousTimelineSize = timelinePanel.rect.size;
            dynamicTimelinePanel = PanelUtils.CreatePanelFor(timelinePanel, dynamicCanvas);
            dynamicTimelinePanel.DockToRoot(Direction.Bottom);
            dynamicTimelinePanelTab = PanelUtils.GetAssociatedTab(timelinePanel);
            dynamicTimelinePanelTab.MinSize = timelineMinSize;
            dynamicTimelinePanelTab.Icon = null;
            dynamicTimelinePanelTab.Label = "Timeline";
            dynamicTimelinePanel.ResizeTo(previousTimelineSize);
            dynamicTimelinePanel.OnResized.AddListener(references.timeline.OnResized);
        }
    }
}
