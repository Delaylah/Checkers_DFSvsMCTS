using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckersBoard
{
    public interface IAIAlgorithm
    {
        Move GetMove(CheckerBoard currentBoard, Player player);
    }
}
