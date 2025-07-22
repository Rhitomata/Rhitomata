using UnityEngine;
using UnityEngine.EventSystems;
using Riten.Native.Cursors;

namespace DynamicPanels
{
	public class PanelCursorHandler : MonoBehaviour
	{
		private static PanelCursorHandler instance = null;

		private PanelResizeHelper activeResizeHelper;
		private PointerEventData activeEventData;

		private bool isResizing;
		private Vector2 prevPointerPos;

#pragma warning disable 0649
		[SerializeField]
		private Texture2D horizontalCursor;
		[SerializeField]
		private Texture2D verticalCursor;
		[SerializeField]
		private Texture2D diagonalCursorTopLeft;
		[SerializeField]
		private Texture2D diagonalCursorTopRight;
#pragma warning restore 0649

		private void Awake()
		{
			instance = this;
		}

		public static void OnPointerEnter( PanelResizeHelper resizeHelper, PointerEventData eventData )
		{
			if( instance == null )
				return;

			instance.activeResizeHelper = resizeHelper;
			instance.activeEventData = eventData;
		}

		public static void OnPointerExit( PanelResizeHelper resizeHelper )
		{
			if( instance == null )
				return;

			if( instance.activeResizeHelper == resizeHelper )
			{
				instance.activeResizeHelper = null;
				instance.activeEventData = null;

				if( !instance.isResizing )
					SetDefaultCursor();
			}
		}

		public static void OnBeginResize( Direction primary, Direction secondary )
		{
			if( instance == null )
				return;

			instance.isResizing = true;
			instance.UpdateCursor( primary, secondary );
		}

		public static void OnEndResize()
		{
			if( !instance )
				return;

			instance.isResizing = false;

			if( !instance.activeResizeHelper )
				SetDefaultCursor();
			else
				instance.prevPointerPos = new Vector2( -1f, -1f );
		}

		private void Update()
		{
			if( isResizing )
				return;

			if( activeResizeHelper != null )
			{
				Vector2 pointerPos = activeEventData.position;
				if( pointerPos != prevPointerPos )
				{
					if( activeEventData.dragging )
						SetDefaultCursor();
					else
					{
						Direction direction = activeResizeHelper.Direction;
						Direction secondDirection = activeResizeHelper.GetSecondDirection( activeEventData.position );
						if( activeResizeHelper.Panel.CanResizeInDirection( direction ) )
							UpdateCursor( direction, secondDirection );
						else if( secondDirection != Direction.None )
							UpdateCursor( secondDirection, Direction.None );
						else
							SetDefaultCursor();
					}

					prevPointerPos = pointerPos;
				}
			}
		}

		private static void SetDefaultCursor()
		{
			NativeCursor.ResetCursor();
		}

		private void UpdateCursor( Direction primary, Direction secondary ) {
			
			var cursorType = NTCursors.Default;
			switch (primary) {
				case Direction.Left: {
					if (secondary == Direction.Top)
						cursorType = NTCursors.ResizeDiagonalLeft;
					else if( secondary == Direction.Bottom )
						cursorType = NTCursors.ResizeDiagonalRight;
					else
						cursorType = NTCursors.ResizeHorizontal;
					break;
				}
				case Direction.Right: {
					if (secondary == Direction.Top)
						cursorType = NTCursors.ResizeDiagonalRight;
					else if (secondary == Direction.Bottom)
						cursorType = NTCursors.ResizeDiagonalLeft;
					else
						cursorType = NTCursors.ResizeHorizontal;
					break;
				}
				case Direction.Top: {
					if( secondary == Direction.Left )
						cursorType = NTCursors.ResizeDiagonalLeft;
					else if( secondary == Direction.Right )
						cursorType = NTCursors.ResizeDiagonalRight;
					else
						cursorType = NTCursors.ResizeVertical;
					break;
				}
				default: {
					if( secondary == Direction.Left )
						cursorType = NTCursors.ResizeDiagonalRight;
					else if( secondary == Direction.Right )
						cursorType = NTCursors.ResizeDiagonalLeft;
					else
						cursorType = NTCursors.ResizeVertical;
					break;
				}
			}

			NativeCursor.SetCursor(cursorType);
			Debug.Log("Cursor type attempted to change");
		}
	}
}