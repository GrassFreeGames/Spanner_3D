# Team Workflow Rules - Spanner_3D

## Prefab Editing Rules

✅ **Always edit in Prefab Mode** (double-click prefab in Project window)  
✅ **NEVER apply overrides from scene to prefab**  
✅ **Test changes before committing** (Play mode)  
✅ **Only edit prefabs you own** (see PREFAB_OWNERS.md)  
✅ **Commit and push immediately after changes**

## Scene Editing Rules

✅ **Only scene owners can edit scenes**  
✅ **Announce in team chat before editing**  
✅ **Pull before editing:** `git pull`  
✅ **Push immediately after:** `git push`

## Git Workflow

✅ **Always pull before starting work:** `git pull`  
✅ **Work in feature branches for major changes**  
✅ **Commit frequently** (every 30-60 min)  
✅ **Push immediately after committing**  
✅ **Write descriptive commit messages**

## Daily Workflow Checklist

### Starting Work
```bash
git pull
# Open Unity
# Announce what you're working on in team chat
```

### During Work
- Save frequently (Ctrl+S)
- Edit prefabs in Prefab Mode only
- Commit every 30-60 minutes
- Test changes in Play mode

### Ending Work
```bash
# In Unity: File → Save Project (Ctrl+S)
# Close Unity
git status
git add .
git commit -m "Descriptive message of what you did"
git push
```

## Common Scenarios

### "I need to edit someone else's prefab"
1. Ask the owner first
2. If urgent, create a branch: `git checkout -b fix/prefab-name`
3. Make changes and create Pull Request
4. Owner reviews and merges

### "I got a merge conflict"
1. **DON'T panic**
2. Ask Stu (QA Lead) for help immediately
3. Usually: pick one version, manually re-add changes in Unity

### "Git hook is blocking my commit"
1. Read the checklist carefully
2. Make sure you followed all the rules
3. If you really need to bypass (EMERGENCY ONLY): `git commit --no-verify -m "message"`

## Questions?

Ask Stu (QA Lead) in team chat or Discord

---
Last updated: November 11, 2025  
By: Stu (QA Lead)