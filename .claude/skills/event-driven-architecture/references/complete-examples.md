# Complete Implementation Examples

This file contains detailed, real-world implementation examples demonstrating the event-driven architecture in action.

---

## Example 1: GameManager (Complete Implementation)

Complete GameManager showing all patterns:

```csharp
using System.Collections;
using UnityEngine;
using GD3.Events;

public enum GameState { gameMenu, gamePlay, gamePause, gameOver, gameVictory }

public class GameManager : Manager<GameManager> {

    #region Game State
    private GameState m_GameState;
    public bool IsPlaying { get { return m_GameState == GameState.gamePlay; } }
    #endregion

    #region Lives
    [Header("GameManager")]
    [SerializeField] private int m_NStartLives;
    private int m_NLives;
    public int NLives { get { return m_NLives; } }

    void DecrementNLives(int decrement) {
        SetNLives(m_NLives - decrement);
    }

    void SetNLives(int nLives) {
        m_NLives = nLives;
        EventManager.Instance.Raise(new GameStatisticsChangedEvent() {
            eBestScore = BestScore,
            eScore = m_Score,
            eNLives = m_NLives
        });
    }
    #endregion

    #region Score
    private int m_Score;
    public int Score {
        get { return m_Score; }
        set {
            m_Score = value;
            BestScore = Mathf.Max(BestScore, value);
        }
    }

    public int BestScore {
        get { return PlayerPrefs.GetInt("BEST_SCORE", 0); }
        set { PlayerPrefs.SetInt("BEST_SCORE", value); }
    }

    void IncrementScore(int increment) {
        SetScore(m_Score + increment);
    }

    void SetScore(int score) {
        Score = score;
        EventManager.Instance.Raise(new GameStatisticsChangedEvent() {
            eBestScore = BestScore,
            eScore = m_Score,
            eNLives = m_NLives
        });
    }
    #endregion

    #region Events' subscription
    public override void SubscribeEvents() {
        base.SubscribeEvents();
        EventManager.Instance.AddListener<PlayButtonClickedEvent>(PlayButtonClicked);
        EventManager.Instance.AddListener<PlayerHasBeenHitEvent>(PlayerHasBeenHit);
        EventManager.Instance.AddListener<ScoreItemEvent>(ScoreHasBeenGained);
    }

    public override void UnsubscribeEvents() {
        base.UnsubscribeEvents();
        EventManager.Instance.RemoveListener<PlayButtonClickedEvent>(PlayButtonClicked);
        EventManager.Instance.RemoveListener<PlayerHasBeenHitEvent>(PlayerHasBeenHit);
        EventManager.Instance.RemoveListener<ScoreItemEvent>(ScoreHasBeenGained);
    }
    #endregion

    #region Manager implementation
    protected override IEnumerator InitCoroutine() {
        Menu();
        EventManager.Instance.Raise(new GameStatisticsChangedEvent() {
            eBestScore = BestScore,
            eScore = 0,
            eNLives = 0
        });
        yield break;
    }
    #endregion

    #region Game flow
    private void InitNewGame() {
        SetScore(0);
        SetNLives(m_NStartLives);
        m_GameState = GameState.gamePlay;
        EventManager.Instance.Raise(new GamePlayEvent());
    }

    private void Menu() {
        Time.timeScale = 0;
        m_GameState = GameState.gameMenu;
        EventManager.Instance.Raise(new GameMenuEvent());
    }

    private void Play() {
        m_GameState = GameState.gamePlay;
        EventManager.Instance.Raise(new GamePlayEvent());
        InitNewGame();
    }

    private void Pause() {
        Time.timeScale = 0;
        m_GameState = GameState.gamePause;
        EventManager.Instance.Raise(new GamePauseEvent());
    }

    private void GameOver() {
        Time.timeScale = 0;
        m_GameState = GameState.gameOver;
        EventManager.Instance.Raise(new GameOverEvent());
    }
    #endregion

    #region Callbacks to UI events
    private void PlayButtonClicked(PlayButtonClickedEvent e) {
        Play();
    }
    #endregion

    #region Callbacks to Player events
    private void PlayerHasBeenHit(PlayerHasBeenHitEvent e) {
        DecrementNLives(1);
        if (m_NLives == 0) GameOver();
    }
    #endregion

    #region Callbacks to Score events
    private void ScoreHasBeenGained(ScoreItemEvent e) {
        IncrementScore(e.eScore.Score);
    }
    #endregion
}
```

---

## Example 2: Static Collection Pattern

For managing all instances of a type (useful for collectibles, enemies, etc.):

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GD3.Events;

public class Bomb : MonoBehaviour, IScore {

    enum State { none, off, on, collected }

    #region Static collection management
    private static List<Bomb> m_Bombs = new List<Bomb>();
    public static List<Bomb> Bombs { get { return m_Bombs; } }

    public static bool AreAllBombsDestroyed {
        get {
            Bomb nonDestroyedBomb = m_Bombs.Find(item => item != null && !item.m_Destroyed);
            return nonDestroyedBomb == null;
        }
    }

    public static Bomb RandomOffBomb {
        get {
            List<Bomb> offBombs = m_Bombs.FindAll(item => !item.IsOn);
            if (offBombs != null && offBombs.Count > 0)
                return offBombs[Random.Range(0, offBombs.Count)];
            return null;
        }
    }

    public static void LightRandomBomb() {
        Bomb bomb = RandomOffBomb;
        if (bomb) bomb.LightWick();
    }
    #endregion

    [Header("Bomb")]
    [SerializeField] private int m_ScoreOff;
    [SerializeField] private int m_ScoreOn;

    private State m_State;
    private bool m_Destroyed;

    public bool IsOn { get { return m_State == State.on; } }
    public int Score {
        get { return m_State == State.on ? m_ScoreOn : m_ScoreOff; }
    }

    #region Unity lifecycle
    private void OnEnable() {
        if (!m_Bombs.Contains(this))
            m_Bombs.Add(this);
    }

    private void OnDestroy() {
        m_Bombs.Remove(this);
    }

    void Start() {
        m_State = State.off;
    }
    #endregion

    #region Bomb logic
    public void LightWick() {
        StartCoroutine(LightWickCoroutine());
    }

    IEnumerator LightWickCoroutine() {
        yield return new WaitForSeconds(1f);
        m_State = State.on;
    }
    #endregion

    #region Collision handling
    private void OnTriggerEnter(Collider other) {
        if (GameManager.Instance.IsPlaying && !m_Destroyed && other.CompareTag("Player")) {
            m_Destroyed = true;

            EventManager.Instance.Raise(new ScoreItemEvent() { eScore = this });
            EventManager.Instance.Raise(new BombHasBeenDestroyedEvent() {
                eBomb = this,
                eDestroyedByPlayer = true
            });
            Destroy(gameObject);

            if (IsOn) LightRandomBomb();
        }
    }
    #endregion
}
```

---

## Example 3: Event Publishing Patterns

### Simple event (no data)
```csharp
EventManager.Instance.Raise(new GameOverEvent());
```

### Event with data (properties)
```csharp
EventManager.Instance.Raise(new GameStatisticsChangedEvent() {
    eBestScore = BestScore,
    eScore = m_Score,
    eNLives = m_NLives
});
```

### Event with object reference
```csharp
EventManager.Instance.Raise(new EnemyHasBeenDestroyedEvent() {
    eEnemy = this,
    eDestroyedByPlayer = true
});
```

---

## Example 4: Event Subscription in Regular Classes

For non-manager classes that need to listen to events:

```csharp
public class Level : MonoBehaviour, IEventHandler {
    public void SubscribeEvents() {
        EventManager.Instance.AddListener<BombHasBeenCollectedEvent>(BombHasBeenCollected);
        EventManager.Instance.AddListener<PowerCoinHitEvent>(PowerCoinHit);
    }

    public void UnsubscribeEvents() {
        EventManager.Instance.RemoveListener<BombHasBeenCollectedEvent>(BombHasBeenCollected);
        EventManager.Instance.RemoveListener<PowerCoinHitEvent>(PowerCoinHit);
    }

    private void Awake() {
        SubscribeEvents();
    }

    private void OnDestroy() {
        UnsubscribeEvents();
    }

    private void BombHasBeenCollected(BombHasBeenCollectedEvent e) {
        // Handle bomb collection...
    }

    private void PowerCoinHit(PowerCoinHitEvent e) {
        // Handle power coin hit...
    }
}
```
