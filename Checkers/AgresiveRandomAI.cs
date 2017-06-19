using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckersBoard
{
    public class AgresiveRandomAI : IAIAlgorithm
    {
        public Move GetMove(CheckerBoard currentBoard, Player player)
        {
            var avaliableMoves = GetAvaliableMoves(currentBoard, player);
            avaliableMoves.Shuffle();
            if (avaliableMoves.Count < 1)
                return null;
            return avaliableMoves[0];
        }

        private IList<Move> GetAvaliableMoves(CheckerBoard currentBoard, Player player)
        {
            List<Piece> currentPieces = new List<Piece>();
            List<Move> avaliableMoves = new List<Move>();

            // If there exists jump move, return that because by rules nothing else can be played
            var jumpMoves = currentBoard.GetJumpMoves(player);
            if (jumpMoves.Count > 0)
            {
                return jumpMoves;
            }

            // Get available pieces for current player
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    var state = currentBoard.GetState(r, c);
                    if (player == Player.Red && (new[] { FieldState.Red, FieldState.RedKing }).Contains(state))
                        currentPieces.Add(new Piece(r, c));
                    else if (player == Player.Black && (new[] { FieldState.Black, FieldState.BlackKing }).Contains(state))
                        currentPieces.Add(new Piece(r, c));
                }

            // Get all avaiable moves for current playder
            foreach (Piece p in currentPieces)
            {
                var moves = currentBoard.GetSimpleMoves(p);
                avaliableMoves.AddRange(moves);
            }

            return avaliableMoves;
        }
    }
}
