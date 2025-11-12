using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace TeamWorkflow
{
    public class WorkflowValidator : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<string> validationErrors = new List<string>();
        private List<string> validationWarnings = new List<string>();
        private bool hasRun = false;

        [MenuItem("Team/Validate Project %#V")] // Ctrl+Shift+V
        static void ShowWindow()
        {
            var window = GetWindow<WorkflowValidator>("Project Validator");
            window.minSize = new Vector2(500, 400);
            window.RunValidation();
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.Space(10);
            GUILayout.Label("Project Workflow Validation", EditorStyles.largeLabel);
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Run Validation", GUILayout.Height(40)))
            {
                RunValidation();
            }
            
            EditorGUILayout.Space(15);
            
            if (!hasRun)
            {
                EditorGUILayout.HelpBox("Click 'Run Validation' to check your project", MessageType.Info);
            }
            else
            {
                // Display errors
                if (validationErrors.Count > 0)
                {
                    EditorGUILayout.HelpBox(
                        $"❌ Found {validationErrors.Count} error(s)",
                        MessageType.Error
                    );
                    
                    EditorGUILayout.Space(5);
                    
                    foreach (string error in validationErrors)
                    {
                        EditorGUILayout.HelpBox(error, MessageType.Error);
                        EditorGUILayout.Space(2);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("✓ No errors found", MessageType.Info);
                }
                
                EditorGUILayout.Space(10);
                
                // Display warnings
                if (validationWarnings.Count > 0)
                {
                    EditorGUILayout.HelpBox(
                        $"⚠️ Found {validationWarnings.Count} warning(s)",
                        MessageType.Warning
                    );
                    
                    EditorGUILayout.Space(5);
                    
                    foreach (string warning in validationWarnings)
                    {
                        EditorGUILayout.HelpBox(warning, MessageType.Warning);
                        EditorGUILayout.Space(2);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("✓ No warnings", MessageType.Info);
                }
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void RunValidation()
        {
            validationErrors.Clear();
            validationWarnings.Clear();
            hasRun = true;
            
            Debug.Log("[Team Workflow] Running project validation...");
            
            ValidateProjectSettings();
            ValidatePrefabNaming();
            ValidatePrefabStructure();
            ValidateScenes();
            
            int totalIssues = validationErrors.Count + validationWarnings.Count;
            
            if (totalIssues == 0)
            {
                Debug.Log("[Team Workflow] ✓ Validation passed! No issues found.");
            }
            else
            {
                Debug.LogWarning($"[Team Workflow] Validation found {validationErrors.Count} errors " +
                               $"and {validationWarnings.Count} warnings");
            }
            
            Repaint();
        }

        private void ValidateProjectSettings()
        {
            // Check Asset Serialization Mode
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                validationErrors.Add(
                    "❌ Asset Serialization Mode is not 'Force Text'\n" +
                    "Fix: Edit → Project Settings → Editor → Asset Serialization Mode → Force Text"
                );
            }
            
            // Check Version Control Mode (only if not using Perforce)
            string versionControl = EditorSettings.externalVersionControl;
            if (versionControl != "Visible Meta Files" && versionControl != "Perforce")
            {
                validationErrors.Add(
                    "❌ Version Control Mode should be 'Visible Meta Files'\n" +
                    "Fix: Edit → Project Settings → Editor → Version Control Mode → Visible Meta Files"
                );
            }
        }

        private void ValidatePrefabNaming()
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string filename = System.IO.Path.GetFileNameWithoutExtension(path);
                
                // Check naming convention
                if (filename.Length > 0 && !char.IsUpper(filename[0]))
                {
                    validationWarnings.Add($"⚠️ Prefab should start with capital: {path}");
                }
                
                if (filename.Contains(" "))
                {
                    validationWarnings.Add($"⚠️ Prefab has spaces (use underscores): {path}");
                }
            }
        }

        private void ValidatePrefabStructure()
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab == null) continue;
                
                // Check for missing scripts
                Component[] components = prefab.GetComponentsInChildren<Component>(true);
                foreach (Component comp in components)
                {
                    if (comp == null)
                    {
                        validationErrors.Add($"❌ Missing script on prefab: {path}");
                        break;
                    }
                }
            }
        }

        private void ValidateScenes()
        {
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            
            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string filename = System.IO.Path.GetFileNameWithoutExtension(path);
                
                if (filename.Length > 0 && !char.IsUpper(filename[0]))
                {
                    validationWarnings.Add($"⚠️ Scene should start with capital: {path}");
                }
            }
        }
    }
}