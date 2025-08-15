using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Input;


namespace RouletteWheel
{
    public partial class MainWindow : Window
    {
        private const int NumPockets = 37; // numbers 0-36
        private readonly string[] Numbers = new string[NumPockets]
{
    "0","32","15","19","4","21","2","25","17","34","6","27","13","36","11",
    "30","8","23","10","5","24","16","33","1","20","14","31","9","22","18",
    "29","7","28","12","35","3","26"
};



        private readonly Brush[] Colors = new Brush[NumPockets]; // Define your colors here

        private Ellipse ball;
        private double ballAngle;
        private double ballSpeed;
        private double radius = 200;
        private Point center;
        private DispatcherTimer timer;
        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            InitializeColors();
            DrawWheel();
            InitializeBall();
            StartBallSpin();
        }

        private void InitializeColors()
        {
            // Example: alternating red/black, green for 0
            Colors[0] = Brushes.Green;
            for (int i = 1; i < NumPockets; i++)
            {
                Colors[i] = (i % 2 == 0) ? Brushes.Red : Brushes.Black;
            }
        }

        private void DrawWheel()
        {
            wheelCanvas.Children.Clear(); // clear previous drawings
            center = new Point(wheelCanvas.Width / 2, wheelCanvas.Height / 2);
            double pocketAngle = 360.0 / NumPockets;

            for (int i = 0; i < NumPockets; i++)
            {
                double startAngleRad = i * pocketAngle * Math.PI / 180;
                double endAngleRad = (i + 1) * pocketAngle * Math.PI / 180;
                double midAngleRad = (i + 0.5) * pocketAngle * Math.PI / 180;
                double midAngleDeg = (i + 0.5) * pocketAngle;

                // Draw sectors
                Path sector = new Path
                {
                    Fill = Colors[i]
                };
                PathFigure figure = new PathFigure { StartPoint = center };
                figure.Segments.Add(new LineSegment(new Point(center.X + radius * Math.Cos(startAngleRad), center.Y + radius * Math.Sin(startAngleRad)), true));
                figure.Segments.Add(new ArcSegment(new Point(center.X + radius * Math.Cos(endAngleRad), center.Y + radius * Math.Sin(endAngleRad)), new Size(radius, radius), 0, false, SweepDirection.Clockwise, true));
                figure.Segments.Add(new LineSegment(center, true));
                PathGeometry geo = new PathGeometry();
                geo.Figures.Add(figure);
                sector.Data = geo;
                wheelCanvas.Children.Add(sector);

                // Draw numbers
                TextBlock text = new TextBlock
                {
                    Text = Numbers[i],
                    Foreground = Brushes.White,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };

                // Position the number along the radius slightly inward
                double textRadius = radius - 40; // move inside so no clipping
                double x = center.X + textRadius * Math.Cos(midAngleRad);
                double y = center.Y + textRadius * Math.Sin(midAngleRad);

                // Temporarily add to canvas to measure size
                wheelCanvas.Children.Add(text);
                text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double textWidth = text.DesiredSize.Width;
                double textHeight = text.DesiredSize.Height;

                // Center text on the point
                Canvas.SetLeft(text, x - textWidth / 2);
                Canvas.SetTop(text, y - textHeight / 2);

                // Rotate tangentially
                RotateTransform textRotate = new RotateTransform(midAngleDeg + 90);
                text.RenderTransform = textRotate;
            }
        }


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartBallSpin(); // starts the ball spinning when you click anywhere in the window
        }


        private void InitializeBall()
        {
            ball = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.White
            };
            wheelCanvas.Children.Add(ball);
            ballAngle = 0;
        }

        private void StartBallSpin()
        {
            ballSpeed = 10 + random.NextDouble() * 5; // random speed
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Move ball along circular path
            ballAngle += ballSpeed;
            ballAngle %= 360;

            double rad = ballAngle * Math.PI / 180;
            double ballRadius = radius - 20; // slightly inward from wheel edge
            double x = center.X + ballRadius * Math.Cos(rad) - ball.Width / 2;
            double y = center.Y + ballRadius * Math.Sin(rad) - ball.Height / 2;
            Canvas.SetLeft(ball, x);
            Canvas.SetTop(ball, y);

            // Gradually slow down
            ballSpeed *= 0.99;

            // Stop when very slow
            if (ballSpeed < 0.1)
            {
                timer.Stop();
                int pocketIndex = (int)((ballAngle + (360.0 / NumPockets) / 2) / (360.0 / NumPockets)) % NumPockets;
                resultText.Text = $"Result: {Numbers[pocketIndex]}";
            }
        }
    }
}
