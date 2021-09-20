using System;
using System.Collections.Generic;
using System.Text;

namespace TaskRunner.GeneticStructures
{
    public class RandomGenerator
    {
        private static RandomGenerator Instance;

        private readonly Random Rng;

        private RandomGenerator(int? seed = null)
        {
            if (seed.HasValue)
            {
                Rng = new Random(seed.Value);
            }
            else
            {
                Rng = new Random();
            }
            
        }

        public static RandomGenerator GetInstance(int? seed = null)
        {
            if (Instance == null) Instance = new RandomGenerator(seed);

            return Instance;
        }

        public int Next() => Rng.Next();
        public int Next(int i) => Rng.Next(i);
        public int Next(int i, int j) => Rng.Next(i, j);
        public double NextDouble() => Rng.NextDouble();
        public bool NextBoolean() => Rng.NextDouble() < 0.5;

    }
}
