---
model: sonnet
---

# Commit and Push

Auto-commit and push changes to git using SSH.

## Workflow

1. **Auto-convert HTTPS to SSH** - if HTTPS remote detected, automatically convert to SSH
2. **Check status & diff** - analyze staged/unstaged changes in the current branch
3. **Auto-generate commit message** (imperative mood, specific, with context)
4. **Show user** what will be committed + generated message for review
5. **Stage all**: `git add .`
6. **Commit** with message
7. **Push** to remote
8. **Confirm success**

## Commit Message Format
```
<Short summary line>

<Detailed description with bullet points if multiple changes>
```

## Rules
- ✅ Auto-convert HTTPS remote to SSH
- ✅ Show files + message before committing
- ✅ Never skip hooks, never force push
- ❌ Don't commit if no changes

