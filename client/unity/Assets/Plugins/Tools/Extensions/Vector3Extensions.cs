using UnityEngine;

namespace Wtf
{
	public static class Vector3Extensions
	{
		public static Vector3 ToXzVector3(this Vector3 self)
		{
			return new Vector3(self.x, 0f, self.z);
		}

		public static Vector2 ToXzVector2(this Vector3 self)
		{
			return new Vector2(self.x, self.z);
		}

		public static float XzDistanceTo(this Vector3 self, Vector3 to)
		{
			return Vector2.Distance(self.ToXzVector2(), to.ToXzVector2());
		}

		public static Vector3 XzVector3DirectionTo(this Vector3 self, Vector3 to)
		{
			return (to - self).ToXzVector3().normalized;
		}

		public static float SignedAngle(this Vector3 self, Vector3 to)
		{
			Vector3 rhs = Vector3.Cross(Vector3.up, self);
			float num = Vector3.Angle(to, self);
			return Mathf.Sign(Vector3.Dot(to, rhs)) * num;
		}
	}
}