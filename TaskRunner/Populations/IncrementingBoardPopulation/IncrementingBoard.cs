using System;
using System.Collections.Generic;
using System.Text;
using TaskRunner.GeneticStructures;

namespace TaskRunner.Populations
{
    public class IncrementingBoard : IOrganism<long[]>
    {
        public long[] GeneInfo { get; set; }
        private Random Rng { get; }

        public IncrementingBoard(int boardSize, Random rng)
        {
            Rng = rng;
            
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
    }
}
