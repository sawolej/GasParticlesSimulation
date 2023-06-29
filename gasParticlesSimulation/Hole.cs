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
       
        public int particleCount = 0;
        public Point Location { get; private set; }
        public bool IsActive { get; set; }
        public Ellipse HoleEllipse { get; private set; }
        private Thread thread;

        private SemaphoreSlim semaphore = new SemaphoreSlim(10, 10);
        public bool TryEnter(Particle particle)
        {
            if (!semaphore.Wait(0))
                return false;

            try
            {
                double dx = Location.X - particle.X;
                double dy = Location.Y - particle.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < 5 && IsActive)
                {
                    particleCount++;
                    return true;
                }

                return false;
            }
            finally
            {
                semaphore.Release();
            }
        }
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
       
                while (IsActive)
                {
          
                        
                        if (particleCount >= 10)
                        {
                            RemoveFromCanvas();
                            break;
                        }
                    

                    Thread.Sleep(100);
                }
            });
        }

        public void Start()
        {
            thread.Start();
        }

        public void RemoveFromCanvas()
        {
            IsActive = false; 
            if (HoleEllipse != null && HoleEllipse.Parent is Canvas canvas)
            {
                canvas.Dispatcher.Invoke(() => canvas.Children.Remove(HoleEllipse));
            }
        }
    }




}
