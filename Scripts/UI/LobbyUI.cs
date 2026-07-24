using SculptGame.AI;
using SculptGame.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SculptGame.UI
{
    public class LobbyUI : MonoBehaviour
    {
        [Header("Controls")]
        public Button startGameButton;
        public TMP_Dropdown providerDropdown;
        public TMP_InputField endpointInput;
        public TMP_InputField apiKeyInput;

        private void Start()
        {
            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGameClicked);
            }

            if (providerDropdown != null)
            {
                providerDropdown.ClearOptions();
                providerDropdown.options.Add(new TMP_Dropdown.OptionData("Mock 테스트 (API키 미필요)"));
                providerDropdown.options.Add(new TMP_Dropdown.OptionData("LM Studio / Qwen2.5-VL (로컬)"));
                providerDropdown.options.Add(new TMP_Dropdown.OptionData("OpenAI GPT-4o Vision API"));
                providerDropdown.onValueChanged.AddListener(OnProviderChanged);
            }

            if (endpointInput != null && AIVisionEvaluator.Instance != null)
            {
                endpointInput.text = AIVisionEvaluator.Instance.apiEndpoint;
                endpointInput.onValueChanged.AddListener((val) => AIVisionEvaluator.Instance.apiEndpoint = val);
            }
        }

        private void OnProviderChanged(int index)
        {
            if (AIVisionEvaluator.Instance == null) return;

            switch (index)
            {
                case 0:
                    AIVisionEvaluator.Instance.provider = AIProvider.MockTest;
                    break;
                case 1:
                    AIVisionEvaluator.Instance.provider = AIProvider.LMStudio_QwenVL;
                    AIVisionEvaluator.Instance.apiEndpoint = "http://localhost:1234/v1/chat/completions";
                    AIVisionEvaluator.Instance.modelName = "qwen2.5-vl-7b-instruct";
                    break;
                case 2:
                    AIVisionEvaluator.Instance.provider = AIProvider.OpenAI_Vision;
                    AIVisionEvaluator.Instance.apiEndpoint = "https://api.openai.com/v1/chat/completions";
                    AIVisionEvaluator.Instance.modelName = "gpt-4o";
                    break;
            }

            if (endpointInput != null)
            {
                endpointInput.text = AIVisionEvaluator.Instance.apiEndpoint;
            }
        }

        private void OnStartGameClicked()
        {
            if (AIVisionEvaluator.Instance != null && apiKeyInput != null)
            {
                AIVisionEvaluator.Instance.apiKey = apiKeyInput.text;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }
    }
}
