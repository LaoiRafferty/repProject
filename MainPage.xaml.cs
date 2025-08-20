using System;
using System.Numerics;
using System.Timers;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Storage;


namespace repProject
{
    public partial class MainPage : ContentPage
    {
        double angle = 0; // Turn controls (degreees)
        Vector2 position = new Vector2(200, 200); // Ship location 
        double speed = 20; // Forward movement speed

        IDispatcherTimer gameTimer;
        Random rng = new Random();
        List<Image> asteroids = new ();
        List<BoxView> bullets = new ();
        bool gameOver = false;
        int highScore;

        bool shooting = false;
        DateTime lastShotTime = DateTime.MinValue;
        TimeSpan fireRate = TimeSpan.FromMilliseconds(400);

        int timeLeft = 30;
        IDispatcherTimer countdownTimer;

        int score = 0;

        public MainPage()
        {
            InitializeComponent();

            highScore = Preferences.Get("HighScore", 0);
            UpdateScore();

            // Set start position
            AbsoluteLayout.SetLayoutBounds(PlayerSprite,
                new Rect(position.X, position.Y, 50, 50));

#if WINDOWS
        this.Loaded += (s, e) =>
        {
            var window = Application.Current?.Windows[0]?.Handler?.PlatformView
                         as Microsoft.UI.Xaml.Window;

            if (window?.Content is Microsoft.UI.Xaml.FrameworkElement rootElement)
            {
                rootElement.KeyDown += OnKeyDown;
                rootElement.KeyUp += OnKeyUp;
                rootElement.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
            }
        };
#endif



            // Starts timer
            gameTimer = Dispatcher.CreateTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            // Spawns asteroid every 2 seconds
            Device.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                if (!gameOver) SpawnAsteroid();
                return !gameOver; // keeps spawning until games over
            });
        }

        private void OnStartClicked(object sender, EventArgs e)
        {
            ResetGame();
            Overlay.IsVisible = false; // hide start overlay
        }

        private void ResetGame()
        {
           
            foreach (var a in asteroids) GameArea.Children.Remove(a);
            foreach (var b in bullets) GameArea.Children.Remove(b);
            asteroids.Clear();
            bullets.Clear();

            // Resets scores
            score = 0;
            UpdateScore();

            // Resets ship
            position = new Vector2(200, 200);
            angle = 0;
            PlayerSprite.Rotation = 0;
            AbsoluteLayout.SetLayoutBounds(PlayerSprite, new Rect(position.X, position.Y, 50, 50));

            gameOver = false;
            shooting = false;

            // rEsets timer
            timeLeft = 30;
            TimerLabel.Text = $"Time: {timeLeft}";

            // Starts countdown
            countdownTimer?.Stop();
            countdownTimer = Dispatcher.CreateTimer();
            countdownTimer.Interval = TimeSpan.FromSeconds(1);
            countdownTimer.Tick += (s, e) =>
            {
                if (gameOver) { countdownTimer.Stop(); return; }

                timeLeft--;
                TimerLabel.Text = $"Time: {timeLeft}";

                if (timeLeft <= 0)
                {
                    EndGame();
                    countdownTimer.Stop();
                }
            };
            countdownTimer.Start();

            // Starts game loop
            gameTimer.Start();

            // Spawns asteroids every 2s
            Device.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                if (!gameOver) SpawnAsteroid();
                return !gameOver;
            });
        }


        // movement controls
        private void RotateLeft()
        {
            angle -= 15;
            PlayerSprite.Rotation = angle;
        }

        private void RotateRight()
        {
            angle += 15;
            PlayerSprite.Rotation = angle;
        }

        private void MoveForward()
        {
            double radians = Math.PI * angle / 180.0;
            float dx = (float)Math.Cos(radians);
            float dy = (float)Math.Sin(radians);
            position += new Vector2(dx, dy) * (float)speed;

            AbsoluteLayout.SetLayoutBounds(PlayerSprite,
                new Rect(position.X, position.Y, 50, 50));
        }


        private void SpawnAsteroid()
        {
            int size = rng.Next(20, 200);
            // Create asteroid sprite
            var asteroid = new Image
            {
                Source = "asteroid.png",
                WidthRequest = size,
                HeightRequest = size
            };

            // Random spawn
            int edge = rng.Next(4);
            double x = 0, y = 0;
            switch (edge)
            {
                case 0: x = rng.Next(0, 800); y = 0; break;      // Top
                case 1: x = rng.Next(0, 800); y = 600; break;     // Bottom
                case 2: x = 0; y = rng.Next(0, 600); break;      // Left
                case 3: x = 800; y = rng.Next(0, 600); break;     // Right
            }

            // Random speeds
            float vx = (float)(rng.NextDouble() * 4 - 2);
            float vy = (float)(rng.NextDouble() * 4 - 2);

            asteroid.Behaviors.Add(new AsteroidBehavior(new Vector2((float)x, (float)y), new Vector2(vx, vy), size));

            AbsoluteLayout.SetLayoutBounds(asteroid, new Rect(x, y, 40, 40));
            GameArea.Children.Add(asteroid);
            asteroids.Add(asteroid);
        }


        private void Shoot(object sender, EventArgs e) => FireBullet();

        private void FireBullet()
        {
            double radians = Math.PI * angle / 180.0;
            float dx = (float)Math.Cos(radians);
            float dy = (float)Math.Sin(radians);

            var bullet = new BoxView
            {
                Color = Colors.Green,
                WidthRequest = 5,
                HeightRequest = 5
            };

            var startPos = new Vector2(position.X + 25, position.Y + 25);
            bullet.Behaviors.Add(new BulletBehavior(startPos, new Vector2(dx, dy) * 15));

            AbsoluteLayout.SetLayoutBounds(bullet,
                new Rect(startPos.X, startPos.Y, 5, 5));

            GameArea.Children.Add(bullet);
            bullets.Add(bullet);
        }


#if WINDOWS
        private void OnKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Left: RotateLeft(); break;
                case Windows.System.VirtualKey.Right: RotateRight(); break;

                case Windows.System.VirtualKey.Up:
                MoveForward();
                e.Handled = true;
                break;

                case Windows.System.VirtualKey.Space:
                shooting = true;
                e.Handled = true;
                return;

            }
        }

         private void OnKeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        { 
           if (e.Key == Windows.System.VirtualKey.Space)
{
                shooting = false;
                e.Handled = true;
                return;
}

        }

#endif

        private void GameLoop(object sender, EventArgs e)
        {
            if (gameOver) return;

            if (shooting && DateTime.Now - lastShotTime > fireRate)
            {
                FireBullet();
                lastShotTime = DateTime.Now;
            }

            foreach (var asteroid in asteroids.ToList())
            {
                var behavior = asteroid.Behaviors.OfType<AsteroidBehavior>().First();

                // asteroid position
                behavior.Position += behavior.Velocity;
                AbsoluteLayout.SetLayoutBounds(asteroid,
                    new Rect(behavior.Position.X, behavior.Position.Y, 40, 40));

                // hit detection
                double dx = behavior.Position.X - position.X;
                double dy = behavior.Position.Y - position.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < (behavior.Size / 2 + 25)) // hitbox
                {
                    EndGame();
                    break;
                }
            }
            foreach (var bullet in bullets.ToList())
            {
                var behavior = bullet.Behaviors.OfType<BulletBehavior>().First();
                behavior.Position += behavior.Velocity;
                AbsoluteLayout.SetLayoutBounds(bullet,
                    new Rect(behavior.Position.X, behavior.Position.Y, 5, 5));

                // Remove offscreen bullets
                if (behavior.Position.X < 0 || behavior.Position.X > 10000 ||
                    behavior.Position.Y < 0 || behavior.Position.Y > 10000)
                {
                    GameArea.Children.Remove(bullet);
                    bullets.Remove(bullet);
                }

                // counts hits and adds score
                foreach (var asteroid in asteroids.ToList())
                {
                    var ab = asteroid.Behaviors.OfType<AsteroidBehavior>().First();
                    double dx = ab.Position.X - behavior.Position.X;
                    double dy = ab.Position.Y - behavior.Position.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);

                    if (dist < (ab.Size / 2 + 5))
                    {
                        GameArea.Children.Remove(asteroid);
                        GameArea.Children.Remove(bullet);
                        asteroids.Remove(asteroid);
                        bullets.Remove(bullet);
                        score += 100; // 🟢 add score

                        if (score >= highScore)
                        {
                            highScore = score;
                            // save score + update scores
                            Preferences.Set("HighScore", highScore);
                            UpdateScore();

                        }
                       

                        break;

                        
                    }
                }
            }

        }

            // mobile controls
            private void Left(object sender, EventArgs e) => RotateLeft();
            private void Right(object sender, EventArgs e) => RotateRight();
            private void Forward(object sender, EventArgs e) => MoveForward();



        private void EndGame()
        {
            gameOver = true;
            gameTimer.Stop();

            GameMessage.Text = "Game Over";
            StartButton.Text = "Restart";
            Overlay.IsVisible = true;
        }


        private void UpdateScore()
        {
            Score.Text = $"Score: {score}";
            HighScore.Text = $"HighScore: {highScore}";
        }


        class AsteroidBehavior : Behavior<Image>
        {
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public int Size { get; set; }

            public AsteroidBehavior(Vector2 startPos, Vector2 velocity, int size)
            {
                Position = startPos;
                Velocity = velocity;
                Size = size;
            }
        }
        class BulletBehavior : Behavior<BoxView>
            {
                public Vector2 Position { get; set; }
                public Vector2 Velocity { get; set; }
                public BulletBehavior(Vector2 startPos, Vector2 velocity)
                {
                    Position = startPos;
                    Velocity = velocity;
                }
            }

    } 
}
