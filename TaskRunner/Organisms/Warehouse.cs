using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using TaskRunner.GeneticStructures;

namespace TaskRunner.Organisms
{
    public class Warehouse : IOrganism
    {
        public Dictionary<int, Product> Products { get; set; }
        public PickTicket[] PickTickets { get; set; }
        public int?[] Shelves { get; set; }
        private int AisleDepth { get; set; }
        public Random Rng { get; set; } = new Random();


        public Warehouse(int aisleCount, int aisleDepth, IEnumerable<Product> products, IEnumerable<PickTicket> pickTickets)
        {
            Shelves = new int?[aisleCount * aisleDepth];

            //Randomly place the items on the shelves.
            var shelfIndices = Enumerable.Range(0, Shelves.Length - 1).ToList();
            foreach(var product in products)
            {
                var tempIndex = Rng.Next(0, shelfIndices.Count);
                var randomShelfIndex = shelfIndices[tempIndex];

                Shelves[randomShelfIndex] = product.Id;

                shelfIndices.RemoveAt(tempIndex);
            }

            AisleDepth = aisleDepth;
            Products = products.ToDictionary(k => k.Id, v => v);
            PickTickets = pickTickets.ToArray();
        }

        public void Clone(IOrganism parent)
        {
            var p = (Warehouse)parent;

            //Copy the shelves
            for(int i = 0; i < Shelves.Length; i++)
            {
                Shelves[i] = p.Shelves[i];
            }
        }

        public void Mate(IOrganism parent1, IOrganism parent2)
        {
            var p1 = (Warehouse)parent1;
            var p2 = (Warehouse)parent2;

            var shelfLength = p1.Shelves.Length;

            var cutoff = Rng.Next(1, shelfLength - 1);

            for(var i = 0; i < cutoff; i++)
            {
                Shelves[i] = p1.Shelves[i];
            }

            for(var i = cutoff; i < shelfLength; i++)
            {
                Shelves[i] = p2.Shelves[i];
            }
        }

        public void Mutate(decimal probability)
        {
            //Randomly select two shelves
            var shelf1Pos = Rng.Next(Shelves.Length);
            var shelfPos2 = Rng.Next(Shelves.Length);

            //Swap their contents
            var tempShelf = Shelves[shelf1Pos];
            Shelves[shelf1Pos] = Shelves[shelfPos2];
            Shelves[shelfPos2] = tempShelf;
        }

        public long Score()
        {
            long score = 0;

            var productCoordinatesLookup = new Dictionary<int, int[]>();
            foreach(var product in Products.Values)
            {
                productCoordinatesLookup.Add(product.Id, GetProductCoordinates(product.Id));
            }

            //Score each pick ticket.
            foreach(var pickTicket in PickTickets)
            {
                //Calculate the spread of the pick ticket items
                for(int i = 0; i < pickTicket.ProductIds.Length - 1; i++)
                {
                    var pick1Coords = productCoordinatesLookup[pickTicket.ProductIds[i]];

                    for(int j = i; j < pickTicket.ProductIds.Length; j++)
                    {
                        var pick2Coords = productCoordinatesLookup[pickTicket.ProductIds[j]];

                        var distance = (long)Math.Sqrt((pick2Coords[0] - pick1Coords[0]) * (pick2Coords[0] - pick1Coords[0]) + (pick2Coords[1] - pick1Coords[1]) * (pick2Coords[1] - pick1Coords[1]));

                        score += distance;
                    }
                }

                //Calculate the distance of the center of mass of the items from the origin for each pick ticket
                var productCoordinates = pickTicket.ProductIds.Select(p => productCoordinatesLookup[p]);
                var xAvg = productCoordinates.Average(c => c[0]);
                var yAvg = productCoordinates.Average(c => c[1]);

                score += (int)(yAvg + xAvg);
            }

            return score;
        }

        private int[] GetProductCoordinates(int productId)
        {
            var productShelfIndex = -1;
            for(int i = 0; i < Shelves.Length; i++)
            {
                if(Shelves[i] == productId)
                {
                    productShelfIndex = i;
                    break;
                }
            }

            return new int[] { productShelfIndex / AisleDepth, productShelfIndex % AisleDepth };
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            for (int i = 0; i < Shelves.Length; i++)
            {
                if (i % AisleDepth == 0)
                {
                    sb.AppendLine();
                }
                sb.Append(Shelves[i].ToString().PadLeft(4));
            }

            return sb.ToString();
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
    }

    public class PickTicket
    {
        public int[] ProductIds { get; set; }
    }
}
