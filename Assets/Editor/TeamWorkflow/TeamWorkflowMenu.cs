using UnityEngine;
using UnityEditor;
using System.IO;

namespace TeamWorkflow
{
    public static class TeamWorkflowMenu
    {
        [MenuItem("Team/Documentation/Open Workflow Rules")]
        private static void OpenWorkflowRules()
        {
            OpenFileInProject("WORKFLOW_RULES.md");
        }

        [MenuItem("Team/Documentation/Open Prefab Owners")]
        private static void OpenPrefabOwners()
        {
            OpenFileInProject("PREFAB_OWNERS.md");
        }

        [MenuItem("Team/Quick Actions/Save All & Commit Prep")]
        private static void SaveAllAndCommitPrep()
        {
            // Save all scenes
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            
            // Save project
            AssetDatabase.SaveAssets();
            
            Debug.Log("[Team Workflow] ✓ All scenes and assets saved!");
            
            bool closeUnity = EditorUtility.DisplayDialog(
                "Ready to Commit",
                "All scenes and assets have been saved.\n\n" +
                "Next steps:\n" +
                "1. Close Unity\n" +
                "2. Run: git status\n" +
                "3. Commit your changes\n" +
                "4. Push immediately\n\n" +
                "Close Unity now?",
                "Yes, Close Unity",
                "No, Keep Working"
            );

            if (closeUnity)
            {
                EditorApplication.Exit(0);
            }
        }

        [MenuItem("Team/Help/Keyboard Shortcuts")]
        private static void ShowKeyboardShortcuts()
        {
            EditorUtility.DisplayDialog(
                "Team Workflow Shortcuts",
                "Ctrl+Shift+P - Open Team Prefab Editor\n" +
                "Ctrl+Shift+V - Validate Project\n" +
                "\n" +
                "Access all tools from the 'Team' menu.",
                "OK"
            );
        }

        [MenuItem("Team/Help/About Team Workflow")]
        private static void ShowAbout()
        {
            EditorUtility.DisplayDialog(
                "Team Workflow Tools",
                "Version: 1.0\n" +
                "Created for: Spanner_3D Team\n\n" +
                "These tools help enforce Git and Unity best practices.\n\n" +
                "For help, contact your QA Lead (Stu).",
                "OK"
            );
        }

        private static void OpenFileInProject(string fileName)
        {
            string path = Path.Combine(Application.dataPath, "..", fileName);
            
            if (File.Exists(path))
            {
                System.Diagnostics.Process.Start(path);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "File Not Found",
                    $"{fileName} not found in project root.\n\n" +
                    "Expected location: " + path,
                    "OK"
                );
            }
        }
    }
}