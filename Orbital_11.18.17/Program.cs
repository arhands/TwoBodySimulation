using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using OpenTK;

namespace Orbital_11._18._17
{
    class Program
    {
        static int Sqr(int n) => n * n;
        static void Main(string[] args)
        {
            Orbit.U = 100000;
            Bitmap bmp = InitBMP(4000, 4000);
            Orbit test = new Orbit(1990, 100);
            Draw(test, bmp);
            test = new Orbit(1500, 85,0,4);
            Draw(test, bmp);
            test = new Orbit(1990, 100);
            Draw(test, bmp);
            test = new Orbit(500, 6);
            Draw(test, bmp);
            bmp.Save("image.bmp");
        }
        static Bitmap InitBMP(int width,int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    if (Sqr(i - bmp.Width / 2) + Sqr(j - bmp.Height / 2) <= 25)
                    {
                        bmp.SetPixel(i, j, Color.Yellow);
                    }
                    else if (i % 100 == 0 || j % 100 == 0)
                    {
                        bmp.SetPixel(i, j, Color.BlueViolet);
                    }
                    else bmp.SetPixel(i, j, Color.Black);
                }
            }
            return bmp;
        }
        static void Draw(Orbit or,Bitmap bmp)
        {
            or.Update();
            double diviser = or.GetSpeed(or.Periapsis);

            for (double i = 0; i <= 2*Math.PI; i+=0.001)
            {
                Vector2d pos = new Vector2d(Math.Cos(i), Math.Sin(i)) * or.GetRadius(i);
                int amount = 1;
                if (diviser != 0)
                    amount = (int)(255 * or.GetSpeed(pos.Length) / diviser);
                bmp.SetPixel((int)pos.X + bmp.Width / 2, (int)pos.Y + bmp.Height / 2, Color.FromArgb(amount, 255 - amount, 0));
                if (i % 100 == 0)
                    Console.WriteLine((int)(100 * i / or.OrbitalPeriod * Orbit.dt) + "%");
            }
        }
    }
}
