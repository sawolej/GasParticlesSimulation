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
        Slider speedSlider;

        private List<Hole> holes = new List<Hole>();

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
                    Fill = Brushes.White
                };
                ellipses.Add(ellipse);
            }

            // Tworzymy płótno do wyświetlania cząstek
            canvas = new Canvas()
            {
                Background = Brushes.LightBlue // setting background color for the Canvas
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
            holeEllipse.Visibility = Visibility.Hidden;

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
                createHoleButton.IsEnabled = false; // Disable the button
                Random rand = new Random();
                Point newHoleLocation = new Point(rand.Next(50, 750), rand.Next(50, 550));

                Hole newHole = new Hole(newHoleLocation, canvas, Dispatcher);
                newHole.Start();
                holes.Add(newHole);

                // Enable the button after 2 seconds
                Timer timer = null;
                timer = new Timer((state) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        createHoleButton.IsEnabled = true;
                        timer.Dispose(); // Dispose the timer to prevent further execution
                    });
                }, null, 300, Timeout.Infinite);
            };

            canvas.Children.Add(createHoleButton);


            //slider 
            speedSlider = new Slider
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

            // Temperature text
            TextBlock temperatureText = new TextBlock
            {
                Text = "Temperature",
                Foreground = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(620, 80, 0, 0)
            };
            canvas.Children.Add(temperatureText);

            // Uruchamiamy symulację w tle
            StartSimulation();
        }

        private void StartSimulation()
        {
            speedSlider.ValueChanged += (sender, e) =>
            {
                simulationSpeed = (int)e.NewValue;

                // Calculate color values for the gradient based on the slider value
                // Calculate color values for the gradient based on the slider value
                byte red = (byte)(255 * (simulationSpeed - speedSlider.Minimum) / (speedSlider.Maximum - speedSlider.Minimum));
                byte green = (byte)(255 * (speedSlider.Maximum - simulationSpeed) / (speedSlider.Maximum - speedSlider.Minimum));
                byte blue = 200; // Set a fixed blue value for lightness

                // Set the background color of the canvas
                canvas.Background = new SolidColorBrush(Color.FromRgb(blue, green, red));
            };

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
                            particles[index].MoveTowardsHole(holes, particles);
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



    }
}