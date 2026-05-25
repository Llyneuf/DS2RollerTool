using System;
using System.Windows;
using System.Windows.Threading;
using DS2Roller.Data;
using DS2Roller.Logic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace DS2Roller.UI
{
    
    public partial class MainWindow : Window
    {
        private DispatcherTimer _rollTimer;
        private int _ticks;
        private const int MaxTicks = 18; // длительность "крутилки"
        private const int StartSpeedMs = 60;   // стартовая скорость
        private const int EndSpeedMs = 300;    // скорость в конце
        private const int TimerSpeedMs = 80;
        private int _skipCounter;
        private MediaPlayer _tickSound;
        private MediaPlayer _finalSound;





        public MainWindow()
        {
            InitializeComponent();
            InitSounds();
            InitTimer();
        }

        private void InitTimer()
        {
            _rollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(TimerSpeedMs)
            };
            _rollTimer.Tick += RollTick;
        }
        
        private void InitSounds()
        {
            _tickSound = new MediaPlayer();
            _tickSound.Open(new Uri(ExtractSound("tick.wav"), UriKind.Absolute));
            _tickSound.Volume = 0.4;

            _finalSound = new MediaPlayer();
            _finalSound.Open(new Uri(ExtractSound("final.wav"), UriKind.Absolute));
            _finalSound.Volume = 0.7;
        }

        private static string ExtractSound(string fileName)
        {
            string outputDirectory = Path.Combine(Path.GetTempPath(), "DS2Roller");
            Directory.CreateDirectory(outputDirectory);

            string outputPath = Path.Combine(outputDirectory, fileName);
            Uri resourceUri = new($"pack://application:,,,/Assets/Sounds/{fileName}", UriKind.Absolute);
            using Stream? source = Application.GetResourceStream(resourceUri)?.Stream;

            if (source is null)
            {
                throw new FileNotFoundException($"Sound resource not found: {fileName}");
            }

            using FileStream destination = File.Create(outputPath);
            source.CopyTo(destination);
            return outputPath;
        }

        private void Roll_Click(object sender, RoutedEventArgs e)
        {
            RollButton.IsEnabled = false;

            _ticks = 0;
            _skipCounter = 0;

            ClassText.Opacity = 0.6;
            GiftText.Opacity = 0.6;

            ClassText.Foreground = Brushes.White;
            GiftText.Foreground = Brushes.White;

            _rollTimer.Start();
        }


        private void RollTick(object? sender, EventArgs e)
        {
            _ticks++;

            // коэффициент прогресса (0 → 1)
            double progress = (double)_ticks / MaxTicks;

            // чем дальше — тем больше пропусков
            int skipThreshold = progress switch
            {
                < 0.5 => 1,   // быстро
                < 0.75 => 3,  // медленнее
                < 0.9 => 6,   // почти финал
                _ => 10        // залипание
            };

            _skipCounter++;

            if (_skipCounter >= skipThreshold)
            {
                _skipCounter = 0;

                ClassText.Text = $"Класс: {Roller.Roll(Classes.All)}";
                GiftText.Text = $"Начальный дар: {Roller.Roll(Gifts.All)}";
                
                _tickSound.Stop();
                _tickSound.Position = TimeSpan.Zero;
                _tickSound.Play();
            }

            if (_ticks >= MaxTicks)
            {
                _rollTimer.Stop();
                ShowFinalResult();
            }
        }



        private void ShowFinalResult()
        {
            _finalSound.Stop();
            _finalSound.Position = TimeSpan.Zero;
            _finalSound.Play();

            ClassText.Text = $"Класс: {Roller.Roll(Classes.All)}";
            GiftText.Text = $"Начальный дар: {Roller.Roll(Gifts.All)}";

            AnimateFinalText(ClassText);
            AnimateFinalText(GiftText);

            RollButton.IsEnabled = true;
        }

        private void AnimateFinalText(System.Windows.Controls.TextBlock text)
        {
            // Fade-in
            var fade = new DoubleAnimation
            {
                From = 0.3,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Цвет -> золотой Souls
            var colorAnim = new ColorAnimation
            {
                From = Colors.White,
                To = (Color)ColorConverter.ConvertFromString("#C9A86A"),
                Duration = TimeSpan.FromMilliseconds(400)
            };

            var brush = new SolidColorBrush(Colors.White);
            text.Foreground = brush;

            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            text.BeginAnimation(OpacityProperty, fade);
        }

    }
}
