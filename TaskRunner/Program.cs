﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;
using TaskRunner.GeneticStructures;
using TaskRunner.Organisms;
using TaskRunner.Populations;

namespace TaskRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var rng = new Random();

            var products = Enumerable.Range(0, 1000).Select(x => new Product
            {
                Id = x + 1,
                ProductName = new string(Enumerable.Range(0, 9).Select(n => (char)('a' + rng.Next(0, 26))).ToArray())
            }).ToArray();

            var pickTickets = Enumerable.Range(0, 100).Select(x =>
            {
                var pickTicketSize = rng.Next(1, 15);
                var productIds = new int[pickTicketSize];

                var pickIndex = 0;
                for(int i = 0; i < products.Length; i++)
                {
                    //Probability this product will be next on the pick ticket.
                    var selectThreshold = (double)pickTicketSize / (products.Length - i);

                    if(rng.NextDouble() < selectThreshold)
                    {
                        var productId = products[i].Id;

                        productIds[pickIndex] = productId;

                        pickIndex++;
                    }

                    //If we've picked all the products for the pick ticket, exit the loop
                    if(pickIndex >= pickTicketSize)
                    {
                        break;
                    }
                }

                var pickTicket = new PickTicket
                {
                    ProductIds = productIds
                };

                return pickTicket;
            })
            .ToArray();

            var aisleCount = 30;
            var aisleDepth = 100;
            var shelfCount = aisleCount * aisleDepth;
            var distanceLookup = new long[shelfCount][];
            for(int i = 0; i < distanceLookup.Length; i++)
            {
                distanceLookup[i] = new long[shelfCount];
            }

            for (int p1 = 0; p1 < shelfCount; p1++)
            {
                for (int p2 = 0; p2 < shelfCount; p2++)
                {
                    var p1X = p1 / aisleDepth;
                    var p1Y = p1 % aisleDepth;
                    var p2X = p2 / aisleDepth;
                    var p2Y = p2 % aisleDepth;

                    var distance = (long)Math.Sqrt((p2X - p1X) * (p2X - p1X) + (p2Y - p1Y) * (p2Y - p1Y));

                    distanceLookup[p1][p2] = distance;
                }
            }

            var initialPopulation = Enumerable.Range(0, 100)
                .Select(x => new Warehouse(aisleCount: 30, aisleDepth: 100, products: products, pickTickets: pickTickets, distanceLookup: distanceLookup))
                .ToArray();

            var emptyPopulation = Enumerable.Range(0, 100)
                .Select(x => new Warehouse(aisleCount: 30, aisleDepth: 100, products: products, pickTickets: pickTickets, distanceLookup: distanceLookup))
                .ToArray();

            var runner = new Runner(initialPopulation: initialPopulation, emptyPopulation: emptyPopulation, iterations: 1000, mutationRate: 0.05m, matePopulationCutoff: 30, keepTopCutoff: 10);
            runner.Start();
        }
    }
}
