using CsvHelper;
using CsvHelper.Configuration;
using IA_AEstrela.Models;
using System.Globalization;

namespace IA_AEstrela
{
    public static class GraphReader
    {
        private static readonly CsvConfiguration _CsvConfiguration
            = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ";"
            };

        public static IEnumerable<GraphEdge> ReadEdges(string filePath)
        {
            using(var reader = new StreamReader(filePath))
            using(var csv = new CsvReader(reader, _CsvConfiguration))
            {
                var models = csv.GetRecords<GraphEdge>().ToList();
                return models;
            }
        }

        public static IEnumerable<GraphHeuristic> ReadHeuristics(string filePath)
        {
            using(var reader = new StreamReader(filePath))
            using(var csv = new CsvReader(reader, _CsvConfiguration))
            {
                var models = csv.GetRecords<GraphHeuristic>().ToList();
                return models;
            }
        }
    }
}
