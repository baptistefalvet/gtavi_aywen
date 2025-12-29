---
model: claude-sonnet-4-5-20250929
---

# Debug Feature

Debug a Unity feature by systematically verifying console, scene setup, components, events, and runtime behavior.

**Usage**:
- `/debug-feature` - Interactive mode (lists all stories/features)
- `/debug-feature story-NNN` - Debug by story number
- `/debug-feature "feature name"` - Debug by fuzzy name match

---

## Instructions

Load required skills or reload if already loaded:
- `coding-guidelines`
- `event-driven-architecture`
- `unity-core-guidelines`


### 1. Identify Feature

If argument provided:
- Search `Assets/_Docs/Stories/` for matches (story number or fuzzy title match)
- If multiple matches, ask user to choose
- If no match, list all stories and ask user to choose

If no argument:
- List all stories from InProgress/ and Done/
- Ask: "Which feature would you like to debug?"

### 2. Console Error Check

Use `read_console` with `types=['error']`:
- If errors exist: Report and fix them before proceeding
- If no errors: Proceed to next step

### 3. Scene Setup Verification

Identify the main scene for this feature, then use `manage_scene` action='get_hierarchy':
- Verify GameObject hierarchy organization
- Check all GameObjects have required components
- Validate all serialized fields are assigned (not empty/null)
- Report any missing or misconfigured elements

### 4. Component References Validation

For each script involved in the feature:
- Read script to identify public/serialized reference fields
- Use `manage_gameobject` action='get_components' to verify field assignments
- Report any null or unassigned ObjectField references

### 5. Event Subscriptions Check

Per event-driven architecture pattern:
- Verify OnEnable subscribes to events
- Verify OnDisable unsubscribes from events
- Check event names match `AllEvents.cs` definitions
- Validate Manager/Observer separation

### 6. Settings Assets Verification

If feature uses ScriptableObject settings:
- Use `manage_asset` action='get_info' on settings .asset files (verification only)
- To fix invalid values: Use `manage_scriptable_object` action='modify' with patches
- Verify all fields contain valid data
- Report any default/uninitialized values

### 7. Play Mode Test

Use `manage_editor` action='play' to enter play mode:
- Monitor console output during feature execution
- Use `read_console` to capture runtime errors/warnings
- Use `manage_editor` action='stop' to exit play mode
- Report any runtime issues discovered

### 8. Prefab Integrity Check

If feature uses prefabs:
- Use `manage_prefabs` action='get_components' on affected prefabs
- Verify no broken component references
- Check for invalid prefab overrides
- Report prefab-specific issues

### 9. Debug Report

Summarize findings:
- Issues found and fixed
- Warnings or potential problems
- Recommendations for improvements
- Feature status (working/needs attention)

---

## Rules

**Do**:
- Fix errors immediately when found
- Report findings after each step
- Follow event-driven architecture patterns
- Verify both editor and runtime state

**Don't**:
- Skip steps even if feature appears working
- Proceed past console errors without fixing
- Modify feature behavior unless fixing bugs
- Create new features or enhancements
