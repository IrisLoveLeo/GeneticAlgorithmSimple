using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KangryBaseLib.Logger;

namespace GeneticAlgorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            IEventLogger logger = new CompositeLogger(new ConsoleLogger());

            GeneticAlgorithmSimple ga = new GeneticAlgorithmSimple(logger, 0.25, 0.01, 10, 1000);
            ga.Run();
            ga.WriteResult("GAResult.txt");


            Console.ReadKey();
        }
    }
}
