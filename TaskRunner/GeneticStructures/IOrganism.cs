using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        void Clone(IOrganism parent);
    }

    public class Runner
    {
        protected int Iterations { get; }
        protected Population Population { get; set; }
        protected Population NextGeneration { get; set; }
        protected Random Rng { get; } = new Random();
        protected decimal MutationRate { get; set; }
        protected int MatePopulationCutoff { get; set; }
        protected int KeepTopCutoff { get; set; }
        private IOrganism AbsoluteBestOrganism { get; set; }

        public Runner(IEnumerable<IOrganism> initialPopulation, int iterations = 10000, decimal mutationRate = 0.02m, int matePopulationCutoff = 20, int keepTopCutoff = 5)
        {
            Iterations = iterations;
            Population = new Population(initialPopulation, Rng);
            NextGeneration = new Population(initialPopulation, Rng);
            MutationRate = mutationRate;
            MatePopulationCutoff = matePopulationCutoff;
            KeepTopCutoff = keepTopCutoff;
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
                var matePopulation = new HashSet<IOrganism>(sortedOrganisms[0..MatePopulationCutoff]);
                var survivors = new HashSet<IOrganism>(sortedOrganisms[0..KeepTopCutoff]);

                //Generate new organisms with the best organisms
                for(int i = 0; i < Population.Organisms.Length; i++)
                {
                    //Clone the survivors over to the next generation
                    if (survivors.Contains(Population.Organisms[i]))
                    {
                        NextGeneration.Organisms[i].Clone(Population.Organisms[i]);
                        continue;
                    }

                    //Replace this organism with an offspring of two random best organisms.
                    var parentIndex1 = Rng.Next(0, MatePopulationCutoff - 1);
                    var parentIndex2 = Rng.Next(parentIndex1 + 1, MatePopulationCutoff);

                    var parent1 = sortedOrganisms[parentIndex1];
                    var parent2 = sortedOrganisms[parentIndex2];

                    NextGeneration.Organisms[i].Mate(parent1, parent2);
                    NextGeneration.Organisms[i].Mutate(MutationRate);
                }

                //Switch the current population and the next generation.
                var tempGeneration = Population;
                Population = NextGeneration;
                NextGeneration = Population;

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
