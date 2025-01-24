using System;
using System.Collections.Generic;
using System.Linq;
using A0;
using D3.Types;

namespace long_term
{
    public class OutageSimpleType
    {
        public string Name;
        public string Date;
        public double Capacity;
        public double Outage;
        public double AvailableCapacity;
    }
    public class OutageRecord
    {
        public string Plant;
        public DateTime Date;
        public double Amount;

        public static OutageRecord[] Parse(OutageType[] outages)
        {
            var theSeries = outages.Select(o => new { Outage = o, o.EndDate.Subtract(o.StartDate).Days });
            var theDiff = theSeries.Select(x => x.Days.Range().Select(o => new OutageRecord { Plant = x.Outage.Plant, Date = x.Outage.StartDate.AddDays(o), Amount = x.Outage.Lost }));
            return theDiff.SelectMany(o => o).ToArray();
        }
    }

    public class SimpleSeries
    {
        
        public static List<OutageSimpleType> Basic3()
        {
            var plantA = new PlantType() { Capacity = 100, Name = "unitA" };
            var plantB = new PlantType() { Capacity = 60, Name = "unitB" };

            var outage1 = new OutageType() { Plant = "unitA", StartDate = "2025-02-01".ToDateTime(), EndDate = "2025-03-01".ToDateTime(), Lost = 30 };
            var outage2 = new OutageType() { Plant = "unitB", StartDate = "2025-02-01".ToDateTime(), EndDate = "2025-03-01".ToDateTime(), Lost = 20 };

            var outages = new[] { outage1, outage2 };
            var plants = new[] { plantA, plantB };

            var outageRecords = OutageRecord.Parse(outages).Dump("outages");

            var firstDate = new DateTime(2025, 1, 1);

            var dayTable = plants
                .Select(o => Enumerable.Range(0, 365).Select(z => (double)z).ToArray()
                    .Select((y, i) => new {  Date = firstDate.AddDays(i), Name = o.Name, Val = (double)y }).ToArray())
                .SelectMany(x => x).Dump();

            var withAdditional = from a in dayTable
                join b in outageRecords on new { a.Name, a.Date } equals new { Name = b.Plant, b.Date } into g1
                from z in g1.DefaultIfEmpty()
                select new { a.Name, a.Date, Capacity = a.Val, Outage = z?.Amount ?? 0, AvailableCapacity = a.Val - z?.Amount ?? 0 };


            //File.WriteAllText(@"C:\disk.win\hub\html\outage\json\data4.js", "var data = " + JsonCamelCase(withAdditional));

            var simpleTypes = from a in withAdditional
                select new OutageSimpleType
                {
                    Name = a.Name, Date = a.Date.ToStringNeat(), Capacity = a.Capacity, Outage = a.Outage,
                    AvailableCapacity = a.AvailableCapacity
                };
            return simpleTypes.ToList();
        }
    }
}
