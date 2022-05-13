// See https://aka.ms/new-console-template for more information

using IA_AEstrela;
using IA_AEstrela.Models;
using IA_AEstrela.Printing;
using IA_AEstrela.Utils;

Console.WriteLine("Execution Started");

const string initialVertex = "A";
const string destinationVertex = "R";


var graphEdges = GraphReader.ReadEdges("files/Grafo_Prova01.txt").AsList() ?? new List<GraphEdge>();
var graphHeuristics = GraphReader.ReadHeuristics("files/Heuristica_Prova01.txt").AsList() ?? new List<GraphHeuristic>();

// ReSharper disable once UseObjectOrCollectionInitializer
var graph = new Graph(graphEdges);

// Setting the G(x) and H(x) functions to be used in the search
graph.Gx = (edge, graphInternal) => edge.Cost;
graph.Hx = (edge, graphInternal) => graphHeuristics.Find(x => x.VertexFrom.Equals(edge.VertexTo) &&
                                                              x.VertexTo.Equals(destinationVertex))?.Cost 
                                    ?? int.MaxValue;


var resultRoute = graph.SearchForRoute(initialVertex, destinationVertex);

if(resultRoute is not null)
{
    PrintHandler.Init()
                .BreakLine()
                .ChangeForegroundColor(ConsoleColor.Magenta)
                .PrintPatternLine("-+-")
                .PrintLine($"The successful route is: {string.Join(" => ", resultRoute.TraveledVertexes)}")
                .PrintLine($"The total travel cost was: {resultRoute.AccumulatedCost}")
                .PrintPatternLine("-+-")
                .RevertForegroundColor()
                .BreakLine()
                .BreakLine();
}

// TODO: Print result
Console.WriteLine("Execution Ended");
