using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gasParticlesSimulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Canvas canvas;
        private List<Particle> particles;
        private List<Ellipse> ellipses;
        private Barrier barrier;
        private List<Point> holeLocations = new List<Point>();
        private List<Ellipse> holeEllipses = new List<Ellipse>();
        private Point? holeLocation = null;
        private Ellipse holeEllipse = null;
        private int simulationSpeed = 10;
        bool isHole = false;
        public MainWindow()
        {
            InitializeComponent();
            this.Width = 800;
            this.Height = 600;
            // Ustalamy liczbę cząstek i tworzymy barierę
            int particleCount = 100;
            barrier = new Barrier(particleCount);

            // Tworzymy listę cząstek i ich wizualizacji
            particles = new List<Particle>();
            ellipses = new List<Ellipse>();

            // Inicjalizujemy cząstki i ich wizualizacje
            for (int i = 0; i < particleCount; i++)
            {
                Particle particle = new Particle();
                particles.Add(particle);

                Ellipse ellipse = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Red
                };
                ellipses.Add(ellipse);
            }

            // Tworzymy płótno do wyświetlania cząstek
            canvas = new Canvas()
            {
                Background = Brushes.LightGray // setting background color for the Canvas
            };
            foreach (Ellipse ellipse in ellipses)
            {
                canvas.Children.Add(ellipse);
            }
            Content = canvas;

            //hole
            holeEllipse = new Ellipse
            {
                Width = 50,
                Height = 50,
                Fill = Brushes.Black
            };
            canvas.Children.Add(holeEllipse);
            holeEllipse.Visibility = Visibility.Hidden;  // Hide it initially

            // Add a button to create a hole
            Button createHoleButton = new Button
            {
                Content = "Create Hole",
                Width = 100,
                Height = 30
            };
            Canvas.SetLeft(createHoleButton, 700); // Place it on the right side
            Canvas.SetTop(createHoleButton, 10); // Place it on the top
            createHoleButton.Click += (sender, e) =>
            {
                Random rand = new Random();

               

                // Create a hole in the middle of the canvas
                Point newHoleLocation = new Point(rand.Next(50, 750), rand.Next(50, 550));
                holeLocations.Add(newHoleLocation);

                // Create a new ellipse for this hole
                Ellipse newHoleEllipse = new Ellipse
                {
                    Width = 50,
                    Height = 50,
                    Fill = Brushes.Black
                };
                holeEllipses.Add(newHoleEllipse);
                canvas.Children.Add(newHoleEllipse);

                // Show the hole ellipse
                Canvas.SetLeft(newHoleEllipse, newHoleLocation.X - newHoleEllipse.Width / 2);
                Canvas.SetTop(newHoleEllipse, newHoleLocation.Y - newHoleEllipse.Height / 2);
                newHoleEllipse.Visibility = Visibility.Visible;
            };
            canvas.Children.Add(createHoleButton);

            //slider 
            Slider speedSlider = new Slider
            {
                Minimum = 1,
                Maximum = 100,
                Value = simulationSpeed,
                Width = 100,
                Height = 30,
            };
            Canvas.SetLeft(speedSlider, 700); // Place it on the right side
            Canvas.SetTop(speedSlider, 50); // Place it below the button
            speedSlider.ValueChanged += (sender, e) =>
            {
                simulationSpeed = (int)e.NewValue;
            };
            canvas.Children.Add(speedSlider);

            // Uruchamiamy symulację w tle
            for (int i = 0; i < particleCount; i++)
            {
                int index = i;
                new Thread(() =>
                {
                    while (true)
                    {
                        //if (isHole) particles[index].Move();
                        //[index].MoveTowardsHole(holeLocation); 

                        // Blokada dla obsługi kolizji
                        lock (particles[index])
                        {
                            particles[index].MoveTowardsHole(holeLocations, particles);
                            particles[index].HandleCollision(particles);

                        }

                        // Bariera synchronizująca ruchy wszystkich cząstek
                        barrier.SignalAndWait();

                        // Aktualizujemy wizualizację cząstki w głównym wątku
                        Dispatcher.Invoke(() =>
                        {
                            Canvas.SetLeft(ellipses[index], particles[index].X);
                            Canvas.SetTop(ellipses[index], particles[index].Y);
                        });
                        Thread.Sleep(simulationSpeed);
                    }
                }).Start();
            }
        }

        public class Particle
        {
            // RNG for initial positions
            private static Random rand = new Random();

            public double X { get; set; }
            public double Y { get; set; }
            // Properties for velocity
            public double VelocityX { get; set; }
            public double VelocityY { get; set; }
            public Particle()
            {
                // Initialize with a random position.
                // Here we assume that the container is 800x600 pixels.
                X = rand.NextDouble() * 800;
                Y = rand.NextDouble() * 600;
            }

            public void Move()
            {
                // Randomly adjust the velocities
                VelocityX += (rand.NextDouble() - 0.5) * 0.1;
                VelocityY += (rand.NextDouble() - 0.5) * 0.1;

                // Move the particle
                X += VelocityX;
                Y += VelocityY;

                // Check for out-of-bounds and wrap around
                if (X < 0) X += 800;
                if (X > 800) X -= 800;
                if (Y < 0) Y += 600;
                if (Y > 600) Y -= 600;
            }
            public void MoveTowardsHole(List<Point> holeLocations, List<Particle> particles)
            {
                // If there are no holes, move randomly
                if (holeLocations.Count == 0)
                {
                    Move();
                    return;
                }

                // Find the closest hole
                Point closestHole = holeLocations.OrderBy(h => Math.Sqrt((h.X - X) * (h.X - X) + (h.Y - Y) * (h.Y - Y))).First();

                // If there's a hole, move towards it
                var directionX = closestHole.X - X;
                var directionY = closestHole.Y - Y;

                // Normalize the direction
                var directionLength = Math.Sqrt(directionX * directionX + directionY * directionY);
                directionX /= directionLength;
                directionY /= directionLength;

                // Predict the next position
                double nextX = X + directionX * 5;
                double nextY = Y + directionY * 5;

                // Check for collisions with other particles
                foreach (Particle other in particles)
                {
                    // Don't collide with yourself
                    if (ReferenceEquals(this, other)) continue;

                    double dx = other.X - nextX;
                    double dy = other.Y - nextY;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    // If a collision is detected, adjust the velocity
                    if (distance < 10) // Assuming the particle diameter to be 10
                    {
                        VelocityX = -VelocityX;
                        VelocityY = -VelocityY;
                    }
                }

                // Move the particle
                X += directionX * 5;
                Y += directionY * 5;

                // Check for out-of-bounds and wrap around
                if (X < 0) X += 800;
                if (X > 800) X -= 800;
                if (Y < 0) Y += 600;
                if (Y > 600) Y -= 600;
            }


            private double vX;  // Velocity in the X direction
            private double vY;  // Velocity in the Y direction

            public void HandleCollision(List<Particle> particles)
            {
                foreach (Particle other in particles)
                {
                    // Don't collide with yourself
                    if (ReferenceEquals(this, other)) continue;

                    // Calculate the distance between this particle and the other
                    double dx = other.X - X;
                    double dy = other.Y - Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    // If the distance is less than some threshold (here, the size of a particle)...
                    if (distance < 10)
                    {
                        // Swap velocities
                        double tempVX = VelocityX;
                        double tempVY = VelocityY;
                        VelocityX = other.VelocityX;
                        VelocityY = other.VelocityY;
                        other.VelocityX = tempVX;
                        other.VelocityY = tempVY;
                    }
                }
            }

            // ... rest of your Particle code here
        }


    }
}