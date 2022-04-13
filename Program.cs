// See https://aka.ms/new-console-template for more information

using IA_AEstrela;
using IA_AEstrela.Models;
using IA_AEstrela.Utils;

Console.WriteLine("Execution Started");

var graphEdges = GraphReader.ReadEdges("files/Grafo.txt").AsList() ?? new();
var graphHeuristics = GraphReader.ReadHeuristics("files/Heuristica.txt").AsList() ?? new();


var dictOriginCity = graphEdges.ToLookup(x => x.VertexFrom);

const string destinationCity = "Bucareste";
var initialCity = "Arad";

var routes = new List<GraphRoute>();
routes.Add(new GraphRoute()
{
    CurrentVertex = initialCity,
    AccumulatedCost = 0,
    PossibleEdges = dictOriginCity.Contains(initialCity)
        ? dictOriginCity[initialCity].ToList()
        : new List<GraphEdge>()
});

while(true)
{
    var availablePaths = routes.SelectMany(x => x.PossibleEdges
        .Select(conn => new
        {
            path = x,
            nextConn = conn,
            heuristics = graphHeuristics.Find(h => h.VertexTo == conn.VertexTo),
        }))
        .Select(x => new
        {
            path = x.path,
            x.nextConn,
            nextCity = x.nextConn.VertexFrom,
            Cost = x.path.AccumulatedCost + x.nextConn.Cost + (x.heuristics?.Cost ?? int.MaxValue)
        });

    // TODO: Print Available Paths

    var smallestCostWay = availablePaths.MinBy(x => x.Cost);

    if(smallestCostWay is null)
    {
        break;
    }

    // TODO: Print Chosen Path

    smallestCostWay.path.PossibleEdges.Remove(smallestCostWay.nextConn);

    var newPathCity = smallestCostWay.nextCity;

    var newPath = new GraphRoute
    {
        CurrentVertex = newPathCity,
        AccumulatedCost = smallestCostWay.Cost
    };

    newPath.PossibleEdges = dictOriginCity.Contains(newPathCity)
        ? dictOriginCity[newPathCity]
            .ExceptBy(smallestCostWay.path.TraveledVertexs,
                x => x.VertexFrom,
                StringComparer.InvariantCultureIgnoreCase)
            .ToList()
        : new List<GraphEdge>();

    newPath.TraveledVertexs.AddRange(smallestCostWay.path.TraveledVertexs);
    newPath.TraveledVertexs.Add(newPathCity);

    routes.Add(newPath);

    if(newPath.CurrentVertex == destinationCity)
    {
        Console.WriteLine($"Test: {newPath.CurrentVertex}");
        break;
    }

}


// TODO: Print result
Console.WriteLine("Execution Ended");
