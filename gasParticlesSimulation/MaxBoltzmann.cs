using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gasParticlesSimulation
{
    internal class MaxBoltzmann
    {
        private static Random rand = new Random();
        public static double GenerateVelocity(double temperature)
        {
            double mass = 1.0;
            double boltzmannConstant = 1.380649e-23;
            double sigma = Math.Sqrt(boltzmannConstant * temperature / mass);
            double mu = 0.0; // Średnia

            double u1 = 1.0 - rand.NextDouble(); 
            double u2 = 1.0 - rand.NextDouble(); 

            double velocity = sigma * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2) + mu;

            return velocity;
        }
    }
}
