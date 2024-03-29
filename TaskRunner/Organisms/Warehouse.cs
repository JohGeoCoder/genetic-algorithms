﻿using System;
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
        public Dictionary<int, int?> ProductLocation { get; set; } = new Dictionary<int, int?>();
        public long[][] DistanceLookup { get; set; }
        private int AisleDepth { get; set; }
        private RandomGenerator Rng { get; set; } = RandomGenerator.GetInstance();


        public Warehouse(int aisleCount, int aisleDepth, IEnumerable<Product> products, IEnumerable<PickTicket> pickTickets, long[][] distanceLookup)
        {
            Shelves = new int?[aisleCount * aisleDepth];
            Products = products.ToDictionary(k => k.Id, v => v);
            AisleDepth = aisleDepth;
            PickTickets = pickTickets.ToArray();

            ProductLocation = products.ToDictionary(p => p.Id, v => (int?)null);

            //Randomly place the items on the shelves.
            var shelfIndices = Enumerable.Range(0, Shelves.Length).ToList();
            foreach(var product in products)
            {
                var tempIndex = Rng.Next(0, shelfIndices.Count);
                var randomShelfIndex = shelfIndices[tempIndex];

                SetShelf(randomShelfIndex, product.Id);

                shelfIndices.RemoveAt(tempIndex);
            }

            DistanceLookup = distanceLookup;
        }

        private void SetShelf(int shelfPosition, int? newProductId)
        {
            Shelves[shelfPosition] = newProductId;

            if (newProductId.HasValue)
            {
                ProductLocation[newProductId.Value] = shelfPosition;
            }
        }

        public void Clone(IOrganism parent)
        {
            var p = (Warehouse)parent;

            //Copy the shelves
            for (int i = 0; i < Shelves.Length; i++)
            {
                SetShelf(i, p.Shelves[i]);
            }
        }

        public void Mate(IOrganism parent1, IOrganism parent2)
        {
            var p1 = (Warehouse)parent1;
            var p2 = (Warehouse)parent2;

            for (int n = 0; n < Shelves.Length; n++)
            {
                SetShelf(n, null);
            }

            var shelfLength = p1.Shelves.Length;

            var cutoff = Rng.Next(1, shelfLength - 1);

            //Build the initial child and keep track of unused products.
            var usedIdDictionary = new Dictionary<int, int>();

            //Place the dominant genes (products) on the shelves.
            for (var i = 0; i < p1.Shelves.Length; i++)
            {
                //Determine the used product ID
                int? currentUsedId;
                if(i < cutoff)
                {
                    currentUsedId = p1.Shelves[i];
                }
                else
                {
                    currentUsedId = p2.Shelves[i];
                }

                //Place the used Product IDs
                if (currentUsedId.HasValue)
                {
                    /**
                     * If this Product has already been placed on a previous shelf, then
                     * we flip a coin to see if the product stays in its current place 
                     * or instead gets placed here.
                     * 
                     * Otherwise the product gets placed at this shelf.
                     */
                    if (usedIdDictionary.ContainsKey(currentUsedId.Value))
                    {
                        //50% chance to use this shelf for this product instead.
                        if (Rng.NextBoolean())
                        {
                            SetShelf(i, currentUsedId.Value);
                            SetShelf(usedIdDictionary[currentUsedId.Value], null);
                            usedIdDictionary[currentUsedId.Value] = i;
                        }
                    }
                    else
                    {
                        //Place the product at this shelf.
                        SetShelf(i, currentUsedId);
                        usedIdDictionary.Add(currentUsedId.Value, i);
                    }
                }
            }

            //Identify the unused Product IDs
            var unusedIdSet = new HashSet<int>();
            for (var i = 0; i < p1.Shelves.Length; i++)
            {
                //Determine the unused product ID
                int? currentUnusedId;
                if (i < cutoff)
                {
                    currentUnusedId = p2.Shelves[i];
                }
                else
                {
                    currentUnusedId = p1.Shelves[i];
                }

                //Place the unused Product IDs
                if (currentUnusedId.HasValue)
                {
                    //Skip this Product if it has already been placed.
                    if (usedIdDictionary.ContainsKey(currentUnusedId.Value))
                    {
                        continue;
                    }

                    unusedIdSet.Add(currentUnusedId.Value);
                }
            }

            //Place the straggling items in random locations in the warehouse.
            if (unusedIdSet.Any())
            {
                //Find all the empty shelves.
                var emptyShelves = new List<int>();
                for (var i = 0; i < Shelves.Length; i++)
                {
                    if (!Shelves[i].HasValue) emptyShelves.Add(i);
                }

                foreach(var unplacedProduct in unusedIdSet)
                {
                    var shelf = emptyShelves[Rng.Next(emptyShelves.Count)];

                    SetShelf(shelf, unplacedProduct);
                    emptyShelves.Remove(shelf);
                }
            }           
        }

        public void Mutate(decimal probability)
        {
            //Randomly select two shelves
            var shelf1Pos = Rng.Next(Shelves.Length);
            var shelfPos2 = Rng.Next(Shelves.Length);

            //Swap their contents
            var tempShelf = Shelves[shelf1Pos];
            SetShelf(shelf1Pos, Shelves[shelfPos2]);
            SetShelf(shelfPos2, tempShelf);
        }

        public long Score()
        {
            long score = 0;

            //Score each pick ticket.
            foreach(var pickTicket in PickTickets)
            {
                //Calculate the spread of the pick ticket items
                for(int i = 0; i < pickTicket.ProductIds.Length - 1; i++)
                {
                    var pick1Location = GetProductLocation(pickTicket.ProductIds[i]);

                    for(int j = i + 1; j < pickTicket.ProductIds.Length; j++)
                    {
                        var pick2Location = GetProductLocation(pickTicket.ProductIds[j]);
                        
                        score += DistanceLookup[pick1Location][pick2Location];
                    }
                }

                //Calculate the distance of the center of mass of the items from the origin for each pick ticket
                var productCoordinates = pickTicket.ProductIds.Select(p => GetProductCoordinates(p));
                var xAvg = productCoordinates.Average(c => c[0]);
                var yAvg = productCoordinates.Average(c => c[1]);

                score += (int)(yAvg * yAvg + xAvg * xAvg);
            }

            return score;
        }

        private int[] GetProductCoordinates(int productId)
        {
            var productShelfIndex = ProductLocation[productId].Value;

            return new int[] { productShelfIndex / AisleDepth, productShelfIndex % AisleDepth };
        }

        private int GetProductLocation(int productId) => ProductLocation[productId].Value;

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

                if (Shelves[i].HasValue)
                {
                    sb.Append(Shelves[i].ToString().PadLeft(4));
                }
                else
                {
                    sb.Append("X".PadLeft(4));
                }
                
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
