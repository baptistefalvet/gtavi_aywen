---
model: claude-sonnet-4-5-20250929
---

# Plan Feature Implementation

Create a high-level technical implementation plan for a user story.

**Usage**:
- `/plan-feature` - Interactive mode (lists all stories)
- `/plan-feature story-NNN` - Plan by story number
- `/plan-feature "feature name"` - Plan by fuzzy name match

---

## Instructions

Load the `event-driven-architecture`, `coding-guidelines` and `unity-core-guidelines` skills.

### 1. Find the Story

If argument provided:
- Search `Assets/_Docs/Stories/` for matches (story number or fuzzy title match)
- If multiple matches, ask user to choose
- If no match, list all stories and ask user to choose

If no argument:
- List all stories from Backlog/, InProgress/, Done/
- Ask: "Which story would you like to plan?"

### 2. Read & Confirm Story

Read the selected story file and display:
- Story title
- All acceptance criteria (ACs)

Ask: "Ready to create a technical plan for this story?"

### 3. Discovery & Reconnaissance

**BEFORE creating any plan, search for existing assets that might be reused:**

- Search the project for related files
- Read through related code.
- Analyze the game scene and the hierarchy of game objects affected by the feature.
- Do not write any code right now.

Consider the user story. Conduct review, read relevant
files for the project and prepare to proceed. ultrathink.

**Document findings in the plan:**
- ✅ What assets/scripts already exist (will reuse/extend)
- ✅ What needs to be created from scratch
- ✅ What existing GameObjects need components added
- ✅ What existing scripts need modifications

### 4. Create the Plan

**How will we implement only the functionality we need right now?**

ultrathink and make a plan to accomplish this.

Plan with a macro view. Don't go in the details of the implementation. 

Identify files that need to be changed.

Write a short overview of what you are about to do.

Write function names and 1 sentence about what they do.

Write gameobjects and components you plan to create or modify. Write bullet points about how you are going to modify them.

Do not include plans for legacy fallback unless required or explicitly requested.

### 5. Save the Plan

Append the plan to the user story file itself (no separate plan file needed).

Add to the end of `Assets/_Docs/Stories/[Status]/story-NNN-feature-name.md`:


### 6. Next Steps

Display:
```
✅ Plan appended to story-NNN-feature-name.md
```

Ask the user: "Would you like me to run `/implement-feature story-NNN` to build it now?"

If yes, run the command. If no, stop here.

---

## Rules

**Do**:
- Load architecture skill, coding skill, and unity-core skill first
- Think deeply about implementation approach
- Follow event-driven patterns (Manager/Observer separation)
- Keep overview short and clear

**Don't**:
- Create a separate plan file (append to the story file)
- Mix Manager and Observer responsibilities
- Plan for features beyond current ACs
- Overwrite existing story content (append only)
- Describe algorithms or classes implementation in detail
