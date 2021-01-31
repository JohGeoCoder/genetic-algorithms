using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskRunner.GeneticStructures
{


    public class Population
    {
        public IOrganism[] Organisms { get; }
        public Random Rng { get; }
        public decimal MutationRate { get; set; }
        public long Score => Organisms.Sum(o => o.Score());

        public Population(IEnumerable<IOrganism> initialPopulation, Random rng)
        {
            Rng = rng;
            Organisms = initialPopulation.ToArray();
        }
    };

    public interface IOrganism
    {
        long Score();

        void Mate(IOrganism parent1, IOrganism parent2);

        void Mutate(decimal probability);
    }

    public class Runner
    {
        protected int Iterations { get; }
        protected Population Population { get; }
        protected Random Rng { get; } = new Random();
        protected decimal MutationRate { get; set; }
        private IOrganism AbsoluteBestOrganism { get; set; }

        public Runner(IEnumerable<IOrganism> initialPopulation, int iterations = 10000, decimal mutationRate = 0.02m)
        {
            Iterations = iterations;
            Population = new Population(initialPopulation, Rng);
            MutationRate = mutationRate;
        }

        public void Start()
        {
            int iteration = 0;

            while(iteration < Iterations)
            {
                iteration++;

                var scoredOrganisms = Population.Organisms.Select(o => new
                {
                    Score = o.Score(),
                    Organism = o
                });

                //Sort the organisms by score
                var sortedOrganisms = scoredOrganisms
                    .OrderBy(o => o.Score)
                    .Select(o => o.Organism)
                    .ToArray();

                //Keep track of the absolute best organism.
                var currentBestOrganism = sortedOrganisms.First();
                if(AbsoluteBestOrganism == null || currentBestOrganism.Score() < AbsoluteBestOrganism.Score())
                {
                    AbsoluteBestOrganism = currentBestOrganism;
                }

                //Keep the best organisms
                var bestOrganisms = new HashSet<IOrganism>(sortedOrganisms[0..(Population.Organisms.Length / 10)]);

                //Generate new organisms with the best organisms
                for(int i = 0; i < Population.Organisms.Length; i++)
                {
                    if (bestOrganisms.Contains(Population.Organisms[i]))
                    {
                        continue;
                    }

                    //Replace this organism with an offspring of two random best organisms.
                    var organismIndex1 = Rng.Next(0, (Population.Organisms.Length / 10) - 1);
                    var organismIndex2 = Rng.Next(organismIndex1 + 1, Population.Organisms.Length / 10);

                    Population.Organisms[i].Mate(sortedOrganisms[organismIndex1], sortedOrganisms[organismIndex2]);

                    Population.Organisms[i].Mutate(MutationRate);
                }

                Console.WriteLine($"Iteration {iteration}: {Population.Score}");
            }

            var bestOrganism = Population.Organisms.Select(o => new
            {
                Score = o.Score(),
                Organism = o
            })
            .OrderBy(o => o.Score)
            .First();

            Console.WriteLine("Final Population Best Organism");
            Console.WriteLine(bestOrganism.Score);
            Console.WriteLine(bestOrganism.ToString());

            Console.WriteLine();
            Console.WriteLine("Absolute Best Organism");
            Console.WriteLine(AbsoluteBestOrganism.Score());
            Console.WriteLine(AbsoluteBestOrganism.ToString());

            return;
        }
    }
}
