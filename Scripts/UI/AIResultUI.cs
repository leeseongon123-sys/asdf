using SculptGame.AI;
using SculptGame.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SculptGame.UI
{
    public class AIResultUI : MonoBehaviour
    {
        [Header("UI Component References")]
        public RawImage capturedImageDisplay;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI resultStatusText;
        public TextMeshProUGUI commentText;
        public Button nextRoundButton;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnEvaluationResultReceived += DisplayResult;
            }

            if (nextRoundButton != null)
            {
                nextRoundButton.onClick.AddListener(OnNextRoundClicked);
            }
        }

        public void DisplayResult(AIEvaluationResult result)
        {
            gameObject.SetActive(true);

            // Unlock cursor so user can click Next Round
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (capturedImageDisplay != null && result.capturedImage != null)
            {
                capturedImageDisplay.texture = result.capturedImage;
            }

            if (scoreText != null)
            {
                scoreText.text = $"{result.score}점";
            }

            if (resultStatusText != null)
            {
                if (result.passed)
                {
                    resultStatusText.text = "통과 (PASS)";
                    resultStatusText.color = new Color(0.2f, 0.9f, 0.3f);
                }
                else
                {
                    resultStatusText.text = "실패 (FAIL)";
                    resultStatusText.color = new Color(0.9f, 0.2f, 0.2f);
                }
            }

            if (commentText != null)
            {
                commentText.text = $"AI 심사평:\n\"{result.comment}\"";
            }

            if (nextRoundButton != null)
            {
                TextMeshProUGUI btnText = nextRoundButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null && RoundManager.Instance != null)
                {
                    btnText.text = RoundManager.Instance.HasNextRound() ? "다음 라운드 진입 →" : "최종 결과 / 다시 하기";
                }
            }
        }

        private void OnNextRoundClicked()
        {
            gameObject.SetActive(false);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNextRound();
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnEvaluationResultReceived -= DisplayResult;
            }
        }
    }
}
