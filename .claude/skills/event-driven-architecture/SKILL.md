---
name: event-driven-architecture
description: "Unity C# event-driven architecture with EventManager pub/sub system. Auto-applies to all Unity C# script creation/editing. Enforces event-based communication, Manager/Observer inheritance, and prevents direct component coupling."
---

# Unity Event-Driven Architecture

This skill defines the event-driven architecture for Unity C# projects using the Observer pattern with a centralized EventManager.

---

## Core Principle

**ALL communication between systems happens through events via EventManager. NEVER use direct references between managers or components.**

**Philosophy**: Components communicate through events rather than direct references, creating loose coupling and making the codebase scalable, maintainable, and testable.

---

## Quick Start

### 1. Choose the Right Base Class

| Base Class | Use When | Key Features |
|------------|----------|--------------|
| **Singleton\<T\>** | Need singleton without events | Singleton pattern, optional DontDestroyOnLoad |
| **SimpleGameStateObserver** | Regular GameObject reacts to game state | Auto-subscribes to 6 game state events |
| **SingletonGameStateObserver\<T\>** | Singleton reacts to game state, no async init | Singleton + game state events |
| **Manager\<T\>** | Manager requires async initialization | Singleton + game state events + IsReady + InitCoroutine() |

**Inheritance Hierarchy:**
```
MonoBehaviour
    ├─── Singleton<T>
    │       └─── SingletonGameStateObserver<T>
    │               └─── Manager<T>
    └─── SimpleGameStateObserver
```

### 2. Implementation Steps

**Step 1: Understand Base Classes**
- Base classes already exist at `Assets/_Scripts/Common/`
- These include: Event.cs, IEventHandler.cs, EventManager.cs, Singleton.cs, SimpleGameStateObserver.cs, SingletonGameStateObserver.cs, Manager.cs
- Review these files to understand the architecture foundation

**Step 2: Create AllEvents.cs**
- Use `assets/templates/AllEvents.template.cs` as starting point
- Create at `Assets/_Scripts/AllEvents.cs`
- Add your custom events to this single centralized file

**Step 3: Create Managers**
- Use `assets/templates/ManagerTemplate.cs` as starting point
- Inherit from `Manager<T>` for systems requiring async initialization
- Organize in `Assets/_Scripts/Managers/` folder

### 3. File Locations Reference

**Core Framework** (Already in project):
- Event base class → `Assets/_Scripts/Common/EventManager/Event.cs`
- IEventHandler interface → `Assets/_Scripts/Common/EventManager/IEventHandler.cs`
- EventManager → `Assets/_Scripts/Common/EventManager/EventManager.cs`
- Base classes → `Assets/_Scripts/Common/` (Singleton.cs, SimpleGameStateObserver.cs, SingletonGameStateObserver.cs, Manager.cs)

**Your Code**:
- **All event definitions** → `Assets/_Scripts/AllEvents.cs` (CRITICAL: centralized)
- **All managers** → `Assets/_Scripts/Managers/`
- **Domain-specific code** → `Assets/_Scripts/[Domain]/` (e.g., Player/, Enemy/, Combat/)

---

## Event System

### Core Game State Events (Pre-configured)

All GameStateObserver classes automatically subscribe to these events:
- **GameMenuEvent** - Game enters menu state
- **GamePlayEvent** - Active gameplay begins
- **GamePauseEvent** - Game is paused
- **GameResumeEvent** - Game resumes from pause
- **GameOverEvent** - Game ends in failure
- **GameVictoryEvent** - Game ends in victory

### Adding New Events

**CRITICAL RULE: ALL events MUST be declared in `Assets/_Scripts/AllEvents.cs`**

Events are simple data carriers that inherit from `Event` base class:

```csharp
// Simple event (no data)
public class CoinCollectedEvent : Event { }

// Event with data (constructor)
public class PlayerDamagedEvent : Event
{
    public int Damage;
    public GameObject Attacker;

    public PlayerDamagedEvent(int damage, GameObject attacker)
    {
        Damage = damage;
        Attacker = attacker;
    }
}

// Event with data (properties)
public class GameStatisticsChangedEvent : Event
{
    public int Score { get; set; }
    public int Lives { get; set; }
}
```

### Event Naming Rules

1. **Completed Actions**: Past tense + "Event"
   - `PlayerHasBeenHitEvent`, `EnemyHasBeenDestroyedEvent`, `BombHasBeenCollectedEvent`

2. **Commands/Requests**: Present tense + "Event"
   - `PlayButtonClickedEvent`, `GoToNextLevelEvent`, `AskToGoToNextLevelEvent`

3. **State Changes**: Descriptive name + "Event"
   - `GameStatisticsChangedEvent`, `BombPointsForPowerCoinsChangedEvent`

4. **Event Properties**: Use **e prefix**
   ```csharp
   public class EnemyHasBeenDestroyedEvent : Event {
       public Enemy eEnemy;
       public bool eDestroyedByPlayer;
   }
   ```

### AllEvents.cs Organization

Use `#region` to group events by system:

```csharp
using GD3.Events;

#region GameManager Events
public class GameMenuEvent : GD3.Events.Event { }
public class GamePlayEvent : GD3.Events.Event { }
// ... other game state events
#endregion

#region MenuManager Events
public class PlayButtonClickedEvent : GD3.Events.Event { }
#endregion

#region Player Events
public class PlayerHasBeenHitEvent : GD3.Events.Event { }
#endregion
```

See `assets/templates/AllEvents.template.cs` for a complete starting template.

---

## Working with Events

### Subscribing to Events

**In Manager classes** (override SubscribeEvents):

```csharp
public class GameManager : Manager<GameManager> {
    #region Events' subscription
    public override void SubscribeEvents() {
        base.SubscribeEvents();  // ← Get game state events

        // Subscribe to custom events
        EventManager.Instance.AddListener<PlayerHasBeenHitEvent>(PlayerHasBeenHit);
        EventManager.Instance.AddListener<ScoreItemEvent>(ScoreHasBeenGained);
    }

    public override void UnsubscribeEvents() {
        base.UnsubscribeEvents();

        EventManager.Instance.RemoveListener<PlayerHasBeenHitEvent>(PlayerHasBeenHit);
        EventManager.Instance.RemoveListener<ScoreItemEvent>(ScoreHasBeenGained);
    }
    #endregion

    #region Callbacks to Player events
    private void PlayerHasBeenHit(PlayerHasBeenHitEvent e) {
        DecrementNLives(1);
        if (m_NLives == 0) GameOver();
    }
    #endregion
}
```

**In regular classes** (implement IEventHandler):

```csharp
public class Level : MonoBehaviour, IEventHandler {
    public void SubscribeEvents() {
        EventManager.Instance.AddListener<BombHasBeenCollectedEvent>(BombHasBeenCollected);
    }

    public void UnsubscribeEvents() {
        EventManager.Instance.RemoveListener<BombHasBeenCollectedEvent>(BombHasBeenCollected);
    }

    private void Awake() {
        SubscribeEvents();
    }

    private void OnDestroy() {
        UnsubscribeEvents();  // CRITICAL: Always unsubscribe!
    }

    private void BombHasBeenCollected(BombHasBeenCollectedEvent e) {
        // Handle event...
    }
}
```

### Raising Events

```csharp
// Simple event
EventManager.Instance.Raise(new GameOverEvent());

// Event with data
EventManager.Instance.Raise(new GameStatisticsChangedEvent() {
    eBestScore = BestScore,
    eScore = m_Score,
    eNLives = m_NLives
});

// Event with object reference
EventManager.Instance.Raise(new EnemyHasBeenDestroyedEvent() {
    eEnemy = this,
    eDestroyedByPlayer = true
});
```

---

## Manager Pattern

### When to Create a Manager

Create a Manager when you need a **singleton system** that:
- Manages a specific game domain (UI, audio, levels, game state)
- Needs to exist only once in the scene
- Responds to game state changes
- Requires global access from other systems

### Manager Responsibilities

Each manager should have a **single, clear responsibility**:

- **GameManager**: Game state, score, lives, victory/defeat conditions
- **MenuManager**: UI panel visibility and navigation
- **HudManager**: Display updates (score, lives, etc.)
- **LevelsManager**: Level instantiation and progression
- **SfxManager**: Sound effect playback
- **MusicLoopsManager**: Background music with fade-in/fade-out

### Creating a Manager

Use `assets/templates/ManagerTemplate.cs` as your starting point. Key pattern:

```csharp
public class ExampleManager : Manager<ExampleManager> {
    #region Events' subscription
    public override void SubscribeEvents() {
        base.SubscribeEvents();  // Always call base first
        // Add custom subscriptions
    }

    public override void UnsubscribeEvents() {
        base.UnsubscribeEvents();  // Always call base first
        // Remove custom subscriptions
    }
    #endregion

    #region Manager implementation
    protected override IEnumerator InitCoroutine() {
        // Async initialization (load resources, etc.)
        yield break;
    }
    #endregion

    #region GameStateObserver overrides
    protected override void GamePlay(GamePlayEvent e) {
        // React to game state changes
    }
    #endregion
}
```

See `references/complete-examples.md` for a fully-implemented GameManager example.

---

## Folder Structure

### Core Structure (Required)

Every Unity project using this architecture must have:

```
Assets/_Scripts/
├── Common/              - Base classes (already in project)
├── EventManager/        - Event system (Event, EventManager, IEventHandler)
├── Settings/            - ScriptableObject settings and configuration
├── Editor/              - Editor scripts and tools
├── AllEvents.cs         - All event definitions (CRITICAL)
```

**Do not modify or reorganize these core folders.** They are reusable across projects.

### Organization Rules

**Rule 1: Create domain-specific folders**
- ✅ `Player/`, `Enemy/`, `Combat/`, `Inventory/`, `Abilities/`
- ❌ Don't create `Scripts/`, `Classes/`, `Interfaces/`

**Rule 2: Managers go in dedicated folder**
```
Managers/
├── AudioManager.cs
├── UIManager.cs
├── GameManager.cs
└── LevelManager.cs
```

**Rule 3: Create folders when needed**
- ❌ Don't create folder for 1-2 scripts
- ✅ Create folder when you have 3+ related scripts

**Rule 4: Utilities for miscellaneous**
```
Utilities/
├── Constants.cs
├── ExtensionMethods.cs
└── EditorTools.cs
```

**Rule 5: Avoid deep nesting**
- ✅ Maximum 2-3 levels: `Assets/_Scripts/Player/Abilities/`
- ❌ Don't create: `Assets/_Scripts/Systems/Player/Abilities/Attacks/`

### Design Principles

- **AllEvents.cs stays at root** - Single source of truth, easy to find
- **Settings/ is centralized** - All configuration in one place
- **Common/ is framework-level** - Only base classes that work in any project
- **Each domain folder is independent** - Minimal cross-folder dependencies
- **Use events, not folder imports** - Systems communicate via EventManager

---

## Architecture Patterns

### Event-Driven Architecture

**Key Components**:
- **EventManager**: Central publish-subscribe system
- **Event classes**: Data containers for event information
- **IEventHandler**: Interface for objects that subscribe to events

**Benefits**:
- Components don't need references to each other
- Easy to add/remove listeners without modifying publishers
- Clear separation of concerns
- Highly testable and modular

### Singleton Pattern

**Usage**: Manager classes that should have only one instance and need global access.

**Implementation**: Generic `Singleton<T>` base class (see `Assets/_Scripts/Common/Singleton.cs`) with:
- Static `Instance` property
- Automatic instance management
- Optional DontDestroyOnLoad support

### Observer Pattern (Game State)

**Usage**: Objects that need to react to game state changes (menu, play, pause, etc.)

**Implementation**:
- `SimpleGameStateObserver`: For non-singleton objects (players, enemies)
- `SingletonGameStateObserver<T>`: For singleton managers

**Benefits**: Automatic subscription to core game state events with clean override pattern.

### Manager Hierarchy

Three-level inheritance hierarchy for game systems:

```
MonoBehaviour → Singleton<T> → SingletonGameStateObserver<T> → Manager<T>
```

Each level adds specific functionality:
1. **Singleton**: Single instance pattern
2. **GameStateObserver**: Automatic game state event subscription
3. **Manager**: Async initialization with IsReady flag

---

## Validation Checklist

When writing Unity code following this architecture, ensure:

### Architecture
- ✅ Use EventManager for component communication
- ✅ Managers inherit from `Manager<T>`
- ✅ GameObjects that observe game state inherit from `SimpleGameStateObserver`
- ✅ Define all events in `AllEvents.cs`

### Events
- ✅ Past tense for completed actions
- ✅ Properties with `e` prefix
- ✅ Organized in regions by system
- ✅ Always unsubscribe in OnDestroy

### Best Practices
- ✅ Never use direct GameObject references between systems
- ✅ Always communicate via EventManager
- ✅ Check `GameManager.Instance.IsPlaying` before game logic in Update/FixedUpdate
- ✅ Unsubscribe events in OnDestroy() to prevent memory leaks

See `references/common-mistakes.md` for anti-patterns to avoid.

---

## Bundled Resources Reference

### assets/templates/
Boilerplate templates for new code:
- `ManagerTemplate.cs` - Starting point for new managers
- `AllEvents.template.cs` - Starting template for events file

### references/
Detailed documentation loaded as needed:
- `complete-examples.md` - Full GameManager and Bomb implementation examples
- `common-mistakes.md` - Anti-patterns and how to avoid them

### Base Classes (in project)
Core framework classes at `Assets/_Scripts/Common/`:
- `Event.cs` - Base event class
- `IEventHandler.cs` - Event handler interface
- `EventManager.cs` - Complete pub/sub system
- `Singleton.cs` - Singleton pattern
- `SimpleGameStateObserver.cs` - Non-singleton observer
- `SingletonGameStateObserver.cs` - Singleton observer
- `Manager.cs` - Manager base class with async init

---

**Key Principle**: Components communicate through events, never through direct references. This creates a scalable, maintainable, and testable codebase.
