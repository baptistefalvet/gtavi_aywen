using GD3.Events;

// ============================================================================
// ALL EVENTS FOR THIS PROJECT
// ============================================================================
// This file contains all event definitions for the project.
// Use #region to organize events by system/manager.
// ============================================================================

#region GameManager Events
public class GameMenuEvent : GD3.Events.Event { }
public class GamePlayEvent : GD3.Events.Event { }
public class GamePauseEvent : GD3.Events.Event { }
public class GameResumeEvent : GD3.Events.Event { }
public class GameOverEvent : GD3.Events.Event { }
public class GameVictoryEvent : GD3.Events.Event { }

public class GameStatisticsChangedEvent : GD3.Events.Event {
    public int eBestScore { get; set; }
    public int eScore { get; set; }
    public int eNLives { get; set; }
}
#endregion

#region MenuManager Events
public class PlayButtonClickedEvent : GD3.Events.Event { }
public class PauseButtonClickedEvent : GD3.Events.Event { }
#endregion

// ============================================================================
// ADD YOUR CUSTOM EVENTS BELOW
// ============================================================================

#region Player Events
// Example:
// public class PlayerHasBeenHitEvent : GD3.Events.Event { }
// public class PlayerDiedEvent : GD3.Events.Event {
//     public int eRemainingLives { get; set; }
// }
#endregion

#region Enemy Events
// Example:
// public class EnemyHasBeenDestroyedEvent : GD3.Events.Event {
//     public Enemy eEnemy;
//     public bool eDestroyedByPlayer;
// }
#endregion

#region Score Events
// Example:
// public class ScoreItemEvent : GD3.Events.Event {
//     public IScore eScore;
// }
#endregion
