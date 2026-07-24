using SculptGame.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SculptGame.UI
{
    public class HUDUI : MonoBehaviour
    {
        [Header("UI Text References")]
        public TextMeshProUGUI topicText;
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI roundText;
        public TextMeshProUGUI targetScoreText;
        public TextMeshProUGUI objectCountText;

        [Header("Buttons")]
        public Button submitEarlyButton;

        private void Start()
        {
            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.OnRoundStarted += UpdateRoundInfo;
                RoundManager.Instance.OnTimerUpdated += UpdateTimer;
            }

            if (submitEarlyButton != null)
            {
                submitEarlyButton.onClick.AddListener(OnSubmitEarlyClicked);
            }
        }

        private void Update()
        {
            if (Building.BuildingSystem.Instance != null && objectCountText != null)
            {
                int current = Building.BuildingSystem.Instance.GetPlacedObjectCount();
                int max = Building.BuildingSystem.Instance.maxObjectsPerRound;
                objectCountText.text = $"오브젝트: {current} / {max}";
            }
        }

        private void UpdateRoundInfo(int roundNumber, TopicData topic)
        {
            if (roundText != null) roundText.text = $"ROUND {roundNumber} / {RoundManager.Instance.totalRounds}";
            if (topicText != null) topicText.text = $"주제: {topic.topicName}";
            if (targetScoreText != null) targetScoreText.text = $"목표 점수: {topic.passScore}점 이상";
        }

        private void UpdateTimer(float remainingSeconds)
        {
            if (timerText == null) return;
            int minutes = Mathf.FloorToInt(remainingSeconds / 60f);
            int seconds = Mathf.FloorToInt(remainingSeconds % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (remainingSeconds <= 10f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }

        private void OnSubmitEarlyClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerScoring();
            }
        }

        private void OnDestroy()
        {
            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.OnRoundStarted -= UpdateRoundInfo;
                RoundManager.Instance.OnTimerUpdated -= UpdateTimer;
            }
        }
    }
}
