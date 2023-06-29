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
            barrier = new Barrier(particleCount);

            //particles&&visualisation
            particles = new List<Particle>();
            ellipses = new List<Ellipse>();

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

            // canvas
            canvas = new Canvas()
            {
                Background = Brushes.LightBlue
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

            Button createHoleButton = new Button
            {
                Content = "Create Hole",
                Width = 100,
                Height = 30
            };
            Canvas.SetLeft(createHoleButton, 600); 
            Canvas.SetTop(createHoleButton, 10); 

            createHoleButton.Click += (sender, e) =>
            {
                createHoleButton.IsEnabled = false; // Disable
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
                        timer.Dispose(); 
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
            Canvas.SetLeft(speedSlider, 600);
            Canvas.SetTop(speedSlider, 50);
            speedSlider.ValueChanged += (sender, e) =>
            {
                simulationSpeed = (int)e.NewValue;
            };
            canvas.Children.Add(speedSlider);

            // temperature text
            TextBlock temperatureText = new TextBlock
            {
                Text = "Temperature",
                Foreground = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(620, 80, 0, 0)
            };
            canvas.Children.Add(temperatureText);

            // siup
            StartSimulation();
        }

        private void StartSimulation()
        {
            //colors
            speedSlider.ValueChanged += (sender, e) =>
            {
                simulationSpeed = (int)e.NewValue;

                byte red = (byte)(255 * (simulationSpeed - speedSlider.Minimum) / (speedSlider.Maximum - speedSlider.Minimum));
                byte green = (byte)(255 * (speedSlider.Maximum - simulationSpeed) / (speedSlider.Maximum - speedSlider.Minimum));
                byte blue = 200; 
                canvas.Background = new SolidColorBrush(Color.FromRgb(blue, green, red));
            };

            for (int i = 0; i < particleCount; i++)
            {
                int index = i;
                new Thread(() =>
                {
                    while (true)
                    {
                       
                        lock (particles[index])
                        {
                            particles[index].MoveTowardsHole(holes, particles);
                            particles[index].HandleCollision(particles);

                        }

                        // synchronise particles move
                        barrier.SignalAndWait();

         


                        // actualise in main thread
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