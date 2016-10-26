using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FaceExpressionChallenge
{
    public sealed partial class EmotionStat : UserControl
    {
        public Score score {
            set
            {
                emotionValue.Value = value.Value * 100.0f;
                emotionKey.Text = value.Emotion.ToString();
                percentTxt.Text = value.Value.ToString("P");
                emojiIcon.Glyph = value.Glyph;
            }
        }

        public EmotionStat()
        {
            this.InitializeComponent();
        }

        public EmotionStat(Score score)
        {
            this.InitializeComponent();

            this.score = score;
        }
    }
}
