using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace NumericLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            double[] x = { 0, 2000, 4000, 6000, 8000, 10000, 12000, 14000, 16000, 18000};
            double[] y = { 2, 2001, 4003, 6005, 7999, 10000, 11997, 14005, 15888, 18032};
            Tuple<double, double> s = new Tuple<double, double>(0, 0);
            s = Fit.Line(x,y);
            Console.WriteLine(s.Item1);
            Console.WriteLine(s.Item2);
            Console.ReadKey();
        }


    }
}
