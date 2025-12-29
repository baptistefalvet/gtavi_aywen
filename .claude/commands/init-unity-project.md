---
model: claude-sonnet-4-5-20250929
---

# Initialize Unity Project

You are helping initialize a Unity project with the proper folder structure and scene GameObject hierarchy.

## Workflow

1. **Load Unity guidelines skills** to understand the project standards:
   - Load the `unity-core-guidelines` skill for folder structure and scene management

2. **Create folder structure** by examining the guidelines:
   - Identify the complete folder structure from `unity-core-guidelines`
   - Create all standard Unity folders under the Assets directory
   - Skip any folders that already exist
   - Report which folders were created and which already existed

3. **Setup scene GameObject hierarchy**:
   - Identify the standard scene hierarchy from `unity-core-guidelines`
   - Create the necessary GameObjects in the currently open Unity scene
   - Skip any GameObjects that already exist
   - Report which GameObjects were created and which already existed

4. **Verify setup**:
   - List all folders created
   - List all GameObjects created
   - Provide a summary of the initialization

## Important Rules

- ✅ Load `unity-core-guidelines` skill at the start to get current guidelines
- ✅ Create ALL standard folders from unity-core-guidelines
- ✅ Skip items that already exist (folders or GameObjects)
- ✅ Follow naming conventions from coding-guidelines
- ✅ Report clearly what was created vs what already existed
- ✅ Use MCP Unity tools if available for scene operations
- ❌ Never overwrite existing folders or GameObjects
- ❌ Don't hardcode folder names - read them from the skill
- ❌ Don't create any C# scripts yet - only structure
- ❌ Don't create a new scene - work with the current one

## Notes

This command focuses on project initialization only. For creating scripts, managers, or other code files, use other commands or direct requests after the project structure is set up.
