using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Froggun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static DispatcherTimer timer = new DispatcherTimer();

        private static Vector2 playerPosition = new Vector2();
        private static Vector2 playerVelocity = new Vector2();
        private const float gravity = 0.5f;
        private const float jumpForce = 15.0f;
        private const float maxFallSpeed = 9.8f;
        private const float moveSpeed = 8.0f;
        private const float friction = 0.8f;

        private bool isOnGround = false;
        private bool isGroundSlaming = false;
        private bool isMovementLocked = false;
        private bool moveLeft = false;
        private bool moveRight = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
        }

        void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16.6666667);
            timer.Tick += Loop;
            timer.Start();
        }

        private void Loop(object? sender, EventArgs e)
        {
            int maxY = (int) grid.ActualHeight;
            Console.WriteLine($"{isGroundSlaming} {isMovementLocked} {isOnGround} {Math.Round(playerVelocity.X, 1)}  {Math.Round(playerVelocity.Y, 1)}    //    {playerPosition.X}  {playerPosition.Y}");

            // Check player state to see if we need to lock its movement
            if (isGroundSlaming) isMovementLocked = true;
            else isMovementLocked = false;

            if (isMovementLocked)
            {
                // lock the player movement
                if (isGroundSlaming)
                {
                    playerVelocity.Y = maxFallSpeed * 4.0f;
                    playerPosition.Y += playerVelocity.Y;

                    Canvas.SetLeft(player, playerPosition.X);
                    Canvas.SetTop(player, playerPosition.Y);

                    if (playerPosition.Y >= maxY - player.Height) isGroundSlaming = false;
                }
            }
            else
            {
                //move player down
                if (playerVelocity.Y < maxFallSpeed) playerVelocity.Y += gravity;
                else playerVelocity.Y = maxFallSpeed;

                // Apply vertical velocity
                playerPosition.Y += playerVelocity.Y;

                //plyr is on the ground
                if (playerPosition.Y >= maxY - player.Height)
                {
                    playerPosition.Y = maxY - (float)player.Height;
                    isOnGround = true;
                    playerVelocity.Y = 0;
                }
                else isOnGround = false;
                if (moveRight) playerVelocity.X = moveSpeed;  // Move right
                else if (moveLeft) playerVelocity.X = -moveSpeed; // Move left

                else
                {
                    // lower player verlocity based on friction
                    playerVelocity.X *= friction;
                    // if the velocity (forced to be positive) is < than 0.1f stop moving
                    if (Math.Abs(playerVelocity.X) < 0.1f) playerVelocity.X = 0;
                }

                playerPosition.Y += playerVelocity.Y;
                playerPosition.X += playerVelocity.X;

                Canvas.SetLeft(player, playerPosition.X);
                Canvas.SetTop(player, playerPosition.Y);
            }
        }

        private void keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                playerVelocity.Y = -jumpForce;
            }
            if (e.Key == Key.LeftCtrl)
            {
                if (!isOnGround) isGroundSlaming = true;
            }

            if (e.Key == Key.D)
            {
                moveRight = true;
                moveLeft = false;
            }
            if (e.Key == Key.Q || e.Key == Key.A)
            {
                moveLeft = true;
                moveRight = false;
            }
        }

        private void keyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D)
            {
                moveRight = false;
            }
            if (e.Key == Key.Q || e.Key == Key.A)
            {
                moveLeft = false;
            }
        }
    }
}