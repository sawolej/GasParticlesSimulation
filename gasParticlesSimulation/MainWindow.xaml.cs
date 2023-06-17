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
        private int particleCount = 100;
        private Button restartButton;
        public MainWindow()
        {
            InitializeComponent();
            this.Width = 800;
            this.Height = 600;
            // Ustalamy liczbę cząstek i tworzymy barierę
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
            Canvas.SetLeft(createHoleButton, 600); // Place it on the right side
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

            // Initialize the restart button
            restartButton = new Button
            {
                Content = "Restart Simulation",
                Width = 150,
                Height = 30,
                IsEnabled = false  // Disabled by default
            };
            Canvas.SetLeft(restartButton, 600);
            Canvas.SetTop(restartButton, 90);  // Adjust this value as needed
            restartButton.Click += (sender, e) => RestartSimulation();
            canvas.Children.Add(restartButton);

            //slider 
            Slider speedSlider = new Slider
            {
                Minimum = 1,
                Maximum = 100,
                Value = simulationSpeed,
                Width = 100,
                Height = 30,
            };
            Canvas.SetLeft(speedSlider, 600); // Place it on the right side
            Canvas.SetTop(speedSlider, 50); // Place it below the button
            speedSlider.ValueChanged += (sender, e) =>
            {
                simulationSpeed = (int)e.NewValue;
            };
            canvas.Children.Add(speedSlider);

            // Uruchamiamy symulację w tle
            StartSimulation();
        }

        private void StartSimulation()
        {
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

                        if (!particles.Any(p => p.IsActive))
                        {
                            // If all particles are inactive, disable the simulation and enable the restart button
                            Dispatcher.Invoke(() =>
                            {
                                restartButton.IsEnabled = true;
                            });

                            // Break out of the loop to terminate the thread
                            break;
                        }


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
        private void RestartSimulation()
        {
            // Clear the old particles and holes
            particles.Clear();
            holeLocations.Clear();
            foreach (var ellipse in ellipses) canvas.Children.Remove(ellipse);
            foreach (var holeEllipse in holeEllipses) canvas.Children.Remove(holeEllipse);
            ellipses.Clear();
            holeEllipses.Clear();

            // Initialize a new set of particles
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
                canvas.Children.Add(ellipse);
            }

            // Start the simulation loop again
            restartButton.IsEnabled = false;
            StartSimulation();
        }



    }
}