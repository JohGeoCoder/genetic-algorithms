using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskRunner.GeneticStructures
{


    public interface IPopulation<T>
    {
        IOrganism<T>[] Organisms { get; }
        decimal MutationRate { get; set; }
        Random Rng { get; }
        public long Score => Organisms.Sum(o => o.Score());

        IOrganism<T> Mate(IOrganism<T> parent1, IOrganism<T> parent2);
        void Mate(IOrganism<T> parent1, IOrganism<T> parent2, IOrganism<T> childRef);
    };

    public interface IOrganism<T>
    {
        T GeneInfo { get; }

        long Score();
    }

    public class Runner<T>
    {
        protected int Iterations { get; }
        protected IPopulation<T> Population { get; }
        protected Random Rng => Population.Rng;
        private IOrganism<T> AbsoluteBestOrganism { get; set; }

        public Runner(IPopulation<T> population, int iterations = 10000)
        {
            Iterations = iterations;
            Population = population;
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
                var bestOrganisms = new HashSet<IOrganism<T>>(sortedOrganisms[0..(Population.Organisms.Length / 10)]);

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

                    Population.Mate(sortedOrganisms[organismIndex1], sortedOrganisms[organismIndex2], Population.Organisms[i]);
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
