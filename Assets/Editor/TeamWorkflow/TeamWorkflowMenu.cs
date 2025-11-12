using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

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

        [MenuItem("Team/Git Actions/Fetch Remote Changes", priority = 0)]
        private static void GitFetch()
        {
            RunGitCommand("git fetch", "Fetch Remote Changes");
        }

        [MenuItem("Team/Git Actions/Pull Latest Changes", priority = 1)]
        private static void GitPull()
        {
            if (!ConfirmGitAction("Pull Latest Changes",
                "This will pull the latest changes from the remote repository.\n\n" +
                "Make sure you've saved all your work first.\n\n" +
                "Continue?"))
            {
                return;
            }

            RunGitCommand("git pull", "Pull Latest");
        }

        [MenuItem("Team/Git Actions/Check Status", priority = 2)]
        private static void GitStatus()
        {
            RunGitCommand("git status", "Git Status", showDialog: false);
        }

        [MenuItem("Team/Git Actions/View Uncommitted Changes", priority = 3)]
        private static void GitDiff()
        {
            RunGitCommand("git diff --name-status", "Uncommitted Changes", showDialog: false);
        }

        [MenuItem("Team/Git Actions/View Commit History", priority = 4)]
        private static void GitLog()
        {
            RunGitCommand("git log --oneline --graph --decorate -20", "Recent Commit History", showDialog: true);
        }

        [MenuItem("Team/Git Actions/View My Recent Commits", priority = 5)]
        private static void GitLogMine()
        {
            ProcessStartInfo getUserInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C git config user.name",
                WorkingDirectory = Path.GetDirectoryName(Application.dataPath),
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process getUserProcess = new Process { StartInfo = getUserInfo };
            getUserProcess.Start();
            string userName = getUserProcess.StandardOutput.ReadToEnd().Trim();
            getUserProcess.WaitForExit();

            if (string.IsNullOrEmpty(userName))
            {
                userName = "Unknown";
            }

            RunGitCommand($"git log --oneline --author=\"{userName}\" -10", 
                         $"Your Recent Commits ({userName})", 
                         showDialog: true);
        }

        [MenuItem("Team/Git Actions/Stage All Changes", priority = 10)]
        private static void GitAddAll()
        {
            if (!ConfirmGitAction("Stage All Changes",
                "This will stage ALL modified files for commit.\n\n" +
                "Make sure you've reviewed your changes first.\n\n" +
                "Continue?"))
            {
                return;
            }

            RunGitCommand("git add .", "Stage All");
        }

        [MenuItem("Team/Git Actions/Show Remote URL", priority = 20)]
        private static void GitRemote()
        {
            RunGitCommand("git remote -v", "Remote Repository", showDialog: true);
        }

        [MenuItem("Team/Quick Actions/Save All & Commit Prep", priority = 1)]
        private static void SaveAllAndCommitPrep()
        {
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            
            UnityEngine.Debug.Log("[Team Workflow] ✓ All scenes and assets saved!");
            
            bool shouldShowStatus = EditorUtility.DisplayDialog(
                "Ready to Commit",
                "All scenes and assets have been saved.\n\n" +
                "What would you like to do next?",
                "Show Git Status",
                "Close Unity"
            );

            if (shouldShowStatus)
            {
                RunGitCommand("git status", "Git Status", showDialog: false);
            }
            else
            {
                EditorApplication.Exit(0);
            }
        }

        [MenuItem("Team/Quick Actions/Start of Day Workflow", priority = 10)]
        private static void StartOfDayWorkflow()
        {
            int choice = EditorUtility.DisplayDialogComplex(
                "Start of Day Workflow",
                "Recommended steps:\n\n" +
                "1. Fetch/Pull latest changes from remote\n" +
                "2. Check what changed overnight\n" +
                "3. Announce what you're working on\n" +
                "4. Start work\n\n" +
                "What would you like to do?",
                "Fetch & View Changes",
                "Pull Immediately",
                "Skip"
            );

            if (choice == 0) // Fetch & View
            {
                RunGitCommand("git fetch", "Fetch Remote Changes");
                RunGitCommand("git log HEAD..origin/main --oneline", "New Commits on Remote", showDialog: true);
                
                bool shouldPull = EditorUtility.DisplayDialog(
                    "Pull Changes?",
                    "Would you like to pull these changes now?",
                    "Yes, Pull Now",
                    "No, Later"
                );
                
                if (shouldPull)
                {
                    RunGitCommand("git pull", "Pull Latest");
                }
            }
            else if (choice == 1) // Pull Immediately
            {
                RunGitCommand("git pull", "Pull Latest");
            }

            EditorUtility.DisplayDialog(
                "Don't Forget!",
                "Remember to announce in team chat:\n\n" +
                "• What you're working on today\n" +
                "• Which scenes/prefabs you'll be editing\n" +
                "• Any potential conflicts with teammates",
                "Got it!"
            );
        }

        [MenuItem("Team/Quick Actions/End of Day Workflow", priority = 11)]
        private static void EndOfDayWorkflow()
        {
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            
            bool shouldCommit = EditorUtility.DisplayDialog(
                "End of Day Workflow",
                "All files saved.\n\n" +
                "Next steps:\n" +
                "1. Close Unity\n" +
                "2. Run check-commit.bat\n" +
                "3. Check status: git status\n" +
                "4. Stage changes: git add .\n" +
                "5. Commit: git commit -m \"Your message\" --no-verify\n" +
                "6. Push: git push\n\n" +
                "Close Unity and open command prompt?",
                "Yes",
                "No"
            );

            if (shouldCommit)
            {
                string projectPath = Path.GetDirectoryName(Application.dataPath);
                Process.Start("cmd.exe", "/K cd /d \"" + projectPath + "\"");
                
                EditorApplication.Exit(0);
            }
        }

        [MenuItem("Team/Help/Keyboard Shortcuts", priority = 100)]
        private static void ShowKeyboardShortcuts()
        {
            EditorUtility.DisplayDialog(
                "Team Workflow Shortcuts",
                "Ctrl+Shift+P - Open Team Prefab Editor\n" +
                "Ctrl+Shift+V - Validate Project\n" +
                "\n" +
                "Git Actions:\n" +
                "• Fetch Remote Changes\n" +
                "• Pull Latest Changes\n" +
                "• Check Status\n" +
                "• View Commit History\n" +
                "• View My Recent Commits\n" +
                "\n" +
                "Quick Workflows:\n" +
                "• Team → Quick Actions → Start of Day\n" +
                "• Team → Quick Actions → End of Day\n" +
                "\n" +
                "Access all tools from the 'Team' menu.",
                "OK"
            );
        }

        [MenuItem("Team/Help/About Team Workflow", priority = 101)]
        private static void ShowAbout()
        {
            EditorUtility.DisplayDialog(
                "Team Workflow Tools",
                "Version: 1.2\n" +
                "Created for: Spanner_3D Team\n\n" +
                "These tools help enforce Git and Unity best practices.\n\n" +
                "Features:\n" +
                "• Git command shortcuts (fetch, pull, status, log)\n" +
                "• Prefab editing enforcement\n" +
                "• Project validation\n" +
                "• Workflow automation\n" +
                "• Start/End of day workflows\n\n" +
                "For help, contact your QA Lead (Stu).",
                "OK"
            );
        }

        private static void OpenFileInProject(string fileName)
        {
            string path = Path.Combine(Application.dataPath, "..", fileName);
            
            if (File.Exists(path))
            {
                Process.Start(path);
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

        private static bool ConfirmGitAction(string title, string message)
        {
            return EditorUtility.DisplayDialog(title, message, "Continue", "Cancel");
        }

        private static void RunGitCommand(string command, string windowTitle, bool showDialog = true)
        {
            try
            {
                string projectPath = Path.GetDirectoryName(Application.dataPath);
                
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C " + command,
                    WorkingDirectory = projectPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = new Process { StartInfo = startInfo };
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                {
                    UnityEngine.Debug.Log($"[{windowTitle}]\n{output}");
                }

                if (!string.IsNullOrEmpty(error) && !error.Contains("Already up to date"))
                {
                    UnityEngine.Debug.LogWarning($"[{windowTitle}] {error}");
                }

                if (showDialog)
                {
                    string displayMessage = output;
                    if (!string.IsNullOrEmpty(error))
                    {
                        displayMessage += "\n\nWarnings/Errors:\n" + error;
                    }

                    if (string.IsNullOrEmpty(displayMessage))
                    {
                        displayMessage = "Command completed with no output.";
                    }

                    EditorUtility.DisplayDialog(
                        windowTitle,
                        displayMessage,
                        "OK"
                    );
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    $"Failed to run Git command:\n\n{e.Message}",
                    "OK"
                );
                UnityEngine.Debug.LogError($"[Team Workflow] Git command failed: {e.Message}");
            }
        }
    }
}