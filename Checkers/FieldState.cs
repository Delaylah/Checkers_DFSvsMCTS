using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckersBoard
{
    public enum FieldState
    {
        Invalid = -1,
        Empty = 0,
        Red = 1,
        Black = 2,
        RedKing = 3,
        BlackKing = 4

        //1 is red
        // 2 is black
        // 3 is red king
        // 4 is black king
    }
}
