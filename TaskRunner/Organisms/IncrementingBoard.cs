using System;
using System.Collections.Generic;
using System.Text;
using TaskRunner.GeneticStructures;

namespace TaskRunner.Populations
{
    public class IncrementingBoard : IOrganism
    {
        public long[] GeneInfo { get; set; }
        private Random Rng { get; } = new Random();

        public IncrementingBoard(int boardSize)
        {            
            GeneInfo = new long[boardSize];
            for (int i = 0; i < boardSize; i++)
            {
                GeneInfo[i] = Rng.Next(0, boardSize);
            }
        }

        public long Score()
        {
            long score = 0;
            for (int i = 0; i < GeneInfo.Length; i++)
            {
                score += (GeneInfo[i] - i) * (GeneInfo[i] - i);
            }

            return score;
        }

        public override string ToString()
        {
            return string.Join(",", GeneInfo);
        }

        public void Mate(IOrganism parent1, IOrganism parent2)
        {
            var parent1Cast = (IncrementingBoard)parent1;
            var parent2Cast = (IncrementingBoard)parent2;

            var cutoff = Rng.Next(1, parent1Cast.GeneInfo.Length - 1);

            for (int i = 0; i < cutoff; i++)
            {
                GeneInfo[i] = parent1Cast.GeneInfo[i];
            }

            for (int i = cutoff; i < parent2Cast.GeneInfo.Length; i++)
            {
                GeneInfo[i] = parent2Cast.GeneInfo[i];
            }
        }

        public void Mutate(decimal probability)
        {
            //Mutation
            if (Rng.Next(0, 100) < 2)
            {
                GeneInfo[Rng.Next(0, GeneInfo.Length)] = Rng.Next(0, GeneInfo.Length);
            }
        }

        public void Clone(IOrganism parent)
        {
            var parentCast = (IncrementingBoard)parent;

            for(int i = 0; i < GeneInfo.Length; i++)
            {
                GeneInfo[i] = parentCast.GeneInfo[i];
            }
        }
    }
}
