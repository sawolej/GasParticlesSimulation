using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Threading;

namespace gasParticlesSimulation
{
    public class Hole
    {
        private readonly object lockObject = new object();
        public int particleCount = 0;
        public Point Location { get; private set; }
        public bool IsActive { get; set; }
        public Ellipse HoleEllipse { get; private set; }
        private Thread thread;
       

        public Hole(Point location, Canvas canvas, Dispatcher dispatcher)
        {
            Location = location;
            IsActive= true;
            HoleEllipse = new Ellipse
            {
                Width = 50,
                Height = 50,
                Fill = Brushes.Black
            };

            Canvas.SetLeft(HoleEllipse, Location.X - HoleEllipse.Width / 2);
            Canvas.SetTop(HoleEllipse, Location.Y - HoleEllipse.Height / 2);
            HoleEllipse.Visibility = Visibility.Visible;

            canvas.Children.Add(HoleEllipse);

            thread = new Thread(() =>
            {
                // Your thread's code here.
                while (IsActive)
                {
                    // Perform the hole's functionality here
                    lock (lockObject)
                    {
                        // Perform any operations related to consuming particles or other logic
                        
                        if (particleCount >= 10)
                        {
                            RemoveFromCanvas();
                            break; // Exit the thread loop when the condition is met
                        }
                    }

                    Thread.Sleep(100); // Sleep to avoid busy waiting
                }
            });
        }

        public void Start()
        {
            thread.Start();
        }

        public void RemoveFromCanvas()
        {
            IsActive = false; // Set the isActive flag to false
            if (HoleEllipse != null && HoleEllipse.Parent is Canvas canvas)
            {
                canvas.Dispatcher.Invoke(() => canvas.Children.Remove(HoleEllipse));
            }
        }
    }




}
