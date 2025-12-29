using System.Collections;
using UnityEngine;
using GD3.Events;

/// <summary>
/// [Description of manager's responsibility]
/// </summary>
public class ExampleManager : Manager<ExampleManager> {

    [Header("ExampleManager")]
    [SerializeField] private float m_SomeValue;

    #region Manager-specific state
    private bool m_IsActive;
    #endregion

    #region Events' subscription
    public override void SubscribeEvents() {
        base.SubscribeEvents();

        // Subscribe to domain-specific events
        EventManager.Instance.AddListener<SomeEvent>(OnSomeEvent);
    }

    public override void UnsubscribeEvents() {
        base.UnsubscribeEvents();

        // Unsubscribe from domain-specific events
        EventManager.Instance.RemoveListener<SomeEvent>(OnSomeEvent);
    }
    #endregion

    #region Manager implementation
    protected override IEnumerator InitCoroutine() {
        // Async initialization logic here
        // Can yield for resources to load, etc.
        yield break;
    }
    #endregion

    #region Callbacks to [Source] events
    private void OnSomeEvent(SomeEvent e) {
        // Handle event...
    }
    #endregion

    #region GameStateObserver overrides
    protected override void GameMenu(GameMenuEvent e) {
        // Reset state when returning to menu
    }

    protected override void GamePlay(GamePlayEvent e) {
        // Initialize for gameplay
    }
    #endregion

    #region Public API
    public void DoSomething() {
        // Public methods for other systems to call
    }
    #endregion
}
