using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_AEstrela.Models
{
    public class GraphPath
    {
        public string CurrentCity { get; set; } = string.Empty;
        public int AccumulatedCost { get; set; } = 0;
        public List<string> PassedCities  { get; set; } = new(); 
        public List<GraphConnection> AvailableConnections { get; set; } = new();
    }
}
