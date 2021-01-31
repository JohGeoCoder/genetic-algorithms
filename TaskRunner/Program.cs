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

            var initialPopulation = new IncrementingBoard[200];
            for(int i = 0; i < initialPopulation.Length; i++)
            {
                initialPopulation[i] = new IncrementingBoard(200, rng);
            }

            var runner = new Runner(new Population(initialPopulation, rng), 5000);
            runner.Start();
        }
    }
}
