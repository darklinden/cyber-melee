using System.Collections.Generic;
using UnityEngine;

namespace Wtf
{
	public static class AnimationCurveExtensions
	{
		public static float GetLowestKeyframeValue(this AnimationCurve self)
		{
			float num = float.PositiveInfinity;
			Keyframe[] keys = self.keys;
			for (int i = 0; i < keys.Length; i++)
			{
				Keyframe keyframe = keys[i];
				if (num == float.PositiveInfinity || keyframe.value < num)
				{
					num = keyframe.value;
				}
			}
			return num;
		}

		public static float GetHighestKeyframeValue(this AnimationCurve self)
		{
			float num = float.PositiveInfinity;
			Keyframe[] keys = self.keys;
			for (int i = 0; i < keys.Length; i++)
			{
				Keyframe keyframe = keys[i];
				if (num == float.PositiveInfinity || keyframe.value > num)
				{
					num = keyframe.value;
				}
			}
			return num;
		}

		public static float GetLowestValue(this AnimationCurve self, int steps = 10)
		{
			float num = self.GetTimeAmplitude() / (float)steps;
			float num2 = self.Evaluate(0f);
			for (int i = 1; i <= steps; i++)
			{
				float num3 = self.Evaluate((float)i * num);
				if (num3 < num2)
				{
					num2 = num3;
				}
			}
			return num2;
		}

		public static float GetHighestValue(this AnimationCurve self, int steps = 10)
		{
			float num = self.GetTimeAmplitude() / (float)steps;
			float num2 = self.Evaluate(0f);
			for (int i = 1; i <= steps; i++)
			{
				float num3 = self.Evaluate((float)i * num);
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
			return num2;
		}

		public static float GetKeyframeValueAmplitude(this AnimationCurve self)
		{
			return self.GetHighestKeyframeValue() - self.GetLowestKeyframeValue();
		}

		public static float GetValueAmplitude(this AnimationCurve self, int steps = 10)
		{
			return self.GetHighestValue(steps) - self.GetLowestValue(steps);
		}

		public static float GetLowestTime(this AnimationCurve self)
		{
			float num = float.PositiveInfinity;
			Keyframe[] keys = self.keys;
			for (int i = 0; i < keys.Length; i++)
			{
				Keyframe keyframe = keys[i];
				if (num == float.PositiveInfinity || keyframe.time < num)
				{
					num = keyframe.time;
				}
			}
			return num;
		}

		public static float GetHighestTime(this AnimationCurve self)
		{
			float num = float.NegativeInfinity;
			Keyframe[] keys = self.keys;
			for (int i = 0; i < keys.Length; i++)
			{
				Keyframe keyframe = keys[i];
				if (keyframe.time > num)
				{
					num = keyframe.time;
				}
			}
			return num;
		}

		public static float GetTimeAmplitude(this AnimationCurve self)
		{
			return self.GetHighestTime() - self.GetLowestTime();
		}

		public static int GetNearestKeyframe(this AnimationCurve self, float time)
		{
			float num = float.PositiveInfinity;
			int result = 0;
			for (int i = 0; i < self.keys.Length; i++)
			{
				float num2 = Mathf.Abs(self.keys[i].time - time);
				if (num == float.PositiveInfinity || num2 < num)
				{
					result = i;
				}
			}
			return result;
		}

		public static float GetValueAt(this AnimationCurve self, float relative)
		{
			float time = Mathf.Lerp(self.GetLowestTime(), self.GetHighestTime(), relative);
			return self.Evaluate(time);
		}

		public static void ScaleBy(this AnimationCurve self, float timeScale, float valueScale)
		{
			float lowestTime = self.GetLowestTime();
			float highestTime = self.GetHighestTime();
			float lowestKeyframeValue = self.GetLowestKeyframeValue();
			float highestKeyframeValue = self.GetHighestKeyframeValue();
			Keyframe[] keys = self.keys;
			for (int i = 0; i < keys.Length; i++)
			{
				Keyframe keyframe = keys[i];
				if (timeScale != 1f)
				{
					float t = (keyframe.time - lowestTime) / (highestTime - lowestTime);
					float num2 = (keyframe.time = Mathf.Lerp(lowestTime, self.GetTimeAmplitude() * timeScale + lowestTime, t));
				}
				if (valueScale != 1f)
				{
					float t2 = (keyframe.value - lowestKeyframeValue) / (highestKeyframeValue - lowestKeyframeValue);
					float num4 = (keyframe.value = Mathf.Lerp(lowestKeyframeValue, self.GetKeyframeValueAmplitude() * valueScale + lowestKeyframeValue, t2));
				}
				keys[i] = keyframe;
			}
			self.keys = keys;
		}

		public static void ScaleTo(this AnimationCurve self, float timeAmplitude, float valueAmplitude)
		{
			float timeAmplitude2 = self.GetTimeAmplitude();
			float valueAmplitude2 = self.GetValueAmplitude();
			self.ScaleBy(timeAmplitude / timeAmplitude2, valueAmplitude / valueAmplitude2);
		}

		public static void Reverse(this AnimationCurve self)
		{
			List<Keyframe> list = new List<Keyframe>();
			Keyframe[] keys = self.keys;
			float lowestTime = self.GetLowestTime();
			float highestTime = self.GetHighestTime();
			for (int num = self.keys.Length - 1; num >= 0; num--)
			{
				Keyframe item = keys[num];
				float time = highestTime - (item.time - lowestTime);
				float inTangent = 0f - item.outTangent;
				float outTangent = 0f - item.inTangent;
				item.time = time;
				item.inTangent = inTangent;
				item.outTangent = outTangent;
				list.Add(item);
			}
			self.keys = list.ToArray();
		}

		public static void SetTangents(this AnimationCurve self, int index, float inTangent, float outTangent)
		{
			Keyframe[] keys = self.keys;
			keys[index].inTangent = inTangent;
			keys[index].outTangent = outTangent;
			self.keys = keys;
		}

		public static void Append(this AnimationCurve self, Keyframe[] keys)
		{
			List<Keyframe> list = new List<Keyframe>(self.keys);
			float time = list[list.Count - 1].time;
			for (int i = 0; i < keys.Length; i++)
			{
				keys[i].time += time;
				list.Add(keys[i]);
			}
			self.keys = list.ToArray();
		}

		public static void SnapKeyToValue(this AnimationCurve self, int key, float value)
		{
			Keyframe[] keys = self.keys;
			Keyframe keyframe = keys[key];
			keyframe.value = value;
			self.keys = keys;
		}

		public static void SnapKeyToTime(this AnimationCurve self, int key, float time)
		{
			Keyframe[] keys = self.keys;
			Keyframe keyframe = keys[key];
			keyframe.time = time;
			self.keys = keys;
		}

		public static void ClampValues(this AnimationCurve self, float low, float high, bool clampTangents)
		{
			AnimationCurve animationCurve = new AnimationCurve();
			bool flag = false;
			for (int i = 0; i < self.keys.Length; i++)
			{
				Keyframe key = self.keys[i];
				if (key.value > high)
				{
					key.value = high;
				}
				else if (key.value < low)
				{
					key.value = low;
				}
				if (flag)
				{
					key.inTangent = 0f;
					flag = false;
				}
				if (clampTangents && i < self.keys.Length - 1)
				{
					float[] valuesInRange = self.GetValuesInRange(key.time, self.keys[i + 1].time, 3);
					float[] array = valuesInRange;
					foreach (float num in array)
					{
						if (num > high || num < low)
						{
							key.outTangent = 0f;
							flag = true;
						}
					}
				}
				animationCurve.AddKey(key);
			}
			self.keys = animationCurve.keys;
		}

		public static float[] GetValuesInRange(this AnimationCurve self, float startTime, float endTime, int count)
		{
			float num = (endTime - startTime) / (float)count;
			float[] array = new float[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = self.Evaluate(startTime + (float)(i + 1) * num);
			}
			return array;
		}

		public static void SetValue(this AnimationCurve self, int index, float value)
		{
			AnimationCurve animationCurve = new AnimationCurve(self.keys);
			Keyframe keyframe = animationCurve.keys[index];
			keyframe.value = value;
			animationCurve.keys[index] = keyframe;
			self.keys = animationCurve.keys;
		}

		public static Keyframe Front(this AnimationCurve self)
		{
			return self.keys[0];
		}

		public static Keyframe Back(this AnimationCurve self)
		{
			return self.keys[self.LastIndex()];
		}

		public static int LastIndex(this AnimationCurve self)
		{
			return self.keys.Length - 1;
		}

		public static void RemoveKeys(this AnimationCurve self)
		{
			int num = self.keys.Length;
			for (int i = 0; i < num; i++)
			{
				self.RemoveKey(0);
			}
		}
	}
}