using UnityEngine;

namespace Wtf
{
	public static class Vector2Extensions
	{
		public static Vector2 Rotate(this Vector2 v, float degrees)
		{
			return Quaternion.Euler(0f, 0f, degrees) * v;
		}
	}
}