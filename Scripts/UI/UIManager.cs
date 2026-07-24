using SculptGame.Game;
using TMPro;
using UnityEngine;

namespace SculptGame.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        public GameObject lobbyPanel;
        public GameObject topicAnnouncePanel;
        public GameObject buildingHUDPanel;
        public GameObject scoringLoadingPanel;
        public GameObject resultModalPanel;
        public GameObject gameOverPanel;

        [Header("Topic Announcement Text")]
        public TextMeshProUGUI topicAnnounceTitleText;
        public TextMeshProUGUI topicAnnounceDescText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += UpdateUIState;
                UpdateUIState(GameManager.Instance.CurrentState);
            }
        }

        private void UpdateUIState(GameState state)
        {
            if (lobbyPanel) lobbyPanel.SetActive(state == GameState.Lobby);
            if (topicAnnouncePanel) topicAnnouncePanel.SetActive(state == GameState.TopicDisplay);
            if (buildingHUDPanel) buildingHUDPanel.SetActive(state == GameState.BuildingPhase);
            if (scoringLoadingPanel) scoringLoadingPanel.SetActive(state == GameState.ScoringPhase);
            if (resultModalPanel) resultModalPanel.SetActive(state == GameState.ResultPhase);
            if (gameOverPanel) gameOverPanel.SetActive(state == GameState.GameOver);

            // Handle Topic Display Text & Lock cursor state per state
            if (state == GameState.TopicDisplay && RoundManager.Instance != null && RoundManager.Instance.CurrentTopic != null)
            {
                TopicData topic = RoundManager.Instance.CurrentTopic;
                if (topicAnnounceTitleText) topicAnnounceTitleText.text = $"주제: {topic.topicName}";
                if (topicAnnounceDescText) topicAnnounceDescText.text = topic.description;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= UpdateUIState;
            }
        }
    }
}
