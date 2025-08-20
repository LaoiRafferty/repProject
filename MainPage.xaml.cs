using System;
using System.Numerics;
using System.Timers;
using Microsoft.Maui.Dispatching;


namespace repProject
{
    public partial class MainPage : ContentPage
    {
        double angle = 0; // Turn controls (degreees)
        Vector2 position = new Vector2(200, 200); // Ship location 
        double speed = 20; // Forward movement speed

        IDispatcherTimer gameTimer;
        Random rng = new Random();
        List<Image> asteroids = new List<Image>();
        bool gameOver = false;

        public MainPage()
        {
            InitializeComponent();


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
        

// movement controls
private void RotateLeft()
        {
            angle -= 10;
            PlayerSprite.Rotation = angle;
        }

        private void RotateRight()
        {
            angle += 10;
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
            // Create asteroid sprite
            var asteroid = new Image
            {
                Source = "asteroid1.jpg",
                WidthRequest = 40,
                HeightRequest = 40
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

            asteroid.Behaviors.Add(new AsteroidBehavior(new Vector2((float)x, (float)y), new Vector2(vx, vy)));

            AbsoluteLayout.SetLayoutBounds(asteroid, new Rect(x, y, 40, 40));
            GameArea.Children.Add(asteroid);
            asteroids.Add(asteroid);
        }

        



#if WINDOWS
               
                private void OnKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
                {
                    switch (e.Key)
                    {
                        case Windows.System.VirtualKey.Left:
                            RotateLeft();
                            break;
                        case Windows.System.VirtualKey.Right:
                            RotateRight();
                            break;
                        case Windows.System.VirtualKey.Up:
                            MoveForward();
                            break;
                    }
                }
#endif

        private void GameLoop(object sender, EventArgs e)
        {
            if (gameOver) return;

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

                if (distance < 30) // hitbox
                {
                    EndGame();
                    break;
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

            DisplayAlert("Game Over", "You died", "OK");
        }

    }  

    class AsteroidBehavior : Behavior<Image>
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }

    public AsteroidBehavior(Vector2 startPos, Vector2 velocity)
    {
        Position = startPos;
        Velocity = velocity;
    }
}

}
