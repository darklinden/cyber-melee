using System;
using UnityEngine;

namespace Wtf
{
	public static class GameObjectExtensions
	{
		public static GameObject FindSecurely(this GameObject self, string name, bool recursive = true)
		{
			GameObject gameObject = null;
			Transform transform = self.transform;
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (child.gameObject.name == name)
				{
					gameObject = child.gameObject;
					break;
				}
				if (recursive)
				{
					Transform transform2 = child.FindChildRecursively(name);
					if ((bool)transform2)
					{
						gameObject = transform2.gameObject;
						break;
					}
				}
			}
			if (!gameObject)
			{
				Debug.LogError("GameObject `" + name + "' not found!");
			}
			return gameObject;
		}

		public static T GetComponentAbove<T>(this GameObject self) where T : Component
		{
			Transform parent = self.transform.parent;
			T val = (T)null;
			while (parent != null)
			{
				val = parent.GetComponent<T>();
				if ((UnityEngine.Object)val != (UnityEngine.Object)null)
				{
					break;
				}
				parent = parent.parent;
			}
			return val;
		}

		public static Component GetComponentAbove(this GameObject self, Type type)
		{
			Transform parent = self.transform.parent;
			Component component = null;
			while (parent != null)
			{
				component = parent.GetComponent(type);
				if (component != null)
				{
					break;
				}
				parent = parent.parent;
			}
			return component;
		}

		public static T FindComponentInChildren<T>(this GameObject self, bool includeInactive = true) where T : Component
		{
			//Discarded unreachable code: IL_0019
			T[] componentsInChildren = self.GetComponentsInChildren<T>(includeInactive);
			int num = 0;
			if (num < componentsInChildren.Length)
			{
				return componentsInChildren[num];
			}
			return (T)null;
		}

		public static void SetLayerRecursively(this GameObject self, int layer)
		{
			self.layer = layer;
			Transform[] children = self.transform.GetChildren(true);
			for (int num = children.Length - 1; num >= 0; num--)
			{
				children[num].gameObject.layer = layer;
			}
		}

		public static void SetStaticRecursively(this GameObject self, bool flag)
		{
			self.isStatic = flag;
			Transform[] children = self.transform.GetChildren(true);
			for (int num = children.Length - 1; num >= 0; num--)
			{
				children[num].gameObject.isStatic = flag;
			}
		}

		public static T AddOrGetComponent<T>(this GameObject self) where T : Component
		{
			T val = self.GetComponent<T>();
			if ((UnityEngine.Object)val == (UnityEngine.Object)null)
			{
				val = self.AddComponent<T>();
			}
			return val;
		}

		public static void SetActiveIfDifferent(this GameObject self, bool active)
		{
			if (self.activeSelf != active)
			{
				self.SetActive(active);
			}
		}
	}
}