using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceExpressionChallenge
{
    public class Score
    {
        public int EmotionID { get; }
        public Emotions Emotion { get; }
        public float Value { get; set; }
        public string Glyph { get; set; }
        //public bool IsDublicate { get; } = false;

        //static int[] emotionIDsChecked = new int[8];
        //static int index = -1;

        public Score(Emotions emotion)
        {
            this.Emotion = emotion;
            this.EmotionID = (int) emotion;

            switch (emotion)
            {
                case Emotions.Anger:
                    this.Glyph = "😠";
                    break;
                case Emotions.Contempt:
                    this.Glyph = "🙄";
                    break;
                case Emotions.Disgust:
                    this.Glyph = "😖";
                    break;
                case Emotions.Fear:
                    this.Glyph = "😨";
                    break;
                case Emotions.Happiness:
                    this.Glyph = "😁";
                    break;
                case Emotions.Neutral:
                    this.Glyph = "😐";
                    break;
                case Emotions.Sadness:
                    this.Glyph = "😢";
                    break;
                case Emotions.Surprise:
                    this.Glyph = "😯";
                    break;
            }

            //for (int i = 0; i < index; i++)
            //{
            //    if (emotionIDsChecked[index] == this.EmotionID)
            //        IsDublicate = true;
            //}

            //if (!IsDublicate)
            //    emotionIDsChecked[++index] = this.EmotionID;
        }

        //public static void Reset()
        //{
        //    index = -1;
        //    emotionIDsChecked = new int[8];
        //}
    }

    public enum Emotions
    {
        Anger, Contempt, Disgust, Fear, Happiness, Neutral, Sadness, Surprise
    }
}
