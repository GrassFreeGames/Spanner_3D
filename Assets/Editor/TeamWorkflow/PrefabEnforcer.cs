using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TeamWorkflow
{
    [InitializeOnLoad]
    public class PrefabEnforcer
    {
        private const string PREF_KEY_SHOW_WARNINGS = "TeamWorkflow_ShowPrefabWarnings";
        private static bool showWarnings = true;
        private static double lastCheckTime = 0;
        private const double CHECK_INTERVAL = 2.0; // Check every 2 seconds

        static PrefabEnforcer()
        {
            showWarnings = EditorPrefs.GetBool(PREF_KEY_SHOW_WARNINGS, true);
            EditorApplication.update += OnEditorUpdate;
            Debug.Log("[Team Workflow] Prefab enforcement active");
        }

        private static void OnEditorUpdate()
        {
            if (!showWarnings) return;
            
            // Only check periodically to avoid performance issues
            if (EditorApplication.timeSinceStartup - lastCheckTime < CHECK_INTERVAL)
                return;
            
            lastCheckTime = EditorApplication.timeSinceStartup;
            
            // Check current selection for prefab overrides
            if (Selection.activeGameObject != null)
            {
                CheckPrefabOverrides(Selection.activeGameObject);
            }
        }

        private static void CheckPrefabOverrides(GameObject obj)
        {
            if (obj == null) return;
            
            // Check if this is a prefab instance
            if (PrefabUtility.IsPartOfPrefabInstance(obj))
            {
                // Check for overrides
                if (PrefabUtility.HasPrefabInstanceAnyOverrides(obj, false))
                {
                    // Only warn about significant overrides (not just transforms)
                    var overrides = PrefabUtility.GetObjectOverrides(obj);
                    bool hasSignificantOverrides = false;
                    
                    foreach (var over in overrides)
                    {
                        if (over.instanceObject != null && !(over.instanceObject is Transform))
                        {
                            hasSignificantOverrides = true;
                            break;
                        }
                    }
                    
                    if (hasSignificantOverrides)
                    {
                        // Only show warning once per object
                        string key = "TeamWorkflow_Warned_" + obj.GetInstanceID();
                        if (!SessionState.GetBool(key, false))
                        {
                            SessionState.SetBool(key, true);
                            WarnAboutPrefabOverride(obj);
                        }
                    }
                }
            }
        }

        private static void WarnAboutPrefabOverride(GameObject instance)
        {
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instance);
            
            int choice = EditorUtility.DisplayDialogComplex(
                "⚠️ Prefab Override Detected",
                $"GameObject: {instance.name}\n\n" +
                "Team Rule: Edit prefabs in Prefab Mode, not in scenes.\n\n" +
                "What would you like to do?",
                "Open in Prefab Mode",
                "Revert Overrides",
                "Keep Overrides"
            );

            switch (choice)
            {
                case 0: // Open in Prefab Mode
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab != null)
                    {
                        AssetDatabase.OpenAsset(prefab);
                    }
                    break;
                    
                case 1: // Revert Overrides
                    PrefabUtility.RevertPrefabInstance(instance, InteractionMode.UserAction);
                    Debug.Log($"[Team Workflow] Reverted overrides on {instance.name}");
                    break;
                    
                case 2: // Keep Overrides
                    Debug.LogWarning($"[Team Workflow] Keeping overrides on {instance.name}. " +
                                   "Remember to coordinate with the prefab owner!");
                    break;
            }
        }

        [MenuItem("Team/Workflow/Toggle Prefab Warnings")]
        private static void TogglePrefabWarnings()
        {
            showWarnings = !showWarnings;
            EditorPrefs.SetBool(PREF_KEY_SHOW_WARNINGS, showWarnings);
            
            string status = showWarnings ? "ENABLED" : "DISABLED";
            Debug.Log($"[Team Workflow] Prefab override warnings {status}");
            
            EditorUtility.DisplayDialog(
                "Prefab Warnings " + status,
                $"Prefab override warnings are now {status}",
                "OK"
            );
        }
    }
}