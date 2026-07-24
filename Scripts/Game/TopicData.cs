using System;

namespace SculptGame.Game
{
    [Serializable]
    public class TopicData
    {
        public string topicName;
        public int passScore = 70;
        public float timeLimitSeconds = 120f;
        public string description;

        public TopicData(string name, int score = 70, float duration = 120f, string desc = "")
        {
            topicName = name;
            passScore = score;
            timeLimitSeconds = duration;
            description = desc;
        }
    }
}
