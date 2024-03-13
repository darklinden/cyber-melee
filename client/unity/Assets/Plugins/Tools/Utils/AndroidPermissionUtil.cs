using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Wtf
{
    public class AndroidPermissionUtil
    {
        private static List<string> m_Permissions = new List<string>();
        public static async UniTask<List<string>> RequestPermissionIfNeeded(List<string> permissions)
        {
#if UNITY_ANDROID
            m_Permissions.Clear();
            foreach (var permission in permissions)
            {
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    m_Permissions.Add(permission);
                }
            }

            if (m_Permissions.Count == 0)
            {
                Log.D("All permissions are granted.");
                return permissions;
            }

            Log.D("Requesting permissions...");

            var permissionsGranted = new List<string>();

            var completeSource = AutoResetUniTaskCompletionSource.Create();

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += permission =>
            {
                m_Permissions.Remove(permission);
                if (m_Permissions.Count == 0)
                {
                    completeSource.TrySetResult();
                }
            };
            callbacks.PermissionGranted += permission =>
            {
                permissionsGranted.Add(permission);
                m_Permissions.Remove(permission);
                if (m_Permissions.Count == 0)
                {
                    completeSource.TrySetResult();
                }
            };
            callbacks.PermissionDeniedAndDontAskAgain += permission =>
            {
                m_Permissions.Remove(permission);
                if (m_Permissions.Count == 0)
                {
                    completeSource.TrySetResult();
                }
            };
            Permission.RequestUserPermissions(m_Permissions.ToArray(), callbacks);

            await completeSource.Task;

            return permissionsGranted;
#else
            await UniTask.Yield();
            return permissions;
#endif
        }
    }
}
