---
model: claude-sonnet-4-5-20250929
---

# Create User Story from GDD

Transform a GDD feature into an implementable user story.

**Usage**: `/create-user-story ["Feature Name"]`

---

## Workflow

### 1. Feature Selection
- **With argument**: Fuzzy-match feature name in `Assets/_Docs/GDD/`
- **Without argument**: List all GDD features and ask user to select
- **Multiple matches**: Display options and ask user to select

### 2. Extract & Review
Read GDD section and extract:
- Feature description, mechanics, player interactions, behaviors

Display extracted info and ask: "Any missing information before creating the story?"

Suggest user to prompt "go" to continue. 

### 3. Generate Story File

**Numbering**: Find highest `story-NNN-*.md` in `Assets/_Docs/Stories/` (all subdirs), increment by 1

**Filename**: `story-NNN-feature-name.md` (kebab-case)

**Template**:
```markdown
# Story {StoryNum}: {Feature Name}

## Status: Draft

## Story
- As a [role]
- I want [action]
- so that [benefit]

## Acceptance Criteria (ACs)
{3-7 numbered, testable criteria}

## Story Progress Notes
### Agent Model Used: `<Model Name/Version>`
### Completion Notes List
{Notes}
### Change Log
```

**Save to**: `Assets/_Docs/Stories/Backlog/`

### 4. Summary & Next Steps
Display:
- ✅ Story created: `story-NNN-feature-name.md`
- 📋 Story number, feature, status, GDD source
- Complete story content

Ask: "Would you like me to run `/plan-feature story-NNN` to build it now?"

---

## Guidelines
- Use fuzzy matching (partial, case-insensitive)
- Write stories in plain language, non-technical
- 3-7 specific, testable acceptance criteria
- Focus on observable behaviors, not implementation
- Reference source GDD section
- Next phase: `/plan-feature story-NNN`
