using System;
using System.Collections.Generic;
using UnityEngine;

namespace SculptGame.Game
{
    public class RoundManager : MonoBehaviour
    {
        public static RoundManager Instance { get; private set; }

        [Header("Round Configuration")]
        public int totalRounds = 3;
        public int currentRound = 1;
        
        public List<TopicData> defaultTopics = new List<TopicData>
        {
            new TopicData("자동차", 65, 120f, "바퀴 4개와 차체가 돋보이는 멋진 자동차를 만드세요!"),
            new TopicData("집", 70, 150f, "지붕, 창문, 문이 있는 든든한 집을 조형해보세요."),
            new TopicData("동물 (강아지/고양이)", 75, 120f, "귀여운 귀와 꼬리가 표현된 동물을 완성하세요!"),
            new TopicData("로봇", 80, 150f, "머리, 팔, 다리가 달린 미래형 로봇을 만들어보세요."),
            new TopicData("비행기", 85, 150f, "날개와 꼬리날개가 장착된 하늘을 나는 비행기를 만드세요.")
        };

        public TopicData CurrentTopic { get; private set; }
        public float RemainingTime { get; private set; }
        public bool IsTimerRunning { get; private set; }

        public event Action<int, TopicData> OnRoundStarted;
        public event Action<float> OnTimerUpdated;
        public event Action OnBuildingTimeEnded;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            if (IsTimerRunning)
            {
                RemainingTime -= Time.deltaTime;
                OnTimerUpdated?.Invoke(Mathf.Max(0f, RemainingTime));

                if (RemainingTime <= 0f)
                {
                    IsTimerRunning = false;
                    RemainingTime = 0f;
                    OnBuildingTimeEnded?.Invoke();
                }
            }
        }

        public void StartRound(int roundIndex)
        {
            currentRound = roundIndex;

            // Pick topic based on round index or random
            if (defaultTopics != null && defaultTopics.Count > 0)
            {
                int topicIdx = (roundIndex - 1) % defaultTopics.Count;
                CurrentTopic = defaultTopics[topicIdx];
            }
            else
            {
                CurrentTopic = new TopicData("자유 조형", 60, 120f, "원하는 멋진 작품을 만드세요!");
            }

            RemainingTime = CurrentTopic.timeLimitSeconds;
            IsTimerRunning = true;

            OnRoundStarted?.Invoke(currentRound, CurrentTopic);
        }

        public void StopTimer()
        {
            IsTimerRunning = false;
        }

        public bool HasNextRound()
        {
            return currentRound < totalRounds;
        }
    }
}
