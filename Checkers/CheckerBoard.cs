using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckersBoard
{
    //Object to hold the state of the checkers board
    // -1 is a invalid space
    // 0 is empty
    // 1 is red
    // 2 is black
    // 3 is red king
    // 4 is black king
    public class CheckerBoard : ICloneable
    {
        private FieldState[,] board = new FieldState[8, 8];

        public Player NextPlayer { get; private set; }

        private Piece mustPlayPiece { get; set; }

        public CheckerBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    board[r, c] = FieldState.Empty;
                }
            }
        }

        public CheckerBoard(FieldState[,] initialBoard)
        {
            board = initialBoard;
        }

        public void Parse(string str, Player player)
        {
            this.NextPlayer = player;
            var lines = str.Split('\n');

            for (var r = 0; r < 8; r++)
            {
                for (var c = 0; c < 8; c++)
                {
                    var chr = lines[r][c];

                    if (chr == 'E')
                        board[r, c] = FieldState.Empty;
                    else if (chr == 'R')
                        board[r, c] = FieldState.Red;
                    else if (chr == 'S')
                        board[r, c] = FieldState.RedKing;
                    else if (chr == 'B')
                        board[r, c] = FieldState.Black;
                    else if (chr == 'C')
                        board[r, c] = FieldState.BlackKing;
                }
            }
        }


        public bool SetState(int r, int c, FieldState state)
        {
            board[r, c] = state;
            return true;
        }

        public bool SetState(Piece piece, FieldState state)
        {
            return SetState(piece.Row, piece.Column, state);
        }

        public FieldState GetState(int r, int c)
        {
            if ((r > 7) || (r < 0) || (c > 7) || (c < 0))
                return FieldState.Invalid;

            return board[r, c];
        }

        public FieldState GetState(Piece piece)
        {
            return GetState(piece.Row, piece.Column);
        }

        public void InitializeBoard()
        {
            this.NextPlayer = Player.Black;

            // Fill red
            for (var row = 0; row < 3; row++)
                for (var col = 0; col < 8; col++)
                {
                    // If row is even, column is odd
                    if (row % 2 != col % 2)
                        SetState(row, col, FieldState.Red);
                }

            // Fill red
            for (var row = 5; row < 8; row++)
                for (var col = 0; col < 8; col++)
                {
                    // If row is even, column is odd
                    if (row % 2 != col % 2)
                        SetState(row, col, FieldState.Black);
                }
        }

        public void MakeMove(Move move, Player player)
        {
            if (NextPlayer != player)
                throw new Exception($"Wrong player. Next player is {NextPlayer}.");

            // If must play piece exists, checks if it is playing
            if (mustPlayPiece != null && mustPlayPiece != move.piece1)
                throw new Exception($"Piece({mustPlayPiece.Row}, {mustPlayPiece.Column}) must play next.");

            // Check if playing piece is of player color
            var p1State = GetState(move.piece1);
            if (!IsOfColor(player, p1State))
                throw new Exception($"Piece 1 is not of same color as player ({player}!={move.piece1}).");

            // Check if destination is empty
            var p2State = GetState(move.piece2);
            if (p2State != FieldState.Empty)
                throw new Exception($"Destinatino field must be empty (Current value is {p2State}).");

            // If there are jump moves, jump must be played
            var jumpMoves = GetJumpMoves(NextPlayer);
            if (jumpMoves != null && jumpMoves.Count() > 0 && !IsJumpMove(move))
                throw new Exception($"Jump move must be played. There are {jumpMoves.Count()} jumps for {NextPlayer} player.");

            // Check if it is moving in right direction
            // If jump, check if it is eating opponent

            /* Make a move */

            // Check if needs to become king
            if (player == Player.Red)
            {
                if (move.piece2.Row == 7)
                    SetState(move.piece2, FieldState.RedKing);
                else
                    SetState(move.piece2, p1State);
            }
            else if (player == Player.Black)
            {
                if (move.piece2.Row == 0)
                    SetState(move.piece2, FieldState.BlackKing);
                else
                    SetState(move.piece2, p1State);
            }

            // Clear initial position
            SetState(move.piece1, FieldState.Empty);

            // It move is "eat", remove enemy
            if (move.IsEat())
            {
                var eatPosition = getEatPosition(move);
                SetState(eatPosition, FieldState.Empty);

                // Checks if piece has more to eate
                var moreToEat = GetJumpMoves(move.piece2);
                if (moreToEat != null && moreToEat.Count() > 0)
                    mustPlayPiece = move.piece2;
                else
                    mustPlayPiece = null;
            }

            // Update next player if game is not finished and no more connected jumps
            if (GetGameStatus() == GameStatuses.Running && mustPlayPiece == null)
                NextPlayer = (player == Player.Black ? Player.Red : Player.Black);
        }

        private Piece getEatPosition(Move move)
        {
            var row = (move.piece2.Row - move.piece1.Row) / 2 + move.piece1.Row;
            var col = (move.piece2.Column - move.piece1.Column) / 2 + move.piece1.Column;
            return new Piece(row, col);
        }

        public GameStatuses GetGameStatus()
        {
            var redCount = 0;
            var blackCount = 0;

            for (var row = 0; row < 8; row++)
                for (var col = 0; col < 8; col++)
                {
                    var fieldState = GetState(row, col);
                    if (fieldState == FieldState.Red || fieldState == FieldState.RedKing)
                        redCount++;
                    else if (fieldState == FieldState.Black || fieldState == FieldState.BlackKing)
                        blackCount++;
                }

            if (redCount == 0)
                return GameStatuses.BlackWon;
            else if (blackCount == 0)
                return GameStatuses.RedWon;
            else if (GetSimpleMoves(Player.Red).Count() == 0 && GetJumpMoves(Player.Red).Count() == 0)
                    return GameStatuses.BlackWon;
            else if (GetSimpleMoves(Player.Black).Count() == 0 && GetJumpMoves(Player.Black).Count() == 0)
                return GameStatuses.RedWon;
            else
                return GameStatuses.Running;
        }

        public IList<Move> GetJumpMoves(Player color)
        {
            if (mustPlayPiece != null && IsOfColor(color, GetState(mustPlayPiece)))
            {
                return GetJumpMoves(mustPlayPiece);
            }
            else
            {
                List<Move> jumps = new List<Move>();

                for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    var state = GetState(r, c);

                    if (IsPiece(state) && IsOfColor(color, state))
                    {
                        var tmpJumps = GetJumpMoves(new Piece(r, c));
                        jumps.AddRange(tmpJumps);
                    }
                }

                return jumps;
            }
        }

        public IList<Move> GetJumpMoves(Piece piece)
        {
            List<Move> jumps = new List<Move>();

            var state = GetState(piece);
            
            var rows = new List<int>();

            if (state == FieldState.Red || state == FieldState.RedKing || state == FieldState.BlackKing)
                rows.Add(piece.Row + 2);
            if (state == FieldState.Black || state == FieldState.RedKing || state == FieldState.BlackKing)
                rows.Add(piece.Row - 2);

            foreach (var row in rows)
            {
                for (var col = piece.Column - 2; col <= piece.Column + 2; col = col + 4)
                {
                    // Check if position is on board
                    if (row >= 0 && row < 8 && col >= 0 && col < 8)
                    {
                        var destState = GetState(row, col);
                        if (destState == FieldState.Empty)
                        {
                            var midRow = (row - piece.Row) / 2 + piece.Row;
                            var midCol = (col - piece.Column) / 2 + piece.Column;
                            var midState = GetState(midRow, midCol);

                            if (IsPiece(midState) && GetPieceColor(state) != GetPieceColor(midState))
                            {
                                jumps.Add(new Move(piece, new Piece(row, col)));
                            }
                        }
                    }
                }
            }

            return jumps;
        }
        
        public IList<Move> GetSimpleMoves(Player player)
        {
            var moves = new List<Move>();

            for (var r=0; r<8; r++)
            for (var c = 0; c < 8; c++)
            {
                var state = GetState(r, c);
                if (IsOfColor(player, state))
                {
                    var m = GetSimpleMoves(new Piece(r, c));
                    moves.AddRange(m);
                }
            }

            return moves;
        }

        public List<Move> GetSimpleMoves(Piece piece)
        {
            List<Move> moves = new List<Move>();

            var state = GetState(piece);

            // King can move in all directions, it is same for Red and Black
            if (state == FieldState.RedKing || state == FieldState.BlackKing)
            {
                for (var row = piece.Row - 1; row <= piece.Row + 1; row = row + 2)
                    for (var col = piece.Column - 1; col <= piece.Column + 1; col = col + 2)
                    {
                        if (row >= 0 && row < 8 && col >= 0 && col < 8)
                        {
                            var destState = GetState(row, col);
                            if (destState == FieldState.Empty)
                                moves.Add(new Move(piece, new Piece(row, col)));
                        }
                    }
            }

            // Red can go only down
            else if (state == FieldState.Red || state == FieldState.Black)
            {
                var row = -1;
                if (state == FieldState.Red)
                    row = piece.Row + 1;
                else if (state == FieldState.Black)
                    row = piece.Row - 1;

                for (var col = piece.Column - 1; col <= piece.Column + 1; col = col + 2)
                {
                    if (row >= 0 && row < 8 && col >= 0 && col < 8)
                    {
                        var destState = GetState(row, col);
                        if (destState == FieldState.Empty)
                            moves.Add(new Move(piece, new Piece(row, col)));
                    }
                }
            }

            return moves;
        }

        public object Clone()
        {
            var field = (FieldState[,])board.Clone();
            var newBoard = new CheckerBoard(field) { NextPlayer = this.NextPlayer};

            /*for (var r = 0; r<8; r++)
            for (var c = 0; c < 8; c++)
            {
                var state = GetState(r, c);
                newBoard.SetState(r, c, state);
            }*/

            return newBoard;
        }

        #region Static helpers

        public static bool IsJumpMove(Piece p1, Piece p2)
        {
            return (Math.Abs(p2.Row - p1.Row) > 1 && Math.Abs(p2.Column - p1.Column) > 1);
        }

        public static bool IsJumpMove(Move move)
        {
            return IsJumpMove(move.piece1, move.piece2);
        }

        public static bool IsOfColor(Player player, FieldState state)
        {
            return (player == Player.Black && (state == FieldState.Black || state == FieldState.BlackKing))
                   || (player == Player.Red && (state == FieldState.Red || state == FieldState.RedKing));
        }

        public static bool IsPiece(FieldState state)
        {
            return (state == FieldState.Red || state == FieldState.RedKing || state == FieldState.Black || state == FieldState.BlackKing);
        }

        public static Player GetPieceColor(FieldState state)
        {
            if (state == FieldState.Red || state == FieldState.RedKing)
                return Player.Red;
            else if (state == FieldState.Black || state == FieldState.BlackKing)
                return Player.Black;
            else
                throw new Exception($"State {state} is not piece.");
        }

        #endregion

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();

            for (var r = 0; r < 8; r++)
            {
                for (var c = 0; c < 8; c++)
                {
                    var state = GetState(r, c);
                    switch (state)
                    {
                        case FieldState.Empty:
                            strBuilder.Append("E");
                            break;

                        case FieldState.Red:
                            strBuilder.Append("R");
                            break;

                        case FieldState.RedKing:
                            strBuilder.Append("S");
                            break;

                        case FieldState.Black:
                            strBuilder.Append("B");
                            break;

                        case FieldState.BlackKing:
                            strBuilder.Append("C");
                            break;
                    }
                }

                strBuilder.AppendLine();
            }

            return strBuilder.ToString();
        }
    }
}
