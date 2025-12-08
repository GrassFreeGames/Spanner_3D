Suggested Stream Deck Layout
┌─────────┬─────────┬─────────┬─────────┐
│ Start   │  View   │  Lock   │ Unlock  │
│  Day    │  Locks  │  File   │  File   │
├─────────┼─────────┼─────────┼─────────┤
│  Pull   │ Status  │ Commit  │  Push   │
├─────────┼─────────┼─────────┼─────────┤
│  Sync   │ Ship It │ End Day │ FORCE   │
│         │         │         │ UNLOCK  │
└─────────┴─────────┴─────────┴─────────┘

Pro Tips for Stream Deck

Color Code Your Buttons:

Green: Safe operations (Pull, Status, View Locks)
Blue: Standard workflow (Lock, Commit, Push)
Orange/Red: Dangerous (Force Unlock)


Use Folders: If you have limited buttons, create a "Git" folder button that opens a page with all these commands
Clipboard Workflow: Copy a file path from Explorer/your IDE, then hit the Lock/Unlock button
Multiple Projects: Create separate profiles for each game project with the correct paths
Add Sound Feedback: Stream Deck can play sounds on button press—useful confirmation that the script ran


Windows Setup Instructions

Create a folder: C:\Scripts\Git\
Save each script as a .bat file (e.g., view-locks.bat, lock-file.bat)
In Stream Deck software:

Drag "System → Open" to a button
Point to your .bat file
Set title and icon