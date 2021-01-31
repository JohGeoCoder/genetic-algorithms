using System;
using System.Collections.Generic;
using System.Text;
using TaskRunner.GeneticStructures;

namespace TaskRunner.Populations.IncrementingBoardPopulation
{
    public class IncrementingBoardPopulation : IPopulation<long[]>
    {
        public IOrganism<long[]>[] Organisms { get; }
        public decimal MutationRate { get; set; }
        public Random Rng { get; } = new Random();

        public IncrementingBoardPopulation(int populationSize, int boardSize, Random rng)
        {
            Rng = rng;

            Organisms = new IncrementingBoard[populationSize];
            for(int i = 0; i < populationSize; i++)
            {
                Organisms[i] = new IncrementingBoard(boardSize, rng);
            }
        }

        public IOrganism<long[]> Mate(IOrganism<long[]> parent1, IOrganism<long[]> parent2)
        {
            throw new NotImplementedException();
        }

        public void Mate(IOrganism<long[]> parent1, IOrganism<long[]> parent2, IOrganism<long[]> childRef)
        {
            var cutoff = Rng.Next(1, parent1.GeneInfo.Length - 1);

            for (int i = 0; i < cutoff; i++)
            {
                childRef.GeneInfo[i] = parent1.GeneInfo[i];
            }

            for (int i = cutoff; i < parent2.GeneInfo.Length; i++)
            {
                childRef.GeneInfo[i] = parent2.GeneInfo[i];
            }

            //Mutation
            if (Rng.Next(0, 100) < 2)
            {
                childRef.GeneInfo[Rng.Next(0, childRef.GeneInfo.Length)] = Rng.Next(0, childRef.GeneInfo.Length);
            }
        }
    }
}
