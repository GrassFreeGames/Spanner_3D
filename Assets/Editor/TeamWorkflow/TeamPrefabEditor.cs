using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace TeamWorkflow
{
    public class TeamPrefabEditor : EditorWindow
    {
        private GameObject selectedPrefab;
        private string currentUser = "";
        private Vector2 scrollPosition;
        
        private static readonly string[] teamMembers = new string[]
        {
            "Stu", "Select Your Name...", "Other"
        };

        [MenuItem("Team/Edit Prefab (Safe) %#P")] // Ctrl+Shift+P
        static void ShowWindow()
        {
            var window = GetWindow<TeamPrefabEditor>("Team Prefab Editor");
            window.minSize = new Vector2(400, 500);
        }

        void OnEnable()
        {
            currentUser = EditorPrefs.GetString("TeamWorkflow_CurrentUser", "");
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Header
            EditorGUILayout.Space(10);
            GUILayout.Label("Safe Prefab Editing Tool", EditorStyles.largeLabel);
            EditorGUILayout.Space(10);
            
            // User selection
            EditorGUILayout.LabelField("Who are you?", EditorStyles.boldLabel);
            int userIndex = System.Array.IndexOf(teamMembers, currentUser);
            if (userIndex == -1) userIndex = 0;
            
            userIndex = EditorGUILayout.Popup("Team Member:", userIndex, teamMembers);
            currentUser = teamMembers[userIndex];
            
            if (currentUser == "Other" || currentUser == "Select Your Name...")
            {
                currentUser = EditorGUILayout.TextField("Your Name:", currentUser);
            }
            
            EditorPrefs.SetString("TeamWorkflow_CurrentUser", currentUser);
            
            EditorGUILayout.Space(10);
            
            // Prefab selection
            EditorGUILayout.LabelField("Select Prefab to Edit", EditorStyles.boldLabel);
            selectedPrefab = EditorGUILayout.ObjectField("Prefab:", selectedPrefab, typeof(GameObject), false) as GameObject;
            
            EditorGUILayout.Space(10);
            
            // Show prefab info if selected
            if (selectedPrefab != null)
            {
                string prefabPath = AssetDatabase.GetAssetPath(selectedPrefab);
                
                if (PrefabUtility.GetPrefabAssetType(selectedPrefab) != PrefabAssetType.NotAPrefab)
                {
                    EditorGUILayout.HelpBox(
                        $"Prefab: {selectedPrefab.name}\nPath: {prefabPath}",
                        MessageType.Info
                    );
                    
                    // Check ownership
                    string owner = GetPrefabOwner(prefabPath);
                    if (!string.IsNullOrEmpty(owner))
                    {
                        if (owner.Equals(currentUser, System.StringComparison.OrdinalIgnoreCase))
                        {
                            EditorGUILayout.HelpBox(
                                $"✓ You ({currentUser}) are the owner of this prefab",
                                MessageType.Info
                            );
                        }
                        else
                        {
                            EditorGUILayout.HelpBox(
                                $"⚠️ Owner: {owner}\nYou ({currentUser}) are NOT the owner.\n" +
                                "Please coordinate with the owner before editing.",
                                MessageType.Warning
                            );
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(
                            "No owner assigned. Check PREFAB_OWNERS.md",
                            MessageType.Warning
                        );
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Selected object is not a prefab!",
                        MessageType.Error
                    );
                }
            }
            
            EditorGUILayout.Space(10);
            
            // Action buttons
            GUI.enabled = selectedPrefab != null && 
                         !string.IsNullOrEmpty(currentUser) && 
                         currentUser != "Select Your Name...";
            
            if (GUILayout.Button("Open in Prefab Mode", GUILayout.Height(40)))
            {
                OpenPrefabMode();
            }
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Find Scenes Using This Prefab", GUILayout.Height(30)))
            {
                FindScenesUsingPrefab();
            }
            
            GUI.enabled = true;
            
            EditorGUILayout.Space(15);
            
            // Team rules reminder
            EditorGUILayout.HelpBox(
                "TEAM PREFAB RULES:\n\n" +
                "✓ Always edit in Prefab Mode\n" +
                "✓ Test changes before committing\n" +
                "✓ Only edit prefabs you own\n" +
                "✓ Communicate major changes\n" +
                "✓ Commit and push immediately",
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            
            // Quick links
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Open PREFAB_OWNERS.md"))
            {
                OpenPrefabOwnersFile();
            }
            
            if (GUILayout.Button("Open WORKFLOW_RULES.md"))
            {
                OpenWorkflowRulesFile();
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void OpenPrefabMode()
        {
            string prefabPath = AssetDatabase.GetAssetPath(selectedPrefab);
            string owner = GetPrefabOwner(prefabPath);
            
            bool canProceed = true;
            
            if (!string.IsNullOrEmpty(owner) && 
                !owner.Equals(currentUser, System.StringComparison.OrdinalIgnoreCase))
            {
                canProceed = EditorUtility.DisplayDialog(
                    "Ownership Warning",
                    $"This prefab is owned by {owner}, but you are {currentUser}.\n\n" +
                    "Are you sure you want to edit it?\n" +
                    "(Make sure you've coordinated with the owner)",
                    "Yes, Open Anyway",
                    "Cancel"
                );
            }
            
            if (canProceed)
            {
                AssetDatabase.OpenAsset(selectedPrefab);
                Debug.Log($"[Team Workflow] {currentUser} opened {selectedPrefab.name} in Prefab Mode");
            }
        }

        private void FindScenesUsingPrefab()
        {
            string prefabPath = AssetDatabase.GetAssetPath(selectedPrefab);
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
            
            bool found = false;
            string results = $"Scenes using prefab '{selectedPrefab.name}':\n\n";
            
            foreach (string guid in sceneGuids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                
                // Skip scenes in Packages folder
                if (scenePath.StartsWith("Packages/")) continue;
                
                string sceneContent = File.ReadAllText(scenePath);
                string prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);
                
                if (sceneContent.Contains(prefabGuid))
                {
                    found = true;
                    results += $"• {scenePath}\n";
                }
            }
            
            if (!found)
            {
                results += "No scenes found using this prefab.";
            }
            
            EditorUtility.DisplayDialog("Scene Usage", results, "OK");
            Debug.Log($"[Team Workflow] {results}");
        }

        private string GetPrefabOwner(string prefabPath)
        {
            string ownersPath = Path.Combine(Application.dataPath, "../PREFAB_OWNERS.md");
            
            if (File.Exists(ownersPath))
            {
                string content = File.ReadAllText(ownersPath);
                string[] lines = content.Split('\n');
                
                foreach (string line in lines)
                {
                    if (line.Contains(prefabPath) || line.Contains(Path.GetFileName(prefabPath)))
                    {
                        int dashIndex = line.LastIndexOf('-');
                        if (dashIndex > 0 && dashIndex < line.Length - 1)
                        {
                            string ownerPart = line.Substring(dashIndex + 1).Trim();
                            int parenIndex = ownerPart.IndexOf('(');
                            if (parenIndex > 0)
                            {
                                return ownerPart.Substring(0, parenIndex).Trim();
                            }
                            return ownerPart;
                        }
                    }
                }
            }
            
            return null;
        }

        private void OpenPrefabOwnersFile()
        {
            string path = Path.Combine(Application.dataPath, "../PREFAB_OWNERS.md");
            OpenFile(path, "PREFAB_OWNERS.md");
        }

        private void OpenWorkflowRulesFile()
        {
            string path = Path.Combine(Application.dataPath, "../WORKFLOW_RULES.md");
            OpenFile(path, "WORKFLOW_RULES.md");
        }

        private void OpenFile(string path, string fileName)
        {
            if (File.Exists(path))
            {
                System.Diagnostics.Process.Start(path);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "File Not Found",
                    $"{fileName} not found in project root.\n\n" +
                    "Please make sure this file exists.",
                    "OK"
                );
            }
        }
    }
}