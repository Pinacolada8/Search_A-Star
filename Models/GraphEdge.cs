using CsvHelper.Configuration.Attributes;

namespace IA_AEstrela.Models
{
    public class GraphEdge
    {
        public string VertexFrom { get; set; } = string.Empty;

        public string VertexTo { get; set; } = string.Empty;

        public int Cost { get; set; }

        [Ignore]
        public bool Bidirectional { get; set; } = true;
    }
}
