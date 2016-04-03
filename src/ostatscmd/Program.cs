using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ostats;

namespace ostatscmd
{
    class Program
    {
        static void Main(string[] args)
        {
            var apikey = File.ReadAllText("apikey");
            var client = new EventorClient(apikey);

            //PrintAllResultsForClubb(client, "Loftan");
            CreateToplists(client);
        }

        private static void CreateToplists(EventorClient client)
        {
            var events = client.GetEvents();


            var res = (from e in events
                       from c in client.GetResults(e.Id).Classes
                       from r in c.Items
                       where r.Clubb.Contains("Löftan")
                       select new
                       {
                           Event = e.Name,
                           e.Date,
                           Class = c.Name,
                           r.Name,
                           r.Position,
                           r.Time,
                           r.Diff
                       })
                      .ToList();

            var löftanevents = res.Select(x => new { x.Date, x.Event }).Distinct().ToList();

            var nbrOfStarts = res.Count;
            var nbrOfFinished = res.Where(x => x.Time.HasValue).Count();

            var avgPlace = res.Where(x => x.Position.HasValue).Average(x => x.Position);
            var totalMinutes = res.Where(x => x.Position.HasValue).Sum(x => x.Time.Value.TotalMinutes);


            var persons = res.GroupBy(x => x.Name).Select(x => new
            {
                Name = x.Key,
                Tävlingar = x.Count(),
                TotalTime = x.Sum(y => y.Time.HasValue ? (int)y.Time.Value.TotalMinutes : 0),
                AvgTime = x.Sum(y => y.Time.HasValue ? (int)y.Time.Value.TotalMinutes : 0) / x.Count(),
                Events = x.ToList()
            })
            .OrderByDescending(x => x.TotalTime)
            .ToList();

            Console.WriteLine("Top 10 Nbr Events");
            foreach (var item in persons.OrderByDescending(x => x.AvgTime).Take(10))
            {
                Console.WriteLine($"{item.AvgTime} {item.Name}");
            }
        }

        private static void PrintAllResultsForClubb(EventorClient client, string clubb)
        {
            var events = client.GetEvents();

            foreach (var eventet in events)
            {

                var results = client.GetResults(eventet.Id);

                var loftan = results.Classes
                    .SelectMany(x => x.Items.Select(y => new { Class = x, Result = y })).Where(x => x.Result.Clubb.Contains("Löftan"))
                    .ToList();

                if (loftan.Any())
                    Console.WriteLine($"{eventet.Date.ToShortDateString()} {eventet.Name} results {loftan.Count} {eventet.Type}");

                foreach (var item in loftan)
                {
                    Console.WriteLine($"{item.Class.Name} {item.Result.Position} {item.Result.Name} {item.Result.Time} {item.Result.Diff}");
                }
            }
        }
    }
}
