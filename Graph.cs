using IA_AEstrela.Models;
using IA_AEstrela.Printing;
using IA_AEstrela.Utils;

namespace IA_AEstrela
{
    public class Graph
    {
        public bool PrintEnabled { get; set; } = true;

        #region Graph
        public readonly Dictionary<string, List<GraphEdge>> OutputEdgesByVertex;

        public Graph(IEnumerable<GraphEdge> edges)
        {
            var edgesList = edges.AsList() ?? new List<GraphEdge>();

            var bidirectionalVertexes = edgesList.Where(x => x.Bidirectional)
                                                 .Select(x => new GraphEdge()
                                                 {
                                                     VertexTo = x.VertexFrom,
                                                     VertexFrom = x.VertexTo,
                                                     Cost = x.Cost,
                                                     Bidirectional = x.Bidirectional
                                                 });

            OutputEdgesByVertex = edgesList.Concat(bidirectionalVertexes)
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
        public GraphRoute? SearchForRoute(string sourceVertex, string targetVertex)
        {
            var routes = new List<GraphRoute>
            {
                new()
                {
                    CurrentVertex = sourceVertex,
                    TraveledVertexes = new List<string> { sourceVertex },
                    PossibleEdges = OutputEdgesByVertex.TryGetValue(sourceVertex, out var edges)
                                        ? edges
                                        : new List<GraphEdge>()
                }
            };

            while(true)
            {
                var availableEdges = routes.SelectMany(
                    route => route.PossibleEdges
                                  // Removes edges which the destination is a vertex that already has bee traveled too
                                  .ExceptBy(route.TraveledVertexes, x => x.VertexTo)
                                  .Select(x => new
                                  {
                                      route = route,
                                      edge = x,
                                      fx = route.AccumulatedCost + G(x) + H(x)
                                  }));

                // TODO: Print Available Paths
                if (PrintEnabled)
                {
                    PrintHandler.Init()
                                .PrintPattern("=")
                                .BreakLine();
                }

                var smallestCostPath = availableEdges.MinBy(x => x.fx);

                // breaks the loop in case its not possible to find a path for the required city
                if(smallestCostPath is null)
                    break;

                // TODO: Print Chosen Path

                smallestCostPath.route.PossibleEdges.Remove(smallestCostPath.edge);

                var nextVertex = smallestCostPath.edge.VertexTo;

                var newRoute = new GraphRoute
                {
                    CurrentVertex = nextVertex,
                    AccumulatedCost = smallestCostPath.route.AccumulatedCost + G(smallestCostPath.edge),
                    PossibleEdges = OutputEdgesByVertex.TryGetValue(nextVertex, out var pEdges)
                                        ? pEdges
                                        : new List<GraphEdge>()
                };

                newRoute.TraveledVertexes.AddRange(smallestCostPath.route.TraveledVertexes);
                newRoute.TraveledVertexes.Add(nextVertex);

                routes.Add(newRoute);

                if(nextVertex == targetVertex)
                    return newRoute;
            }

            return null;
        }
        #endregion
    }
}
