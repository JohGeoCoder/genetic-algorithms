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
        private RandomGenerator Rng { get; set; } = RandomGenerator.GetInstance();


        public Warehouse(int aisleCount, int aisleDepth, IEnumerable<Product> products, IEnumerable<PickTicket> pickTickets)
        {
            Shelves = new int?[aisleCount * aisleDepth];

            //Randomly place the items on the shelves.
            var shelfIndices = Enumerable.Range(0, Shelves.Length).ToList();
            foreach(var product in products)
            {
                var tempIndex = Rng.Next(0, shelfIndices.Count);
                var randomShelfIndex = shelfIndices[tempIndex];

                Shelves[randomShelfIndex] = product.Id;

                shelfIndices.RemoveAt(tempIndex);
            }



            if(Shelves.All(s => s.HasValue))
            {
                var i = 1;
            }

            AisleDepth = aisleDepth;
            Products = products.ToDictionary(k => k.Id, v => v);
            PickTickets = pickTickets.ToArray();
        }

        public void Clone(IOrganism parent)
        {
            var p = (Warehouse)parent;

            //Copy the shelves
            for (int i = 0; i < Shelves.Length; i++)
            {
                Shelves[i] = p.Shelves[i];
            }
        }

        public void Mate(IOrganism parent1, IOrganism parent2)
        {
            var p1 = (Warehouse)parent1;
            var p2 = (Warehouse)parent2;

            for (int n = 0; n < Shelves.Length; n++)
            {
                Shelves[n] = null;
            }

            var shelfLength = p1.Shelves.Length;

            var cutoff = Rng.Next(1, shelfLength - 1);

            //Build the initial child and keep track of unused products.
            var usedIds = new HashSet<int>();
            var duplicateUsedIds = new HashSet<int>();
            var unusedIds = new HashSet<int>();
            var duplicateUnusedIds = new Queue<int>();
            for (var i = 0; i < p1.Shelves.Length; i++)
            {
                //Determine the used an unused product IDs
                int? currentUsedId;
                int? currentUnusedId;
                if(i < cutoff)
                {
                    currentUsedId = p1.Shelves[i];
                    currentUnusedId = p2.Shelves[i];
                }
                else
                {
                    currentUsedId = p2.Shelves[i];
                    currentUnusedId = p1.Shelves[i];
                }

                //If the current used shelf is empty and the unused shelf has a value, use the value.
                if (!currentUsedId.HasValue && currentUnusedId.HasValue)
                {
                    var tempUsedId = currentUsedId;
                    currentUsedId = currentUnusedId;
                    currentUnusedId = tempUsedId;
                }

                //Assigned the used Product ID to the child's shelf.
                Shelves[i] = currentUsedId;

                //Keep track of duplicate used IDs.
                if (currentUsedId.HasValue)
                {
                    var currentUsedIdValue = currentUsedId.Value;
                    
                    if (usedIds.Contains(currentUsedIdValue))
                    {
                        duplicateUsedIds.Add(currentUsedIdValue);
                    }
                    else
                    {
                        usedIds.Add(currentUsedIdValue);
                    }
                }

                //Keep track of duplicate unused IDs.
                if (currentUnusedId.HasValue)
                {
                    var currentUnusedIdValue = currentUnusedId.Value;

                    if (unusedIds.Contains(currentUnusedIdValue))
                    {
                        duplicateUnusedIds.Enqueue(currentUnusedIdValue);
                    }
                    else
                    {
                        unusedIds.Add(currentUnusedIdValue);
                    }
                }
            }

            //if(duplicateUnusedIds.Count != duplicateUsedIds.Count)
            //{
            //    Console.WriteLine($"Parent1: {p1}");
            //    Console.WriteLine($"Parent2: {p2}");
            //    Console.WriteLine($"Child  : {this}");
            //}

            //Console.WriteLine($"Parent1    : {p1}");
            //Console.WriteLine($"Parent2    : {p2}");
            var childPreReconcileString = $"Child         : {this}";
            var duplicateUnusedIdsList = duplicateUnusedIds.ToList();


            if (Shelves.All(s => s.HasValue))
            {
                //Console.WriteLine(this);
                var x = 1;
            }

            //If there are no missing or duplicate IDs to reconcile, we are finished here.
            if (duplicateUnusedIds.Count == 0 && duplicateUsedIds.Count == 0) return;

            //Iterate through the child and reconcile duplicate or missing product IDs
            var duplicateIdsEncountered = new HashSet<int>();
            var duplicateIdsReplaced = new HashSet<int>();
            for (var i = 0; i < Shelves.Length; i++)
            {
                var currentProductId = Shelves[i];

                if (currentProductId.HasValue)
                {
                    var currentProductIdValue = currentProductId.Value;

                    //Consider replacing encountered duplicate used value.
                    if (duplicateUsedIds.Contains(currentProductIdValue))
                    {
                        //If a previous instance of this duplicate used ID has already been replaced, skip this ID
                        if (duplicateIdsReplaced.Contains(currentProductIdValue))
                        {
                            continue;
                        }

                        //This duplicate used ID has already been encountered and skipped. Replace this one.
                        if (duplicateIdsEncountered.Contains(currentProductIdValue))
                        {
                            if(duplicateUnusedIds.TryDequeue(out int unusedId)) {
                                Shelves[i] = unusedId;
                            }
                            else
                            {
                                Shelves[i] = null;
                            }

                            duplicateIdsReplaced.Add(currentProductIdValue);
                        }
                        else //This is the first time encountering this duplicate used ID. 50% chance to replace it now.
                        {
                            var isReplace = Rng.NextDouble() < 0.5;

                            if (isReplace)
                            {
                                if(duplicateUnusedIds.TryDequeue(out int unusedId))
                                {
                                    Shelves[i] = unusedId;
                                }
                                else
                                {
                                    Shelves[i] = null;
                                }

                                duplicateIdsReplaced.Add(currentProductIdValue);
                            }
                            else
                            {
                                duplicateIdsEncountered.Add(currentProductIdValue);
                            }
                        }
                    }
                }
                //else
                //{
                //    var p1Val = p1.Shelves[i];
                //    var p2Val = p2.Shelves[i];

                //    if((p1Val.HasValue ^ p2Val.HasValue))
                //    {
                //        var potentialUnusedVariable = p1Val;
                //        if (!potentialUnusedVariable.HasValue) potentialUnusedVariable = p2Val;

                //        if (duplicateUnusedIds.Contains(potentialUnusedVariable.Value))
                //        {
                //            var isReplace = Rng.NextDouble() < 0.5;

                //            if (isReplace)
                //            {
                //                if(duplicateUnusedIds.)
                //            }

                //            Shelves[i] = potentialUnusedVariable;
                //        }
                //    }
                //}
            }

            //Console.WriteLine($"Reconciled : {this}");

            if (p1.Shelves.All(s => s.HasValue))
            {
                var i = 1;
            }

            if (p2.Shelves.All(s => s.HasValue))
            {
                var i = 1;
            }

            if (Shelves.All(s => s.HasValue))
            {
                var i = 1;
            }

            if (Shelves.Distinct().ToArray().Length < Shelves.Length)
            {
                Console.WriteLine($"Dupe Used IDs  : {string.Join("  ", duplicateUsedIds)}");
                Console.WriteLine($"Dupe Unused IDs: {string.Join("  ", duplicateUnusedIdsList)}");
                Console.WriteLine($"Parent1        : {p1}");
                Console.WriteLine($"Parent2        : {p2}");
                Console.WriteLine(childPreReconcileString);
                Console.WriteLine($"Reconciled     : {this}");

                var n = 1;
            }
        }

        public void Mutate(decimal probability)
        {
            if (Shelves.All(s => s.HasValue))
            {
                var i = 1;
            }

            //Randomly select two shelves
            var shelf1Pos = Rng.Next(Shelves.Length);
            var shelfPos2 = Rng.Next(Shelves.Length);

            //Swap their contents
            var tempShelf = Shelves[shelf1Pos];
            Shelves[shelf1Pos] = Shelves[shelfPos2];
            Shelves[shelfPos2] = tempShelf;

            if (Shelves.All(s => s.HasValue))
            {
                var i = 1;
            }
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
