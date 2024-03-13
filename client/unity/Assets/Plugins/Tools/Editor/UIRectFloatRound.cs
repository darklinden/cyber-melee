using UnityEngine;
using UnityEditor;

namespace Wtf.Editor
{
	public class UIRectFloatRound
	{
		const string MENU_TITLE = "GameObject/RectTransform Round Floats";

		[MenuItem(MENU_TITLE, true)]
		private static bool CanDo()
		{
			if (Selection.activeObject == null) return false;
			var go = Selection.activeObject as GameObject;
			if (go == null) return false;
			var rt = go.GetComponent<RectTransform>();
			if (rt == null) return false;

			return true;
		}

		[MenuItem(MENU_TITLE, false, 10)]
		public static void RoundSelectedRect()
		{
			var go = Selection.activeObject as GameObject;
			if (go == null) return;
			var rt = go.GetComponent<RectTransform>();
			if (rt == null) return;

			var pos = rt.anchoredPosition;
			pos.x = Mathf.Round(pos.x);
			pos.y = Mathf.Round(pos.y);
			rt.anchoredPosition = pos;

			var size = rt.sizeDelta;
			size.x = Mathf.Round(size.x);
			size.y = Mathf.Round(size.y);
			rt.sizeDelta = size;
		}
	}
}