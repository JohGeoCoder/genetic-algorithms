using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;
using TaskRunner.GeneticStructures;
using TaskRunner.Populations.IncrementingBoardPopulation;

namespace TaskRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //var runner = new GameRunner(200, 30);
            //runner.Start();

            var runner = new Runner<long[]>(new IncrementingBoardPopulation(200, 200, new Random()), 5000);
            runner.Start();
        }
    }

    public class Board
    {
        public long[] Cells { get; set; }

        public Board(int side, Random rng)
        {
            Cells = new long[side * side];
            for(int i = 0; i < side * side; i++)
            {
                Cells[i] = rng.Next(0, side * side);
            }
        }

        public long Score()
        {
            long score = 0;
            for(int i = 0; i < Cells.Length; i++)
            {
                score += (Cells[i] - i) * (Cells[i] - i);
            }

            return score;
        }
    }

    public class GameRunner
    {
        public Board[] Boards { get; set; }
        public Random RNG;

        public GameRunner(int boardCount, int boardSize)
        {
            RNG = new Random();

            Boards = new Board[boardCount];
            for(int i = 0; i < boardCount; i++)
            {
                Boards[i] = new Board(boardSize, RNG);
            }
        }

        public void Start()
        {
            int iteration = 0;

            Console.WriteLine($"Iteration 0: {ScoreGame()}");

            while(iteration < 30000)
            {
                iteration++;

                //Score the boards.
                var scoredBoards = Boards.Select(b => new
                {
                    Score = b.Score(),
                    Board = b
                });

                //Sort the boards by score
                var sortedBoards = scoredBoards.OrderBy(b => b.Score).Select(b => b.Board).ToArray();

                //Keep the best boards
                var bestBoards = new HashSet<Board>(sortedBoards[0..(Boards.Length / 10)]);

                //Generate new boards with the best boards
                for(int i = 0; i < Boards.Length; i++)
                {
                    //Skip if this is a best board
                    if (bestBoards.Contains(Boards[i])) continue;

                    //Replace this board with an offspring of two random best boards
                    var boardIndex1 = RNG.Next(0, (Boards.Length / 10) - 1);
                    var boardIndex2 = RNG.Next(boardIndex1 + 1, Boards.Length / 10);
                    Mate(sortedBoards[boardIndex1], sortedBoards[boardIndex2], Boards[i]);
                }

                Console.WriteLine($"Iteration {iteration}: {ScoreGame()}");
            }

            var bestBoard = Boards.Select(b => new
            {
                Score = b.Score(),
                Board = b
            })
            .OrderBy(b => b.Score)
            .First();

            Console.WriteLine();
            Console.WriteLine(bestBoard.Score);
            Console.WriteLine(string.Join(",", bestBoard.Board.Cells));

            return;
        }

        public long ScoreGame()
        {
            long score = 0;

            for (int i = 0; i < Boards.Length; i++){
                score += Boards[i].Score();
            }

            return score;
        }

        public void Mate(Board parent1, Board parent2, Board child)
        {
            var cutoff = RNG.Next(1, parent1.Cells.Length - 1);

            for (int i = 0; i < cutoff; i++)
            {
                child.Cells[i] = parent1.Cells[i];
            }

            for (int i = cutoff; i < parent2.Cells.Length; i++)
            {
                child.Cells[i] = parent2.Cells[i];
            }

            //Mutation
            if (RNG.Next(0, 100) < 2)
            {
                child.Cells[RNG.Next(0, child.Cells.Length)] = RNG.Next(0, child.Cells.Length);
            }
        }
    }
}
