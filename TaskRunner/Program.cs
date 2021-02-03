using System;
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

            var products = Enumerable.Range(0, 10000).Select(x => new Product
            {
                Id = x,
                ProductName = new string(Enumerable.Range(0, 9).Select(n => (char)('a' + rng.Next(0, 26))).ToArray())
            }).ToArray();

            var pickTickets = Enumerable.Range(0, 100).Select(x =>
            {
                var pickTicketSize = rng.Next(1, 20);
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

            var initialPopulation = Enumerable.Range(0, 199)
                .Select(x => new Warehouse(aisleCount: 200, aisleDepth: 150, products: products, pickTickets: pickTickets))
                .ToArray();

            //var initialPopulation = new IncrementingBoard[200];
            //for(int i = 0; i < initialPopulation.Length; i++)
            //{
            //    initialPopulation[i] = new IncrementingBoard(200);
            //}

            var runner = new Runner(initialPopulation, 100);
            runner.Start();
        }
    }
}
