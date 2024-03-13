using UnityEditor;
using UnityEngine;

namespace Wtf.Editor
{
    public static class FindAndAddMissingMaterial
    {
        private static int _goCount;
        private static int _missingCount;

        private static bool MaterialAllNull(Material[] materials)
        {
            bool allNull = true;
            foreach (var m in materials)
            {
                if (m != null)
                {
                    allNull = false;
                    break;
                }
            }
            return allNull;
        }

        private static Material[] MaterialSetAll(Material material, int count)
        {
            var Materials = new Material[count];
            for (int i = 0; i < Materials.Length; i++)
            {
                Materials[i] = material;
            }
            return Materials;
        }

        private static string PathOfGameObject(GameObject g)
        {
            var s = g.name;
            var t = g.transform;
            while (t.parent != null)
            {
                s = t.parent.name + "/" + s;
                t = t.parent;
            }
            return s;
        }


        [MenuItem("GameObject/Find And Add Missing Material", false, 0x123456)]
        private static void RemoveMissingScripts()
        {
            GameObject[] gameObjects = Selection.gameObjects;
            if (gameObjects.Length < 1)
            {
                Debug.LogError("FindAndAddMissingMaterial: Please Select At Least One GameObject");
                return;
            }

            _goCount = 0;
            _missingCount = 0;

            var debugMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/AddrFiles/Debug/debugMaterial");
            var debugMaterialParticle = AssetDatabase.LoadAssetAtPath<Material>("Assets/AddrFiles/Debug/debugMaterialParticle");


            for (int i = 0; i < gameObjects.Length; i++)
            {
                var go = gameObjects[i];
                foreach (Transform child in go.transform.GetComponentsInChildren<Transform>(true /** includeInactive */))
                {
                    var g = child.gameObject;

                    var meshRenderer = g.GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        if (meshRenderer.sharedMaterials.Length > 0 && MaterialAllNull(meshRenderer.sharedMaterials))
                        {
                            meshRenderer.sharedMaterials = MaterialSetAll(debugMaterial, meshRenderer.sharedMaterials.Length);

                            Debug.Log($"{PathOfGameObject(g)} Add Missing Mesh Material");
                            _missingCount++;
                        }
                    }

                    var particleRender = g.GetComponent<ParticleSystemRenderer>();
                    if (particleRender != null)
                    {
                        if (particleRender.sharedMaterials.Length > 0 && MaterialAllNull(particleRender.sharedMaterials))
                        {
                            particleRender.sharedMaterials = MaterialSetAll(debugMaterialParticle, particleRender.sharedMaterials.Length);

                            Debug.Log($"{PathOfGameObject(g)} Add Missing Particle Material");
                            _missingCount++;
                        }
                    }
                }
            }

            Debug.Log($"Searched {_goCount} GameObjects, Added {_missingCount} Missing Material");
        }
    }
}