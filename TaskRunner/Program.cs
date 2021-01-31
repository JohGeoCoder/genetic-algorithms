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
                initialPopulation[i] = new IncrementingBoard(200);
            }

            var runner = new Runner(initialPopulation, 5000);
            runner.Start();
        }
    }
}
