using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;
using TaskRunner.GeneticStructures;
using TaskRunner.Populations;

namespace TaskRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var rng = new Random();

            var initialPopulation = new IncrementingBoard[1000];
            for(int i = 0; i < initialPopulation.Length; i++)
            {
                initialPopulation[i] = new IncrementingBoard(1000);
            }

            var runner = new Runner(initialPopulation, iterations: 5000, matePopulationCutoff: 200, keepTopCutoff: 20);

            runner.Start();

            Console.ReadKey();
        }
    }
}
