using IA_AEstrela.Models;

namespace IA_AEstrela
{
    public class Graph
    {
        #region Graph
        public readonly Dictionary<string, List<GraphEdge>> OutputEdgesByVertex;

        public readonly Dictionary<string, List<GraphHeuristic>> HeuristicsByVertex;

        public Graph(IEnumerable<GraphEdge> edges, IEnumerable<GraphHeuristic>? heuristics = null)
        {
            heuristics ??= new List<GraphHeuristic>();

            OutputEdgesByVertex = edges
                .ToLookup(x => x.VertexFrom)
                .ToDictionary(x => x.Key, x => x.ToList());
            HeuristicsByVertex = heuristics
                .ToLookup(x => x.VertexFrom)
                .ToDictionary(x => x.Key, x => x.ToList());
        }
        #endregion

        #region Calculation Functions
        public Func<GraphEdge, Graph, double> Gx = (edge, graph) => 0;

        public Func<GraphEdge, Graph, double> Hx = (edge, graph) => 0;

        public double G(GraphEdge edge) => Gx(edge, this);

        public double H(GraphEdge edge) => Hx(edge, this);
        #endregion

        #region Processing
        public GraphRoute Execute_AStar(string sourceVertex, string targetVertex)
        {
            var routes = new List<GraphRoute>();
            routes.Add(new GraphRoute()
            {
                CurrentVertex = sourceVertex,
                PossibleEdges = OutputEdgesByVertex.TryGetValue(sourceVertex, out var edges) ? edges : new()
            });

            while(true)
            {
                var availableEdges = routes.SelectMany(route => route.PossibleEdges
                    .Select(x => new
                    {
                        route = route,
                        edge = x,
                        fx = route.AccumulatedCost + G(x) + H(x)
                    }));

                // TODO: Print Available Paths

                var smallestCostPath = availableEdges.MinBy(x => x.fx);

                if(smallestCostPath is null)
                {
                    // TODO: Properly treat error when the vertex is unreachable.
                    break;
                }

                // TODO: Print Chosen Path

                smallestCostPath.route.PossibleEdges.Remove(smallestCostPath.edge);

                var nextVertex = smallestCostPath.edge.VertexTo;

                var newRoute = new GraphRoute
                {
                    CurrentVertex = nextVertex,
                    AccumulatedCost = smallestCostPath.route.AccumulatedCost + G(smallestCostPath.edge)
                };

                newRoute.PossibleEdges = OutputEdgesByVertex.TryGetValue(sourceVertex, out var pEdges)
                    ? pEdges.ExceptBy(smallestCostPath.route.TraveledVertexs, x => x.VertexFrom, StringComparer.InvariantCultureIgnoreCase)
                        .ToList()
                    : new();

                newRoute.TraveledVertexs.AddRange(smallestCostPath.route.TraveledVertexs);
                newRoute.TraveledVertexs.Add(nextVertex);

                routes.Add(newRoute);

                if(newRoute.CurrentVertex == targetVertex)
                {
                    Console.WriteLine($"Test: {newRoute.CurrentVertex}");
                    return newRoute;
                }

            }
        }
        #endregion
    }
}
