using UnityEngine;
using Game.Ball;
using Game.Targets;

namespace Game.Core
{
    public enum GameState { Aiming, Throwing, Win, Fail }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        [SerializeField] private int totalBalls = 5;
        private int ballsRemaining;
        private int targetsRemaining;
        public GameState CurrentState { get; private set; } = GameState.Aiming;
        public bool CanAim => CurrentState == GameState.Aiming;
        public int BallsRemaining => ballsRemaining;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }
        private void OnEnable()
        {
            GameEvents.OnBallThrown += HandleBallThrown;
            GameEvents.OnTargetHit += HandleTargetHit;
            GameEvents.OnBallSettled += HandleBallSettled;
        }
        private void OnDisable()
        {
            GameEvents.OnBallThrown -= HandleBallThrown;
            GameEvents.OnTargetHit -= HandleTargetHit;
            GameEvents.OnBallSettled -= HandleBallSettled;
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
        private void Start()
        {
            targetsRemaining = RecountTargets();
            ballsRemaining = Mathf.Max(0, totalBalls);
            CurrentState = targetsRemaining > 0 ? GameState.Aiming : GameState.Fail;
            if (targetsRemaining == 0) GameEvents.RaiseLevelFail();
        }
        private void HandleBallThrown()
        {
            if (CurrentState != GameState.Aiming || ballsRemaining <= 0) return;
            ballsRemaining--;
            CurrentState = GameState.Throwing;
        }
        private void HandleTargetHit(TargetController target)
        {
            if (CurrentState != GameState.Throwing) return;
            targetsRemaining = Mathf.Max(0, targetsRemaining - 1);
            if (targetsRemaining == 0)
            {
                CurrentState = GameState.Win;
                GameEvents.RaiseLevelWin();
            }
        }
        private void HandleBallSettled(BallController ball)
        {
            if (CurrentState == GameState.Win || CurrentState == GameState.Fail) return;
            if (ballsRemaining <= 0)
            {
                CurrentState = GameState.Fail;
                GameEvents.RaiseLevelFail();
            }
            else CurrentState = GameState.Aiming;
        }
        public void Retry()
        {
            foreach (var t in FindObjectsOfType<TargetController>(true)) t.ResetTarget();
            ballsRemaining = Mathf.Max(0, totalBalls);
            targetsRemaining = RecountTargets();
            CurrentState = targetsRemaining > 0 ? GameState.Aiming : GameState.Fail;
            GameEvents.RaiseRetry();
            if (targetsRemaining == 0) GameEvents.RaiseLevelFail();
        }
        private int RecountTargets() => FindObjectsOfType<TargetController>(true).Length;
    }
}
