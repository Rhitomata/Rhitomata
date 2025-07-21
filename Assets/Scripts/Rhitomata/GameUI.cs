using System;
using DynamicPanels;
using UnityEngine;

namespace Rhitomata
{
    public class GameUI : MonoBehaviour
    {
        public RectTransform timelinePanel;
        public DynamicPanelsCanvas dynamicCanvas;
        [HideInInspector]
        public Panel dynamicTimelinePanel;
        public Vector2 dynamicTimelineMinSize = new Vector2(200f, 300f);

        private void Awake()
        {
            dynamicTimelinePanel = PanelUtils.CreatePanelFor(timelinePanel, dynamicCanvas);
            dynamicTimelinePanel.DockToRoot(Direction.Bottom);
            
            var panelTab = PanelUtils.GetAssociatedTab(timelinePanel);
            panelTab.MinSize = dynamicTimelineMinSize;
            dynamicTimelinePanel.OnResized.AddListener(OnTimelinePanelResized);
        }

        private void OnTimelinePanelResized()
        {
            // TODO: Put the actual timeline view here and also adjust all the UI elements when resizing
        }
    }
}
