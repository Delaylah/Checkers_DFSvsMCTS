using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckersBoard
{
    public class NodeJsonModel
    {
        public string Name { get; set; }

        public IList<NodeJsonModel> Children { get; set; }
    }
}
