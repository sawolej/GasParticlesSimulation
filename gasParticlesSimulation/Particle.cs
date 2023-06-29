using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class Particle
    {
        //----------------------- params
        private static Random rand = new Random();
       // private static SemaphoreSlim semaphore = new SemaphoreSlim(10, 10);
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsActive { get; set; }  

        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public Particle()
        {
            X = rand.NextDouble() * 800;
            Y = rand.NextDouble() * 600;
            IsActive = true;
        }

        public void Move()
        {

            //random velocity and move
            VelocityX += (rand.NextDouble() - 0.5) * 0.1;
            VelocityY += (rand.NextDouble() - 0.5) * 0.1;

            X += VelocityX;
            Y += VelocityY;

            //dont let it go!
            if (X < 0) X += 800;
            if (X > 800) X -= 800;
            if (Y < 0) Y += 600;
            if (Y > 600) Y -= 600;
        }
        public void MoveTowardsHole(List<Hole> holes, List<Particle> particles)
        {
          
                // if no holes or all holes are inactive, go random
                if (holes.Count == 0 || holes.All(h => !h.IsActive))
                {
                    Move();
                    return;
                }

                // find the closest active hole and calculate direction toward
                Hole closestHole = holes.Where(h => h.IsActive)
                    .OrderBy(h => Math.Sqrt((h.Location.X - X) * (h.Location.X - X) + (h.Location.Y - Y) * (h.Location.Y - Y)))
                    .FirstOrDefault();

                // if no active hole found, go random
                if (closestHole == null)
                {
                    Move();
                    return;
                }

                var directionX = closestHole.Location.X - X;
                var directionY = closestHole.Location.Y - Y;

                var directionLength = Math.Sqrt(directionX * directionX + directionY * directionY);
                directionX /= directionLength;
                directionY /= directionLength;

                // calculate next position
                double nextX = X + directionX * 5;
                double nextY = Y + directionY * 5;

                // check if there is any collision

                foreach (Particle other in particles)
                {

                    if (ReferenceEquals(this, other)) continue;

                    double dx = other.X - nextX;
                    double dy = other.Y - nextY;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance < 10)
                    {
                        VelocityX = -VelocityX;
                        VelocityY = -VelocityY;
                    }

                }

                X += directionX * 5;
                Y += directionY * 5;

                if (X < 0) X += 800;
                if (X > 800) X -= 800;
                if (Y < 0) Y += 600;
                if (Y > 600) Y -= 600;

            closestHole.TryEnter(this);
           

        }




        private double vX; 
        private double vY; 

        public void HandleCollision(List<Particle> particles)
        {
            foreach (Particle other in particles)
            {
                if (ReferenceEquals(this, other)) continue;

                double dx = other.X - X;
                double dy = other.Y - Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < 10)
                {
                    double tempVX = VelocityX;
                    double tempVY = VelocityY;
                    VelocityX = other.VelocityX;
                    VelocityY = other.VelocityY;
                    other.VelocityX = tempVX;
                    other.VelocityY = tempVY;
                }
            }
        }

    }
}
