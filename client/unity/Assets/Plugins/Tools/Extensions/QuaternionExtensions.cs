using UnityEngine;

namespace Wtf
{
	public static class QuaternionExtensions
	{
		public static Quaternion RotateTowardsPoint(this Quaternion self, Vector3 fromPt, Vector3 toPt, float t)
		{
			Vector3 vector = toPt - fromPt;
			Quaternion to = (vector == Vector3.zero)
			 ? self
			 : Quaternion.LookRotation(vector, Vector3.up);
			return Quaternion.Lerp(self, to, t);
		}
	}
}