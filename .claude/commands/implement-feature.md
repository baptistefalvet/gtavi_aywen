---
model: claude-sonnet-4-5-20250929
---

# Implement Feature from User Story

Implement the technical plan from a user story.

Usage: `/implement-feature {story-number}` or `/implement-feature story-{NNN}-name`

---

## Stage 1: Load Skills

Load required skills:
- `coding-guidelines`
- `event-driven-architecture`
- `unity-core-guidelines`

---

## Stage 2: Locate Story

Search `Assets/_Docs/Stories/` for `story-{number}-*.md` across all subdirectories.

**If not found**: List available stories and STOP.
**If multiple found**: Ask user to select and STOP.
**If found**: Read and parse story file (Story statement, Acceptance Criteria, Technical Plan, Implementation Tasks).

**If no technical plan**: Ask user if they want to run `/plan-feature {story-number}` first or proceed with Acceptance Criteria only.

---

## Stage 3: Implementation Loop

For each task in the Technical Plan (or based on Acceptance Criteria if no plan):

### 3.1: Implement Task

**CRITICAL: This stage has two phases to handle Unity's asynchronous compilation**

#### Phase A: Create/Modify Scripts
- Update task status to "In Progress" with timestamp
- Move the user story file to the "In Progress" folder
- Write code following loaded skill guidelines (create or modify C# scripts)

#### Phase B: Compilation Gate (MANDATORY)
**Unity must compile new/modified scripts before components can be added to GameObjects.**

1. **Wait for compilation to complete:**
   - Use `manage_editor` with `action='get_state'` to check compilation status
   - Poll every 2-3 seconds until the response shows `isCompiling: false`
   - Do NOT proceed until compilation is complete

2. **Validate compilation success:**
   - Use `read_console` with `types=['error']` to check for compilation errors
   - If errors exist, fix them immediately and return to step 1
   - Only proceed when BOTH conditions are met: `isCompiling: false` AND no errors

3. **Example polling pattern:**
   ```
   Loop until success:
     1. Call manage_editor action='get_state'
     2. Check if isCompiling: false
     3. If true, call read_console types=['error']
     4. If no errors, break loop and proceed to Phase C
     5. If errors exist, fix them (triggers new compilation, return to step 1)
     6. If still compiling, wait 2-3 seconds and repeat from step 1
   ```

#### Phase C: Add/Configure Components (SAFE AFTER COMPILATION)
**Now Unity knows about new component types - safe to proceed:**
- Add components to GameObjects using `manage_gameobject` or `manage_prefabs`
- Configure component properties and ObjectField references
- Create/modify .asset files if needed
- Validate final state using `read_console`

### 3.2: Complete Task
- Update task status to "Completed" with timestamp
- Move to next task

---

## Stage 4: Final Validation

### 4.2: Update Story Progress
- Add completion notes with date, task count, and test results

### 4.3: Pre-Completion Verification Checklist

**Before marking implementation complete, verify all items:**

- [ ] All .asset files created with proper YAML format (not requested from user)
- [ ] All .asset.meta files created with unique GUIDs
- [ ] All inspector fields assigned (no `{fileID: 0}` references remaining)
- [ ] No "TODO: Assign X in inspector" comments left in code or documentation
- [ ] No setup guides created as substitutes for full implementation
- [ ] Console has no errors or critical warnings

**If any checklist item fails:** Fix it before proceeding to next step.

### 4.4: Display Summary
- Story name and number
- Tasks completed
- Files modified/created
- Any known limitations or future work

