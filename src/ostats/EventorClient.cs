using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ostats
{
    public class EventorClient
    {
        public string APIKEY = "";
        public const string BASE_URL = "https://eventor.orientering.se/api/";

        public EventorClient(string apikey)
        {
            APIKEY = apikey;
        }

        private string GetData(string query)
        {
            //PrepareCache
            var fileName =
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                    "Cache",
                    query.Replace("?", "_").Replace("&", "_") + ".txt");

            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            if (File.Exists(fileName) && File.GetCreationTime(fileName) > DateTime.Now.AddDays(-31))
                return File.ReadAllText(fileName);


            // Download data from the internet
            var client = new System.Net.WebClient();
            client.Headers.Add("ApiKey", APIKEY);
            var bytes = client.DownloadData(BASE_URL + query);
            var responseString = System.Text.Encoding.UTF8.GetString(bytes);

            File.WriteAllText(fileName, responseString);

            return responseString;

        }

        public List<Event> GetEvents()
        {
            var xml = GetData("events?fromDate=2016-01-01&toDate=2016-04-30");

            var s = new XmlSerializer(typeof(Eventor.EventList));
            var def = (Eventor.EventList)s.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

            var events = def.Event.Select(x => new Event
            {
                Id = x.EventId.Text.FirstOrDefault() ?? "",
                Name = x.Name.Text.FirstOrDefault() ?? "",
                Type = x.eventForm,
                Date = DateTime.Parse(x.FinishDate.Date.Text.First())

            })
            .OrderBy(x => x.Date)
            .ToList();

            return events;
        }
        public class Event
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public Eventor.EventEventForm Type { get; set; }
            public DateTime Date { get; set; }
        }


        public Results GetResults(string eventId)
        {
            var xml = GetData($"results/event?eventId={eventId}&includeSplitTimes=true");

            var s = new XmlSerializer(typeof(Eventor.ResultList));
            var def = (Eventor.ResultList)s.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

            var test = new Results
            {
                Classes = def.ClassResult.EmptyIfNull().Select(c => new Class
                {
                    Name = (c.Item as Eventor.EventClass).Name.Text.FirstOrDefault() ?? "",
                    Items = c.Items.OfType<Eventor.PersonResult>().Select(x => new Result
                    {
                        Position = (x.Item2 as Eventor.Result)?.ResultPosition?.Text.First().ToInt32(),
                        Clubb = (x.Item1 as Eventor.Organisation)?.Name.Text?.First() ?? "",
                        Name = (x.Item as Eventor.Person)?.PersonName.Given.First().Text?.First() + " " + (x.Item as Eventor.Person)?.PersonName.Family.Text?.First(),
                        Time = (x.Item2 as Eventor.Result)?.Time?.Text.EmptyIfNull().FirstOrDefault()?.ToTimeSpan(),
                        Diff = (x.Item2 as Eventor.Result)?.TimeDiff?.Text.EmptyIfNull().FirstOrDefault()?.ToTimeSpan()
                    }).ToList()
                }).ToList()
            };


            return test;
        }

        public class Results
        {
            public List<Class> Classes { get; set; }
        }
        public class Class
        {
            public string Name { get; set; }
            public List<Result> Items { get; set; }
        }
        public class Result
        {
            public int? Position { get; set; }
            public string Name { get; set; }
            public string Clubb { get; set; }
            public TimeSpan? Time { get; set; }
            public TimeSpan? Diff { get; set; }

        }

    }

}
