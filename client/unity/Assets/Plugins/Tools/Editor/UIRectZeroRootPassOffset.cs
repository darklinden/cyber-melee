using UnityEngine;
using UnityEditor;

namespace Wtf.Editor
{
	public class UIRectZeroRootPassOffset
	{
		const string MENU_TITLE = "GameObject/RectTransform Zero Root Pass Offset";

		[MenuItem(MENU_TITLE, true)]
		private static bool CanDo()
		{
			if (Selection.activeObject == null) return false;
			var go = Selection.activeObject as GameObject;
			if (go == null) return false;
			go.TryGetComponent<RectTransform>(out var rt);
			if (rt == null) return false;

			return true;
		}

		[MenuItem(MENU_TITLE, false, 10)]
		public static void ZeroSelectedRect()
		{
			var go = Selection.activeObject as GameObject;
			if (go == null) return;
			var rt = go.GetComponent<RectTransform>();
			if (rt == null) return;

			var pos = rt.anchoredPosition;
			var offsetX = pos.x;
			var offsetY = pos.y;
			if (pos.x == 0f && pos.y == 0f)
			{
				return;
			}

			var childCount = go.transform.childCount;
			for (var i = 0; i < childCount; i++)
			{
				var child = go.transform.GetChild(i);
				var crt = child.GetComponent<RectTransform>();
				var childPos = crt.anchoredPosition;
				childPos.x += offsetX;
				childPos.y += offsetY;
				crt.anchoredPosition = childPos;
			}

			rt.anchoredPosition = Vector2.zero;
		}
	}
}