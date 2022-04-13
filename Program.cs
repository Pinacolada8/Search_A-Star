// See https://aka.ms/new-console-template for more information

using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using IA_AEstrela.Models;

Console.WriteLine("Execution Started");

var graphConnections = new List<GraphConnection>();
var lineHeuristics = new List<LineHeuristics>();


var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    HasHeaderRecord = false,
    Delimiter = ";"

};
using(var reader = new StreamReader("files/Grafo.txt"))
using(var csv = new CsvReader(reader, config))
{
    graphConnections = csv.GetRecords<GraphConnection>().ToList();
}

using(var reader = new StreamReader("files/Heuristica.txt"))
using(var csv = new CsvReader(reader, config))
{
    lineHeuristics = csv.GetRecords<LineHeuristics>().ToList();
}


#if false
Console.WriteLine("Graph Paths => ");
graphPaths.ForEach(x =>
{
    Console.WriteLine($"Origin: {x.OriginCity} - Target: {x.TargetCity} - Cost: {x.Cost}");
});

Console.WriteLine("Line Heuristics => ");
lineHeuristics.ForEach(x =>
{
    Console.WriteLine($"Target: {x.TargetCity} - Cost: {x.Cost}");
});
#endif

var dictOriginCity = graphConnections.ToLookup(x => x.OriginCity);

const string destinationCity = "Bucareste";
var initialCity = "Arad";
var success = false;

var paths = new List<GraphPath>();
paths.Add(new GraphPath()
{
    CurrentCity = initialCity,
    AccumulatedCost = 0,
    AvailableConnections = dictOriginCity.Contains(initialCity)
        ? dictOriginCity[initialCity].ToList()
        : new List<GraphConnection>()
});

//while(!currentCity.Equals(destinationCity))
while(true)
{
    var availablePaths = paths.SelectMany(x => x.AvailableConnections
        .Select(conn => new
        {
            path = x,
            nextConn = conn,
            heuristics = lineHeuristics.Find(h => h.TargetCity == conn.TargetCity),
        }))
        .Select(x => new
        {
            path = x.path,
            x.nextConn,
            nextCity = x.nextConn.TargetCity,
            Cost = x.path.AccumulatedCost + x.nextConn.Cost + (x.heuristics?.Cost ?? int.MaxValue)
        });

    // TODO: Print Available Paths

    var smallestCostWay = availablePaths.MinBy(x => x.Cost);

    if (smallestCostWay is null)
    {
        success = false;
        break;
    }

    // TODO: Print Chosen Path

    smallestCostWay.path.AvailableConnections.Remove(smallestCostWay.nextConn);

    var newPathCity = smallestCostWay.nextCity;

    var newPath = new GraphPath
    {
        CurrentCity = newPathCity,
        AccumulatedCost = smallestCostWay.Cost
    };

    newPath.AvailableConnections = dictOriginCity.Contains(newPathCity)
        ? dictOriginCity[newPathCity]
            .ExceptBy(smallestCostWay.path.PassedCities,
                x => x.OriginCity,
                StringComparer.InvariantCultureIgnoreCase)
            .ToList()
        : new List<GraphConnection>();

    newPath.PassedCities.AddRange(smallestCostWay.path.PassedCities);
    newPath.PassedCities.Add(newPathCity);

    paths.Add(newPath);

    if (newPath.CurrentCity == destinationCity)
    {
        Console.WriteLine($"Test: {newPath.CurrentCity}");
        success = true;
        break;
    }
        
}


// TODO: Print result
Console.WriteLine("Execution Ended");
