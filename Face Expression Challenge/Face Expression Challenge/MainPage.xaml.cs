using System;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Media.Core;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FaceExpressionChallenge
{
    /// <summary>
    /// An Emotion Recognition Based Game
    /// </summary>

    public sealed partial class MainPage : Page
    {
        private bool isPreviewing = false;
        private StorageFile photoFile;
        private MediaCapture capture;
        private IMediaEncodingProperties previewProperties;
        private CameraStreamProperties cameraProperties;

        //Insert your API key here
        private string subscriptionKey = "";
        private EmotionServiceClient emotionServiceClient;

        private Emotion[] emotionResult;
        private Score[] scores = new Score[8];
        private Random randomNum = new Random();
        private Emotions currentEmotion;

        private FaceDetectionEffect faceDetectionEffect;
        private double a;

        public MainPage()
        {
            this.InitializeComponent();

            emotionServiceClient = new EmotionServiceClient(subscriptionKey);

            Application.Current.EnteredBackground += async delegate { await CloseCamera(); };
            Application.Current.LeavingBackground += 
                delegate
                {
                    if(screen2.Visibility == Visibility.Visible)
                        OpenCamera();
                };
        }

        private async void OpenCamera()
        {
            try
            {
                capture = new MediaCapture();
                await capture.InitializeAsync();
                capture.Failed += delegate { CloseCamera(); };

                camPreview.Source = capture;
                camPreview.FlowDirection = FlowDirection.RightToLeft;

                await capture.StartPreviewAsync();
                isPreviewing = true;

                previewProperties = capture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
                cameraProperties = new CameraStreamProperties(previewProperties);
                camPreview.Height = facesCanvas.Height = 0.8 * page.ActualHeight;
                camPreview.Width = facesCanvas.Width = cameraProperties.AspectRatio * camPreview.Height;

                FaceDetectionInitialization();
            }
            catch (Exception ex)
            {
                await CloseCamera();
                errorTxt.Text = "There was a problem while opening the camera. Be sure it's connected and not used by onother app";
            }
        }

        private async Task CloseCamera()
        {
            if (faceDetectionEffect != null && faceDetectionEffect.Enabled)
            {
                faceDetectionEffect.Enabled = false;
                faceDetectionEffect.FaceDetected -= FaceDetected;
                await capture.ClearEffectsAsync(MediaStreamType.VideoPreview);
                faceDetectionEffect = null;
                facesCanvas.Children.Clear();
            }

            if (capture != null)
            {
                if (isPreviewing)
                {
                    await capture.StopPreviewAsync();
                    isPreviewing = false;
                }
                capture.Dispose();
                capture = null;
            }

            //if (photoFile != null)
            //{
            //    await photoFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            //    photoFile = null;
            //}
        }

        private async void ReadyBtn_Click(object sender, RoutedEventArgs e)
        {
            ready_Btn.IsEnabled = false;
            progressRing.IsActive = true;

            try
            {
                await TakePhoto();

                scores[(int)currentEmotion].Value =
                    emotionResult.Max(p => p.Scores.ToRankedList().First(p2 => p2.Key == currentEmotion.ToString()).Value);

                int i = 0;
                bool allEmotionsChecked = true;
                while (i < 8 && (allEmotionsChecked &= scores[i++] != null)) ;

                if(!allEmotionsChecked)
                    PrepareNextEmotion();
                else
                {
                    for (int j = 0; j < scores.Length; j++)
                    {
                        EmotionStat stat = new EmotionStat(scores[j]);
                        screen3.Children.Add(stat);
                        Grid.SetColumn(stat, j);
                    }

                    screen2.Visibility = Visibility.Collapsed;
                    screen3.Visibility = Visibility.Visible;

                    CloseCamera();
                }
            }
            catch (Exception ex)
            {
                CloseCamera();
                
                switch (emotionResult.Length)
                {
                    case 0:
                        errorTxt.Text = "Please stay closer to camera so it can detect you.";
                        break;
                    default:
                        errorTxt.Text = ex.Message;
                        break;
                }
            }

            ready_Btn.IsEnabled = true;
            progressRing.IsActive = false;
        }

        private async Task TakePhoto()
        {
            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();

            photoFile = await KnownFolders.PicturesLibrary.CreateFileAsync("face.jpg", CreationCollisionOption.ReplaceExisting);
            await capture.CapturePhotoToStorageFileAsync(imageProperties, photoFile);

            using (Stream imageFileStream = await photoFile.OpenStreamForReadAsync())
            {
                emotionResult = await emotionServiceClient.RecognizeAsync(imageFileStream);
            }

            await photoFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            photoFile = null;
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            PrepareNextEmotion();

            OpenCamera();

            screen1.Visibility = Visibility.Collapsed;
            screen2.Visibility = Visibility.Visible;
        }

        private void PrepareNextEmotion()
        {
            int nextNum = 0;
            while (scores[nextNum = randomNum.Next(0, 8)] != null) ;

            currentEmotion = (Emotions)nextNum;
            scores[nextNum] = new Score(currentEmotion);

            emojiIcon.Glyph = scores[nextNum].Glyph;
            expressionTxt.Text = currentEmotion.ToString();
        }

        private void ResetBtns_Click(object sender, RoutedEventArgs e)
        {
            int totalChildren = screen3.Children.Count;
            for (int i = 2; i < totalChildren; i++)
                screen3.Children.RemoveAt(2);
            scores = new Score[8];

            screen3.Visibility = Visibility.Collapsed;
            if ((sender as AppBarButton).Label == "Home")
                screen1.Visibility = Visibility.Visible;
            else
                StartGame_Click(null, null);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (screen2.Visibility == Visibility.Visible)
            {
                camPreview.Height = facesCanvas.Height = 0.8 * page.ActualHeight;
                camPreview.Width = facesCanvas.Width = cameraProperties.AspectRatio * camPreview.Height;
            }
        }

        private void ErrorTxt_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            tryAgainBtn.Visibility = (errorTxt.Text == string.Empty) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void TryAgainBtn_Click(object sender, RoutedEventArgs e)
        {
            errorTxt.Text = string.Empty;

            OpenCamera();
        }

        private async void FaceDetectionInitialization()
        {
            var definition = new FaceDetectionEffectDefinition()
            {
                SynchronousDetectionEnabled = false,
                DetectionMode = FaceDetectionMode.HighPerformance
            };

            faceDetectionEffect = (FaceDetectionEffect)await capture.AddVideoEffectAsync(definition, MediaStreamType.VideoPreview);

            //Chooses the shortest interval between detection events
            faceDetectionEffect.DesiredDetectionInterval = TimeSpan.FromMilliseconds(33);
            faceDetectionEffect.Enabled = true;
            faceDetectionEffect.FaceDetected += FaceDetected;
        }

        private async void FaceDetected(FaceDetectionEffect sender, FaceDetectedEventArgs args)
        {
            var previewStream = previewProperties as VideoEncodingProperties;

            var dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;

            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {

                a = camPreview.Width / previewStream.Width;

                facesCanvas.Children.Clear();

                //Detects faces in preview (without Project Oxford) and places a rectangle on them
                foreach (Windows.Media.FaceAnalysis.DetectedFace face in args.ResultFrame.DetectedFaces)
                {
                    Rectangle rect = new Rectangle()
                    {
                        Width = face.FaceBox.Width *a,
                        Height = face.FaceBox.Height * a,
                        Stroke = new SolidColorBrush(Windows.UI.Colors.Red),
                        StrokeThickness = 2.0
                    };

                    facesCanvas.Children.Add(rect);
                    Canvas.SetLeft(rect, camPreview.Width - (face.FaceBox.X * a) - rect.Width);
                    Canvas.SetTop(rect, face.FaceBox.Y * a);
                }
            });
        }

    }
}
