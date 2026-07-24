using System;
using UnityEngine;

namespace SculptGame.AI
{
    [Serializable]
    public class AIEvaluationResult
    {
        public int score;
        public bool passed;
        public string comment;
        public Texture2D capturedImage;
    }

    [Serializable]
    public class AIJsonResponse
    {
        public int score;
        public bool passed;
        public string comment;
    }

    public enum AIProvider
    {
        MockTest,           // Local mock response for quick testing in editor
        LMStudio_QwenVL,    // Local LM Studio with Qwen2.5-VL Vision model
        OpenAI_Vision,      // OpenAI GPT-4o Vision API
        Custom_Endpoint     // Custom Vision API Endpoint
    }
}
