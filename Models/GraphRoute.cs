namespace IA_AEstrela.Models
{
    public class GraphRoute
    {
        public string CurrentVertex { get; set; } = string.Empty;
        public double AccumulatedCost { get; set; } = 0;
        public List<string> TraveledVertexs { get; set; } = new();
        public List<GraphEdge> PossibleEdges { get; set; } = new();
    }
}
