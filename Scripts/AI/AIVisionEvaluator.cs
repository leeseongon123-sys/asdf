using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SculptGame.AI
{
    public class AIVisionEvaluator : MonoBehaviour
    {
        public static AIVisionEvaluator Instance { get; private set; }

        [Header("AI Settings")]
        public AIProvider provider = AIProvider.MockTest;
        [Tooltip("LM Studio or OpenAI endpoint URL")]
        public string apiEndpoint = "http://localhost:1234/v1/chat/completions";
        [Tooltip("API Key if required (OpenAI / Custom)")]
        public string apiKey = "";
        [Tooltip("Model ID in LM Studio or OpenAI (e.g., qwen2.5-vl-7b-instruct, gpt-4o)")]
        public string modelName = "qwen2.5-vl-7b-instruct";

        [Header("Capture Settings")]
        public Camera evalCamera;
        public int captureWidth = 512;
        public int captureHeight = 512;

        public event Action<AIEvaluationResult> OnEvaluationCompleted;
        public event Action<string> OnEvaluationFailed;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Captures the target camera and evaluates the build against the given topic.
        /// </summary>
        public void EvaluateCanvas(string topic, int targetPassScore)
        {
            StartCoroutine(EvaluateRoutine(topic, targetPassScore));
        }

        private IEnumerator EvaluateRoutine(string topic, int targetPassScore)
        {
            Texture2D screenshot = CaptureCanvasTexture();

            if (provider == AIProvider.MockTest)
            {
                yield return new WaitForSeconds(1.5f); // Simulate network delay

                // Generate procedural mock score for testing without local LM Studio running
                int mockScore = UnityEngine.Random.Range(50, 96);
                bool mockPassed = mockScore >= targetPassScore;
                string mockComment = mockPassed
                    ? $"[Mock AI] '{topic}'의 특징과 형태 표현이 뛰어납니다! 구도와 조형이 훌륭합니다."
                    : $"[Mock AI] '{topic}'의 느낌이 다소 부족합니다. 부품을 더 추가하고 형태를 보강해보세요.";

                AIEvaluationResult mockResult = new AIEvaluationResult
                {
                    score = mockScore,
                    passed = mockPassed,
                    comment = mockComment,
                    capturedImage = screenshot
                };

                OnEvaluationCompleted?.Invoke(mockResult);
                yield break;
            }

            // Convert image to Base64
            byte[] imageBytes = screenshot.EncodeToPNG();
            string base64Image = Convert.ToBase64String(imageBytes);

            // Construct Vision Prompt
            string promptText = $"Analyze the 3D sculpture in this image. The topic requested was '{topic}'. " +
                               $"Evaluate how closely the built object resembles '{topic}'. " +
                               $"Reply strictly in JSON format with no extra text or markdown format: " +
                               $"{{\"score\": integer_0_to_100, \"passed\": boolean, \"comment\": \"Korean comment analyzing the build\"}}";

            // Prepare JSON payload for OpenAI / LM Studio Vision API format
            string jsonBody = $"{{" +
                $"\"model\":\"{modelName}\"," +
                $"\"messages\":[" +
                    $"{{" +
                        $"\"role\":\"user\"," +
                        $"\"content\":[" +
                            $"{{\"type\":\"text\",\"text\":\"{EscapeJson(promptText)}\"}}," +
                            $"{{\"type\":\"image_url\",\"image_url\":{{\"url\":\"data:image/png;base64,{base64Image}\"}}}}" +
                        $"]" +
                    $"}}" +
                $"]," +
                $"\"max_tokens\":300" +
            $"}}";

            using (UnityWebRequest request = new UnityWebRequest(apiEndpoint, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(apiKey))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                }

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"AI Request Failed: {request.error}. Falling back to fallback score.");
                    OnEvaluationFailed?.Invoke(request.error);

                    // Fallback result on network failure
                    AIEvaluationResult fallbackResult = new AIEvaluationResult
                    {
                        score = 75,
                        passed = 75 >= targetPassScore,
                        comment = $"[네트워크 오류 - 기본 채점] 주제 '{topic}' 조형물 제출 완료 (연동 오류: {request.error})",
                        capturedImage = screenshot
                    };
                    OnEvaluationCompleted?.Invoke(fallbackResult);
                }
                else
                {
                    string responseText = request.downloadHandler.text;
                    AIEvaluationResult parsedResult = ParseAIResponse(responseText, topic, targetPassScore, screenshot);
                    OnEvaluationCompleted?.Invoke(parsedResult);
                }
            }
        }

        public Texture2D CaptureCanvasTexture()
        {
            Camera cam = evalCamera != null ? evalCamera : Camera.main;
            if (cam == null) return null;

            RenderTexture rt = new RenderTexture(captureWidth, captureHeight, 24);
            RenderTexture previousRT = cam.targetTexture;

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D resultTex = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
            resultTex.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
            resultTex.Apply();

            cam.targetTexture = previousRT;
            RenderTexture.active = null;
            Destroy(rt);

            return resultTex;
        }

        private AIEvaluationResult ParseAIResponse(string jsonText, string topic, int targetPassScore, Texture2D tex)
        {
            try
            {
                // Simple search for JSON content in response
                int firstBrace = jsonText.IndexOf('{');
                int lastBrace = jsonText.LastIndexOf('}');

                if (firstBrace >= 0 && lastBrace > firstBrace)
                {
                    string cleanJson = jsonText.Substring(firstBrace, lastBrace - firstBrace + 1);
                    
                    // Look for content block if wrapped in chat completion format
                    if (cleanJson.Contains("choices") && cleanJson.Contains("content"))
                    {
                        // Extract content text inside JSON
                        int contentIdx = cleanJson.IndexOf("\"content\":\"");
                        if (contentIdx != -1)
                        {
                            int start = contentIdx + 11;
                            int end = cleanJson.IndexOf("\"", start);
                            if (end > start)
                            {
                                string contentStr = cleanJson.Substring(start, end - start);
                                contentStr = UnescapeJson(contentStr);
                                int innerFirst = contentStr.IndexOf('{');
                                int innerLast = contentStr.LastIndexOf('}');
                                if (innerFirst >= 0 && innerLast > innerFirst)
                                {
                                    cleanJson = contentStr.Substring(innerFirst, innerLast - innerFirst + 1);
                                }
                            }
                        }
                    }

                    AIJsonResponse jsonObject = JsonUtility.FromJson<AIJsonResponse>(cleanJson);
                    if (jsonObject != null && jsonObject.score > 0)
                    {
                        return new AIEvaluationResult
                        {
                            score = jsonObject.score,
                            passed = jsonObject.score >= targetPassScore,
                            comment = jsonObject.comment,
                            capturedImage = tex
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"JSON parse error: {ex.Message}");
            }

            // Fallback parse if structure didn't match standard
            return new AIEvaluationResult
            {
                score = 80,
                passed = 80 >= targetPassScore,
                comment = $"주제 '{topic}'에 맞춰 창의적인 형상을 완성했습니다.",
                capturedImage = tex
            };
        }

        private string EscapeJson(string str)
        {
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
        }

        private string UnescapeJson(string str)
        {
            return str.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
        }
    }
}
