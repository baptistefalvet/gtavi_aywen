---
name: coding-guidelines
description: Unity C# coding standards for naming conventions, code organization, regions, lifecycle methods, and ScriptableObject settings pattern. **MUST be automatically applied to ALL Unity C# script creation and editing operations.** Use proactively whenever creating or modifying any C# script file to ensure consistent code structure, proper naming conventions, and Unity best practices.
---

# Coding Guidelines

Apply project-agnostic Unity C# coding standards to all scripts. **For architecture patterns, use `event-driven-architecture` skill.**

## Naming Conventions

### Variables & Fields

| Type | Convention | Example |
|------|------------|---------|
| Private/protected fields | `m_` prefix | `private int m_Score;` |
| SerializeField | `[SerializeField] private` + `m_` prefix | `[SerializeField] private float m_Speed;` |
| Static singleton | `_instance` | `private static GameManager _instance;` |
| Public properties | PascalCase, no prefix | `public int Score { get; set; }` |
| Local variables | camelCase | `int currentScore = 0;` |
| Method parameters | camelCase | `void SetScore(int newScore)` |
| Constants | SCREAMING_SNAKE_CASE | `public const int MAX_LIVES = 3;` |

### Methods & Types

| Type | Convention | Example |
|------|------------|---------|
| Methods | PascalCase | `void PlayMusic()` |
| Event callbacks | `On` + PascalCase | `void OnPlayerDied()` |
| Classes | PascalCase | `public class GameManager` |
| Interfaces | `I` + PascalCase | `public interface IEventHandler` |

### Files

- One class per file, filename matches class name (`GameManager.cs`)
- Exception: `AllEvents.cs` contains all event definitions

## Namespace Conventions

**All scripts must use a single flat namespace following the pattern `GD3.{ProjectName}`:**

- Format: `namespace GD3.VibyTank` (where `VibyTank` is the Unity project name)
- No sub-namespaces allowed (e.g., no `GD3.VibyTank.Tank` or `GD3.VibyTank.Enemy`)
- Apply to ALL new scripts created for this project
- Existing scripts may use legacy namespaces but will be migrated over time


## Unity Lifecycle Method Order

### Initialization Guidelines

- **Awake()**: Cache components using `GetComponent`, initialize local state
- **Start()**: Resolve dependencies after all `Awake()` calls complete
- **OnEnable()**: Subscribe to events, reset state when re-enabled
- **OnDisable()**: Unsubscribe from events
- **OnDestroy()**: Perform final cleanup (destroy materials, unsubscribe static events)


## Best Practices

Apply these essential rules:
- Use `[SerializeField] private` for inspector fields, never public fields
- Use property getters for read-only public access instead of public fields
- Avoid backwards compatibility unless explicitly requested
- Group related code by feature or responsibility using regions.
- Never use the "Resources" folder

## ScriptableObject Settings Pattern

**CRITICAL: Store ALL configurable gameplay values in ScriptableObject assets.**

**Benefits:**
- Designers modify values without touching code
- Easy difficulty presets (Easy/Normal/Hard .asset files)
- Hot-reload during Play mode
- Version control tracks setting changes

**CRITICAL: Modifying Settings Values**

When user requests to change a setting value:
- Modify the `.asset` file (e.g., `Assets/_Settings/Gameplay/GameplaySettings.asset`)
- **Tool:** Use `manage_scriptable_object` action='modify' with target={path} and patches=[{property,value}]
- DO NOT modify `.cs` script defaults unless explicitly requested
- Reason: `.asset` files are used at runtime; script defaults are fallbacks only

