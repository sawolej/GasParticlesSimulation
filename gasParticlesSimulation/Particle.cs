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
        // RNG for initial positions
        private static Random rand = new Random();

        public double X { get; set; }
        public double Y { get; set; }
        // Properties for velocity
        public bool IsActive { get; set; }  // Add this property to the Particle class

        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public Particle()
        {
            // Initialize with a random position.
            // Here we assume that the container is 800x600 pixels.
            X = rand.NextDouble() * 800;
            Y = rand.NextDouble() * 600;
            IsActive = true;
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

            foreach (var hole in holeLocations)
            {
                double dx = hole.X - X;
                double dy = hole.Y - Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < 15) // Assuming the hole has a radius of 50
                {
                    IsActive = false;
                    break;
                }
            }

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
