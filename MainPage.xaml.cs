using System;
using System.Numerics;


namespace repProject
{
    public partial class MainPage : ContentPage
    {
        double angle = 0; // Turn controls (degreees)
        Vector2 position = new Vector2(200, 200); // Ship location 
        double speed = 20; // Forward movement speed

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

        // Mobile controls
        private void Left(object sender, EventArgs e) => RotateLeft();
        private void Right(object sender, EventArgs e) => RotateRight();
        private void Forward(object sender, EventArgs e) => MoveForward();
    }
}
