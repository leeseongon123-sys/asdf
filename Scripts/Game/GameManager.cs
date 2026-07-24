using System;
using System.Collections;
using SculptGame.AI;
using SculptGame.Building;
using UnityEngine;

namespace SculptGame.Game
{
    public enum GameState
    {
        Lobby,
        TopicDisplay,
        BuildingPhase,
        ScoringPhase,
        ResultPhase,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Lobby;

        public event Action<GameState> OnGameStateChanged;
        public event Action<AIEvaluationResult> OnEvaluationResultReceived;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.OnBuildingTimeEnded += HandleTimeEnded;
            }
            if (AIVisionEvaluator.Instance != null)
            {
                AIVisionEvaluator.Instance.OnEvaluationCompleted += HandleEvaluationCompleted;
            }

            StartGame();
        }

        public void StartGame()
        {
            if (BuildingSystem.Instance != null)
            {
                BuildingSystem.Instance.ClearAllObjects();
            }
            StartRound(1);
        }

        public void StartNextRound()
        {
            if (RoundManager.Instance != null && RoundManager.Instance.HasNextRound())
            {
                if (BuildingSystem.Instance != null)
                {
                    BuildingSystem.Instance.ClearAllObjects();
                }
                StartRound(RoundManager.Instance.currentRound + 1);
            }
            else
            {
                SetState(GameState.GameOver);
            }
        }

        private void StartRound(int roundNumber)
        {
            SetState(GameState.TopicDisplay);

            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.StartRound(roundNumber);
            }

            // After 3 seconds of topic display, move to building phase
            StartCoroutine(TopicDisplayRoutine());
        }

        private IEnumerator TopicDisplayRoutine()
        {
            yield return new WaitForSeconds(3.0f);
            SetState(GameState.BuildingPhase);
        }

        private void HandleTimeEnded()
        {
            if (CurrentState == GameState.BuildingPhase)
            {
                TriggerScoring();
            }
        }

        public void TriggerScoring()
        {
            if (CurrentState == GameState.BuildingPhase)
            {
                if (RoundManager.Instance != null)
                {
                    RoundManager.Instance.StopTimer();
                }
                SetState(GameState.ScoringPhase);

                // Call AI Vision Evaluator
                if (AIVisionEvaluator.Instance != null && RoundManager.Instance != null && RoundManager.Instance.CurrentTopic != null)
                {
                    TopicData topic = RoundManager.Instance.CurrentTopic;
                    AIVisionEvaluator.Instance.EvaluateCanvas(topic.topicName, topic.passScore);
                }
            }
        }

        private void HandleEvaluationCompleted(AIEvaluationResult result)
        {
            SetState(GameState.ResultPhase);
            OnEvaluationResultReceived?.Invoke(result);
        }

        public void SetState(GameState newState)
        {
            CurrentState = newState;
            OnGameStateChanged?.Invoke(newState);
            Debug.Log($"[GameManager] Game State changed to: {newState}");
        }
    }
}
