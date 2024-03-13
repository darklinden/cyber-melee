using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Wtf
{
    public static class TransformExtensions
    {
        public static Transform GetChildByIndex(this Transform self, int index)
        {
            Transform[] childrenSorted = self.GetChildrenSorted();
            return (index >= childrenSorted.Length) ? null : childrenSorted[index];
        }

        public static Transform[] GetChildren(this Transform self, bool recursive)
        {
            List<Transform> list = new List<Transform>();
            if (recursive)
            {
                AddChildrenToListRecursively(list, self);
            }
            else
            {
                foreach (Transform item in self)
                {
                    if (item != self)
                    {
                        list.Add(item);
                    }
                }
            }
            return list.ToArray();
        }

        public static Transform[] GetChildrenSorted(this Transform self, IComparer<Transform> sorter = null)
        {
            List<Transform> list = new List<Transform>(self.GetChildren(false));
            if (sorter == null)
            {
                list.Sort(SortAlphabetically);
            }
            else
            {
                list.Sort(sorter);
            }
            return list.ToArray();
        }

        public static int GetRecursiveChildCount(this Transform self)
        {
            return self.GetComponentsInChildren<Transform>().Length - 1;
        }

        public static Transform[] GetSiblings(this Transform self)
        {
            if (self.parent == null)
            {
                return new Transform[0];
            }
            List<Transform> list = new List<Transform>();
            foreach (Transform item in self.parent)
            {
                if (item != self && item != self.parent)
                {
                    list.Add(item);
                }
            }
            return list.ToArray();
        }

        public static bool IsSiblingOf(this Transform self, Transform other)
        {
            return self.parent == other.parent;
        }

        public static void SetParentAndMaintainLocalTm(this Transform self, Transform parent)
        {
            Vector3 localPosition = self.localPosition;
            Quaternion localRotation = self.localRotation;
            Vector3 localScale = self.localScale;
            self.SetParent(parent);
            self.localPosition = localPosition;
            self.localRotation = localRotation;
            self.localScale = localScale;
        }

        public static void SetWorldScale(this Transform self, Vector3 scale)
        {
            self.localScale = Vector3.one;
            Vector3 lossyScale = self.lossyScale;
            if (lossyScale.x != 0f)
            {
                scale.x /= lossyScale.x;
            }
            if (lossyScale.y != 0f)
            {
                scale.y /= lossyScale.y;
            }
            if (lossyScale.z != 0f)
            {
                scale.z /= lossyScale.z;
            }
            self.localScale = scale;
        }

        public static Transform FindChildRecursively(this Transform self, string name)
        {
            Transform[] componentsInChildren = self.GetComponentsInChildren<Transform>(true);
            foreach (Transform transform in componentsInChildren)
            {
                if (transform.name == name)
                {
                    return transform;
                }
            }
            return null;
        }

        public static Transform FindChildWithTag(this Transform self, string tag)
        {
            foreach (Transform item in self)
            {
                if (item.gameObject.CompareTag(tag))
                {
                    return item;
                }
            }
            return null;
        }

        public static Transform FindChildWithTagRecursively(this Transform self, string tag)
        {
            Transform[] componentsInChildren = self.GetComponentsInChildren<Transform>(true);
            foreach (Transform transform in componentsInChildren)
            {
                if (transform.gameObject.CompareTag(tag))
                {
                    return transform;
                }
            }
            return null;
        }

        public static Transform FindChildRegex(this Transform self, string pattern, bool recursive = false)
        {
            Transform[] children = self.GetChildren(recursive);
            for (int i = 0; i < children.Length; i++)
            {
                Regex regex = new Regex(pattern);
                if (regex.IsMatch(children[i].name))
                {
                    return children[i];
                }
            }
            return null;
        }

        public static Transform FindChildRegex(this Transform self, string[] patterns, bool recursive = false)
        {
            Transform[] children = self.GetChildren(recursive);
            for (int i = 0; i < patterns.Length; i++)
            {
                for (int j = 0; j < children.Length; j++)
                {
                    Regex regex = new Regex(patterns[i]);
                    if (regex.IsMatch(children[j].name))
                    {
                        return children[j];
                    }
                }
            }
            return null;
        }

        public static void CopyFrom(this Transform self, Transform other, bool localSpace)
        {
            if (localSpace)
            {
                self.localPosition = other.localPosition;
                self.localRotation = other.localRotation;
                self.localScale = other.localScale;
            }
            else
            {
                self.position = other.position;
                self.rotation = other.rotation;
                self.SetWorldScale(other.lossyScale);
            }
        }

        public static void ScaleBy(this Transform self, Vector3 scaleBy)
        {
            Vector3 localScale = self.localScale;
            localScale.Scale(scaleBy);
            self.localScale = localScale;
        }

        public static void ScaleBy(this Transform self, float scaleBy)
        {
            self.localScale *= scaleBy;
        }

        public static void DestroyChildren(this Transform self)
        {
            Transform[] componentsInChildren = self.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                if (componentsInChildren[i] != self)
                {
                    Object.Destroy(componentsInChildren[i].gameObject);
                }
            }
        }

        public static void DestroyChildrenImmediate(this Transform self)
        {
            Transform[] componentsInChildren = self.GetComponentsInChildren<Transform>(true);
            for (int num = componentsInChildren.Length - 1; num >= 0; num--)
            {
                if (componentsInChildren[num] != self)
                {
                    Object.DestroyImmediate(componentsInChildren[num].gameObject);
                }
            }
        }

        private static int SortAlphabetically(Transform a, Transform b)
        {
            return a.name.CompareTo(b.name);
        }

        private static void AddChildrenToListRecursively(List<Transform> list, Transform parent)
        {
            foreach (Transform item in parent)
            {
                list.Add(item);
                AddChildrenToListRecursively(list, item);
            }
        }
    }
}