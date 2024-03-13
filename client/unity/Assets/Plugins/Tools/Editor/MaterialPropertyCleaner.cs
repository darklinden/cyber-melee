// https://forum.unity.com/threads/clear-old-texture-references-from-materials.318769/

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Wtf.Editor
{
    public class MaterialPropertyCleaner : EditorWindow
    {
        private const float REMOVE_BUTTON_WIDTH = 60;
        private const float TYPE_SPACING = 4;
        private const float SCROLLBAR_WIDTH = 15;
        private const float MATERIAL_SPACING = 20;

        private List<Material> m_selectedMaterials = new List<Material>();
        private SerializedObject[] m_serializedObjects;
        private Vector2 scrollPos;
        private GUIStyle warningStyle, errorStyle;

        [MenuItem("Tools/Material Property Cleaner")]
        private static void Init()
        {
            GetWindow<MaterialPropertyCleaner>("Property Cleaner");
        }

        protected virtual void OnEnable()
        {
            GetSelectedMaterials();

            Undo.undoRedoPerformed += OnUndoRedo;
        }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }
        private void OnUndoRedo()
        {
            Repaint();
        }

        protected virtual void OnSelectionChange()
        {
            GetSelectedMaterials();
        }

        protected virtual void OnProjectChange()
        {
            GetSelectedMaterials();
        }

        protected virtual void OnGUI()
        {
            if (m_selectedMaterials == null || m_selectedMaterials.Count <= 0)
            {
                EditorGUILayout.LabelField("No Material Selected", new GUIStyle("LargeLabel"));
            }
            else
            {
                EditorGUIUtility.labelWidth = position.width * 0.5f - SCROLLBAR_WIDTH - 2;
                GUIStyle typeLabelStyle = new GUIStyle("LargeLabel");
                errorStyle = new GUIStyle("CN StatusError");
                warningStyle = new GUIStyle("CN StatusWarn");

                if (GUILayout.Button("Remove All Old References"))
                {
                    for (int i = 0; i < m_selectedMaterials.Count; i++)
                    {
                        var mat = m_selectedMaterials[i];
                        if (HasShader(mat))
                        {
                            RemoveUnusedProperties("m_SavedProperties.m_TexEnvs", i, PropertyType.TexEnv);
                            RemoveUnusedProperties("m_SavedProperties.m_Ints", i, PropertyType.Int);
                            RemoveUnusedProperties("m_SavedProperties.m_Floats", i, PropertyType.Float);
                            RemoveUnusedProperties("m_SavedProperties.m_Colors", i, PropertyType.Color);
                        }
                        else
                            Debug.LogError("Material " + mat.name + " doesn't have a shader");
                    }

                    GUIUtility.ExitGUI();
                }

                var scrollBarStyle = new GUIStyle(GUI.skin.verticalScrollbar);
                scrollBarStyle.fixedWidth = SCROLLBAR_WIDTH;
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, true, GUIStyle.none, scrollBarStyle, GUI.skin.box);
                EditorGUILayout.BeginVertical();

                for (int i = 0; i < m_selectedMaterials.Count; i++)
                {
                    EditorGUILayout.Space(MATERIAL_SPACING);

                    Material m_selectedMaterial = m_selectedMaterials[i];

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(m_selectedMaterial.name, GUILayout.Width(EditorGUIUtility.labelWidth)))
                        EditorGUIUtility.PingObject(m_selectedMaterial);
                    if (!HasShader(m_selectedMaterial))
                        EditorGUILayout.LabelField("NULL Shader", errorStyle);
                    else
                    {
                        if (GUILayout.Button(m_selectedMaterial.shader.name, GUILayout.Width(EditorGUIUtility.labelWidth))) //, new GUIStyle("miniButton")
                            EditorGUIUtility.PingObject(m_selectedMaterial.shader);
                    }
                    EditorGUILayout.EndHorizontal();

                    m_serializedObjects[i].Update();

                    EditorGUI.indentLevel++;
                    {
                        EditorGUILayout.Space(TYPE_SPACING);

                        EditorGUILayout.LabelField("Textures", typeLabelStyle);
                        EditorGUI.indentLevel++;
                        ProcessProperties("m_SavedProperties.m_TexEnvs", i);
                        EditorGUI.indentLevel--;

                        EditorGUILayout.Space(TYPE_SPACING);

                        EditorGUILayout.LabelField("Ints", typeLabelStyle);
                        EditorGUI.indentLevel++;
                        ProcessProperties("m_SavedProperties.m_Ints", i);
                        EditorGUI.indentLevel--;

                        EditorGUILayout.Space(TYPE_SPACING);

                        EditorGUILayout.LabelField("Floats", typeLabelStyle);
                        EditorGUI.indentLevel++;
                        ProcessProperties("m_SavedProperties.m_Floats", i);
                        EditorGUI.indentLevel--;

                        EditorGUILayout.Space(TYPE_SPACING);

                        EditorGUILayout.LabelField("Colors", typeLabelStyle);
                        EditorGUI.indentLevel++;
                        ProcessProperties("m_SavedProperties.m_Colors", i);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space(MATERIAL_SPACING);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();

                EditorGUIUtility.labelWidth = 0;
            }
        }

        private enum PropertyType { TexEnv, Int, Float, Color }
        private static bool ShaderHasProperty(Material mat, string name)
        {
            return mat.HasProperty(name);
        }

        private static string GetName(SerializedProperty property)
        {
            return property.FindPropertyRelative("first").stringValue; // return property.displayName;
        }

        private static bool HasShader(Material mat)
        {
            return mat.shader.name != "Hidden/InternalErrorShader";
        }

        private void RemoveUnusedProperties(string path, int i, PropertyType type)
        {
            if (!HasShader(m_selectedMaterials[i]))
            {
                Debug.LogError("Material " + m_selectedMaterials[i].name + " doesn't have a shader");
                return;
            }

            var properties = m_serializedObjects[i].FindProperty(path);
            if (properties != null && properties.isArray)
            {
                for (int j = properties.arraySize - 1; j >= 0; j--)
                {
                    string propName = GetName(properties.GetArrayElementAtIndex(j));
                    bool exists = ShaderHasProperty(m_selectedMaterials[i], propName);

                    if (!exists)
                    {
                        Debug.Log("Removed " + type + " Property: " + propName);
                        properties.DeleteArrayElementAtIndex(j);
                        m_serializedObjects[i].ApplyModifiedProperties();
                    }
                }
            }
        }

        private void ProcessProperties(string path, int i)
        {
            var properties = m_serializedObjects[i].FindProperty(path);
            if (properties != null && properties.isArray)
            {
                for (int j = 0; j < properties.arraySize; j++)
                {
                    string propName = GetName(properties.GetArrayElementAtIndex(j));
                    bool exists = ShaderHasProperty(m_selectedMaterials[i], propName);

                    if (!HasShader(m_selectedMaterials[i]))
                    {
                        EditorGUILayout.LabelField(propName, "UNKNOWN", errorStyle);
                    }
                    else if (exists)
                    {
                        EditorGUILayout.LabelField(propName, "Exists"); // in Shader
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        float w = EditorGUIUtility.labelWidth * 2 - REMOVE_BUTTON_WIDTH;
                        EditorGUILayout.LabelField(propName, "Old Reference", warningStyle, GUILayout.Width(w));
                        if (GUILayout.Button("Remove", GUILayout.Width(REMOVE_BUTTON_WIDTH)))
                        {
                            properties.DeleteArrayElementAtIndex(j);
                            m_serializedObjects[i].ApplyModifiedProperties();
                            GUIUtility.ExitGUI();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        private void GetSelectedMaterials()
        {
            Object[] objects = Selection.objects;

            m_selectedMaterials = new List<Material>();

            for (int i = 0; i < objects.Length; i++)
            {
                Material newMat = objects[i] as Material;
                if (newMat != null)
                    m_selectedMaterials.Add(newMat);
            }

            if (m_selectedMaterials != null)
            {
                m_serializedObjects = new SerializedObject[m_selectedMaterials.Count];
                for (int i = 0; i < m_serializedObjects.Length; i++)
                    m_serializedObjects[i] = new SerializedObject(m_selectedMaterials[i]);
            }

            Repaint();
        }
    }
}