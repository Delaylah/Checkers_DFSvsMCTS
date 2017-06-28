using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckersBoard
{
    public class Node<T>
    {
        public Move Move { get; private set; }

        public CheckerBoard Board { get; private set; }
        
        public string Name { get; set; }

        public bool IsEvaluated { get; set; }

        public Player ParentPlayer { get; set; }


        public List<T> Children = new List<T>();

        // Konstruktor
        /*public Node(Move move, CheckerBoard board, Player player):this(move,board)
        {
            this.Player = player;
        }*/

        public Node(Move move, CheckerBoard board)
        {
            Move = move;
            Board = board;
        }

        public Node(CheckerBoard board)
        {
            Board = board;
        }


        public void AddChild(T child)
        {
            Children.Add(child);
        }

        public virtual string RenderName()
        {
            return this.Name;
        }
    }
}
