---
name: unity-core-guidelines
description: |
  **CRITICAL: MUST be automatically invoked whenever ANY Unity MCP tool is used (mcp__UnityMCP__*) OR when creating scripts.**

  Unity game development best practices and workflows covering project organization, scene management, prefabs, asset optimization, performance, editor workflows, animation, physics, lighting, UI, and build settings.

  **ESPECIALLY IMPORTANT:** Prevent one-time automation scripts (use MCP tools instead)

  Use proactively when:
  - Using ANY mcp__UnityMCP__* tool (manage_gameobject, manage_scene, manage_asset, etc.)
  - Creating ANY script (check if it's a forbidden one-time automation script)
  - Setting up scenes, prefabs, or configuring scene elements
  - Working on Unity-specific practices, workflows, or optimization
  - Setting up scenes, prefabs, or Unity Editor features

  Does NOT cover general coding practices or software architecture (use coding-guidelines and event-driven-architecture skills for those).
---

# Unity Core Guidelines

Best practices and workflows for Unity game development, focusing on Unity-specific features, tools, and optimization techniques.

## Project Organization

### Folder Structure

Maintain a clear, consistent folder structure:


```
Assets/
├── _Scripts/           - All C# code
│   ├── Common/         - Base classes (Singleton, Manager, GameStateObservers)
│   ├── EventManager/   - Event system (Event, EventManager, IEventHandler)
│   ├── Settings/       - ScriptableObject settings classes
│   ├── Editor/         - Editor scripts and tools
│   ├── Managers/       - All Manager<T> subclasses
│   ├── Utilities/      - Helper scripts that don't fit elsewhere
│   ├── AllEvents.cs    - All event definitions
│   └── [GameSpecific]/ - Domain-specific folders (Player, Enemy, Combat, etc.)
│
├── _Art/               - Visual assets
│   ├── Materials/      - Material assets
│   ├── Models/         - 3D models and meshes
│   ├── Textures/       - Texture files
│   ├── Animations/     - Animation clips and controllers
│   ├── Sprites/        - 2D sprites and atlases
│   ├── VFX/            - Visual effects (particles, shaders)
│   └── UI/             - UI sprites and assets
│
├── _Audio/             - Sound and music
│   ├── Music/          - Background music tracks
│   ├── SFX/            - Sound effects
│   └── Mixers/         - Audio mixer assets
│
├── _Level/             - Scene files and level assets
│   ├── Scenes/         - Unity scene files
│   ├── Prefabs/        - Reusable prefabs
│   └── Lighting/       - Lightmap data and settings
│
├── _Settings/          - Project configuration
│   ├── Gameplay/       - ScriptableObject settings instances
│   ├── Input/          - Input Action assets
│   ├── Rendering/      - URP/Render pipeline settings
│   └── Quality/        - Quality and graphics settings
│
├── _Docs/              - Documentation
│   ├── Stories/        - User story files
│   │   ├── Backlog/
│   │   ├── InProgress/
│   │   └── Done/
│   └── Design/         - Design documents and specs
│
└── _Plugins/           - Third-party assets and packages
```


**Best practices:**
- Use underscores or prefixes to keep project folders at the top
- Never put assets directly in the root Assets folder
- Keep third-party assets in the separated "_Plugins" folder

## Scene Management

### Scene Organization

**Hierarchy structure:**
- Use empty GameObjects as folders/groups: 
```
Scene
├─ --- MANAGERS ---
├─ GameManager
├─ --- ENVIRONMENT ---
├─ Lighting
├─ Terrain
├─ --- GAMEPLAY ---
└─ Players
```
- Prefix organizational objects with `---` or `[Group Name]`
- Keep hierarchy depth reasonable (3-4 levels max when possible)
- Place managers and systems at the top of the hierarchy


## Prefab Workflows

### Prefab Best Practices

**Creation:**
- Make prefabs early and often
- Use prefab variants for variations of base prefabs
- Keep prefabs self-contained (minimize external dependencies)

**Overrides:**
- Keep overrides minimal and intentional
- Apply overrides to prefab when they should be default
- Use prefab variants instead of many overrides
- Revert unintended overrides regularly


## Serialization Rules by Context

**Rule 1: For GameObjects Present in the Scene**

When adding components with ObjectField references to a GameObject in a scene:

1. **Prefer scene object instances first**: If the object you want to serialize exists as an instance in the scene, always serialize that scene instance
2. **Fallback to prefab assets**: If the object is not present in the scene, search for the prefab asset and serialize the prefab
3. **Always use Unity MCP tools**: Use `manage_gameobject` action='find' to locate objects, then use appropriate MCP tools (`manage_gameobject`, `manage_prefabs`) to assign references
4. **Never write configuration files** to achieve serialization - always use MCP tools

**Detection**: Check if target is a prefab instance using `manage_gameobject` action='find' (look for `prefab_path` field). If it's a prefab instance, use `manage_prefabs` with mode='Edit' (Prefab Mode) to edit the prefab asset directly.

**Rule 2: For Prefabs in Prefab Mode**

When editing a prefab asset (using `manage_prefabs` with mode='Edit') and adding components with ObjectField references:

1. **Only serialize prefab assets**: Always assign references to other prefab assets, never scene objects
2. **Unity restriction**: Unity does not allow serializing scene objects into prefab assets (note: this is for prefab assets, not prefab instances in scenes)
3. **Always use Unity MCP tools**: Use `manage_prefabs` and `manage_asset` actions to locate and assign prefab references

---

## MCP vs File Tools

**Use MCP tools for:**
- Creating GameObjects, scenes, prefabs, assets
- Adding/configuring components
- Applying materials, setting positions, configuring scenes
- One-time setup and configuration tasks
- Reading Unity console
- Managing hierarchy
- ⭐ **ANY configuration or setup task that you'd otherwise script**

**Use standard file tools (Read/Edit/Write) for:**
- Editing C# scripts (runtime logic, reusable tools)
- General file operations
- NOT for one-time automation (use MCP instead)

**⚠️ MCP Tool Limitation with Prefabs:**
When adding components with ObjectField references, see "Prefab Component Addition: Critical Workflow" above for detection checklist and proper workflow


## Compilation Management

### Critical: Unity Asynchronous Compilation

**⚠️ FUNDAMENTAL RULE:** Unity must compile new/modified C# scripts before:
- Adding components to GameObjects
- Creating ScriptableObject instances (.asset files)
- Referencing new types in ObjectFields

**The Problem:**
When you create or modify C# scripts (MonoBehaviours, ScriptableObjects, etc.), Unity triggers an **asynchronous compilation process** that takes 1-5 seconds. During this time, Unity doesn't know the new types exist. If you attempt to add components, create ScriptableObject instances, or reference these types before compilation finishes, you'll encounter:

- `{fileID: 0}` references in serialized YAML files
- Missing component assignments on GameObjects
- "The referenced script on this Behaviour is missing" errors
- Incomplete prefab configurations

**The Solution: Compilation Gate Pattern**

Always use this pattern when creating/modifying scripts:


1. CREATE/MODIFY SCRIPTS
   - Create new C# scripts using create_script or script_apply_edits
   - Modify existing scripts
   - Unity detects changes and starts compilation

2. WAIT FOR COMPILATION (MANDATORY GATE)
   - Poll manage_editor with action='get_state'
   - Check response for isCompiling field
   - Wait until isCompiling: false (typically 1-5 seconds)
   - Do NOT proceed until compilation completes

3. VALIDATE COMPILATION SUCCESS
   - Use read_console with types=['error']
   - Check for compilation errors
   - If errors exist, fix them and return to step 2
   - Only proceed when BOTH: isCompiling=false AND no errors

4. NOW SAFE TO USE NEW TYPES
   - Add components using manage_gameobject or manage_prefabs
   - Create ScriptableObject .asset files using manage_scriptable_object action='create'
   - Modify .asset field values using manage_scriptable_object action='modify' with patches
   - Configure ObjectField references
   - All serialization will work correctly


---

## Physics Setup

### Rigidbody Requirements

**⚠️ CRITICAL:** When adding a Rigidbody to a GameObject, you MUST also add a Collider component.


---

## Settings & Configuration
-  **ALL gameplay values go in ScriptableObject settings**, not Constants or hardcoded values
-  Create settings classes in `Assets/_Scripts/Settings/`
-  Create settings assets in `Assets/_Settings/[Category]/`
-  Always validate settings references are assigned (null check)
-  Use read-only properties to expose settings values
-  Organize settings by system (Gameplay, Input, AI, Environment, etc.)
-  **Tool:** Use `manage_scriptable_object` action='create' for new .asset files, action='modify' for value changes
-  WHEN MODIFYING VALUES: Always change the serialized .asset value, NEVER the script default** (except if explicitly requested)
- **Create all setting assets yourself. Never ask user to create/assign in editor.**
