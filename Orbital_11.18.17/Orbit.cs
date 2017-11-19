using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Orbital_11._18._17
{
    class Orbit
    {
        /// <summary>
        /// standard gravitation perameter which is equal to the mass of the main body times G.
        /// </summary>
        public static double U { get; set; }
        /// <summary>
        /// gravitational constant
        /// </summary>
        const double G = 6.67e-11;
        /// <summary>
        /// infinitesimal
        /// </summary>
        const double h = 1e-10;
        /// <summary>
        /// change in time per tick
        /// </summary>
        public const double dt = 0.01;
        public Orbit(double apoapsis,double periapsis, double trueAnomaly = 0,double angleOfThePeriapsis = 0,bool clockwiseOrbit = false)
        {
            Periapsis = periapsis;
            Apoapsis = apoapsis;
            AngleOfThePeriapsis = angleOfThePeriapsis;
            ClockwiseOrbit = clockwiseOrbit;

            SemiMajorAxis = (apoapsis + periapsis) / 2;
            SemiMinorAxis = Math.Sqrt(apoapsis * periapsis);

            MeanAnomaly0 = 0;
            Time0 = 0;

            MeanMotion = Math.Sqrt(U / SemiMajorAxis / SemiMajorAxis / SemiMajorAxis);
            OrbitalPeriod = 2 * Math.PI / MeanMotion;

            Mode = Modes.Rails;

            Eccentricity = (apoapsis - periapsis) / (apoapsis + periapsis);

            double eccentricAnomaly = (Eccentricity + Math.Cos(trueAnomaly) / (1 + Eccentricity * Math.Cos(trueAnomaly)));
            double meanAnomaly = eccentricAnomaly - Eccentricity * Math.Sin(eccentricAnomaly);
            Time = meanAnomaly / MeanMotion;
            Time %= OrbitalPeriod;

            Position = GetPosition(Time);
        }
        public void SetTime(double t)
        {
            t %= OrbitalPeriod;
            Time = t;
            Position = GetPosition(t);
        }
        public double GetSpeed() => GetSpeed(Position.Length);
        public double GetSpeed(double radius) => Math.Sqrt(U * (2 / radius - 1 / SemiMajorAxis));
        public Vector2d Position { get; private set; }
        public Vector2d Velocity
        {
            get
            {
                if (Mode == Modes.Physics || _velocityIsRefreshed)
                    return _velocity;
                else
                {
                    _velocity = (GetPosition(Time + h) - Position) / h;
                    _velocityIsRefreshed = true;
                    return _velocity;
                }
            }
            set
            {
                _velocity = value;
            }

        }
        Vector2d _velocity;
        bool _velocityIsRefreshed = false;
        public double Time { get; private set; }
        public double Apoapsis { get; private set; }
        public double Periapsis { get; private set; }
        public double Eccentricity { get; set; }
        public double AngleOfThePeriapsis { get; private set; }
        public bool ClockwiseOrbit { get; private set; }
        public double SemiMajorAxis { get; private set; }
        double SemiMinorAxis { get; set; }
        double MeanMotion { get; set; }
        public double OrbitalPeriod { get; private set; }
        /// <summary>
        /// mean anomaly at epoch
        /// </summary>
        double MeanAnomaly0 { get; set; }
        /// <summary>
        /// the time that the orbital elements are specified
        /// </summary>
        double Time0 { get; set; }
        public Modes Mode { get; private set; }
        public enum Modes
        {
            Rails = 0,
            Physics,
        }
        public void Update()
        {
            Time += dt;
            if (Mode == Modes.Rails)
            {
                Time %= OrbitalPeriod;
                Position = GetPosition(Time);
            }
            else
            {
                Position += Velocity * dt;
            }

        }
        /// <summary>
        /// calculates the orbital state vectors from the orbital elements
        /// </summary>
        public void ToPhysicsMode()
        {
            var n = Velocity;
            Mode = Modes.Physics;

        }
        /// <summary>
        /// calculates the orbital elements from the orbital state vectors
        /// </summary>
        public void ToRailsMode()
        {
            //is on the third (z) dimension
            double h = Position.X * Velocity.Y - Position.Y * Velocity.X;
            Vector2d eccentricityVector = ((Velocity.LengthSquared - U / Position.Length) * Position - Vector2d.Dot(Position, Velocity) * Velocity)/U;
            Eccentricity = eccentricityVector.Length;
            double specificOrbitalEnergy = Velocity.LengthSquared / 2 - U / Position.Length;
            SemiMajorAxis = -U / 2 / specificOrbitalEnergy;//
            
            Periapsis = SemiMajorAxis * (1 - Eccentricity);//
            Apoapsis = SemiMajorAxis * (1 + Eccentricity);//

            SemiMinorAxis = Math.Sqrt(Periapsis * Apoapsis);
            double trueAnomaly = Math.Acos(Vector2d.Dot(eccentricityVector, Position) / Eccentricity / Position.Length);
            if (Vector2d.Dot(Position, Velocity) < 0)
            {
                ClockwiseOrbit = true;
                trueAnomaly = 2 * Math.PI - trueAnomaly;
            }

            MeanMotion = Math.Sqrt(U / SemiMajorAxis / SemiMajorAxis / SemiMajorAxis);
            OrbitalPeriod = 2 * Math.PI / MeanMotion;
            Time %= OrbitalPeriod;
            Time0 = Time;
            double eccentricAnomaly = (Eccentricity+Math.Cos(trueAnomaly)/(1+Eccentricity*Math.Cos(trueAnomaly)));
            MeanAnomaly0 = eccentricAnomaly - Eccentricity * Math.Sin(Eccentricity);
            AngleOfThePeriapsis = trueAnomaly - Math.Atan2(Position.Y, Position.X);

            Mode = Modes.Rails;
        }
        public Vector2d GetPosition(double t)
        {

            double meanAnomaly = MeanAnomaly0 + MeanMotion * (t - Time0);
            double eccentricAnomaly = meanAnomaly,last=int.MaxValue;
            if (Eccentricity != 0)
            {
                for (int i = 0; eccentricAnomaly != last && i < 1000000; i++) 
                {
                    last = eccentricAnomaly;
                    eccentricAnomaly -= (eccentricAnomaly - Eccentricity * Math.Sin(eccentricAnomaly) - meanAnomaly) / (1 - Eccentricity * Math.Cos(eccentricAnomaly));

                }
            }
            double trueAnomaly = Math.Acos((Math.Cos(eccentricAnomaly) - Eccentricity) / (1 - Eccentricity * Math.Cos(eccentricAnomaly)));
            double radius = SemiMajorAxis * (1 - Eccentricity * Eccentricity) / (1 + Eccentricity * Math.Cos(trueAnomaly));
            if (t > OrbitalPeriod / 2)
                trueAnomaly = 2 * Math.PI - trueAnomaly;
            trueAnomaly -= AngleOfThePeriapsis;
            return new Vector2d(Math.Cos(trueAnomaly), Math.Sin(trueAnomaly)) * radius;
        }
        public double GetRadius(double trueAnomaly) => SemiMajorAxis * (1 - Eccentricity * Eccentricity) / (1 + Eccentricity * Math.Cos(trueAnomaly-AngleOfThePeriapsis));
    }
}
