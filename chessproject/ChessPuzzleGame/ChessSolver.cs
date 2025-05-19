using System;
using System.Collections.Generic;
using System.Drawing;

namespace ChessPuzzleGame
{
    /// <summary>
    /// AI solver for the chess puzzle game that finds the optimal solution path
    /// using breadth-first search algorithm.
    /// </summary>
    public class ChessSolver
    {
        // Constants for the board dimensions
        private const int ROWS = 2;
        private const int COLS = 3;

        // Class to represent a board state with its move history
        private class BoardState
        {
            public PieceType[,] Board { get; }
            public List<Move> Moves { get; }

            public BoardState(PieceType[,] board, List<Move> moves)
            {
                // Deep copy the board
                Board = new PieceType[ROWS, COLS];
                for (int row = 0; row < ROWS; row++)
                {
                    for (int col = 0; col < COLS; col++)
                    {
                        Board[row, col] = board[row, col];
                    }
                }

                // Copy the moves list
                Moves = new List<Move>(moves);
            }

            // Get a unique string representation of the board for comparison
            public string GetBoardHash()
            {
                string hash = "";
                for (int row = 0; row < ROWS; row++)
                {
                    for (int col = 0; col < COLS; col++)
                    {
                        hash += (int)Board[row, col];
                    }
                }
                return hash;
            }
        }

        /// <summary>
        /// Represents a move from one position to another
        /// </summary>
        public class Move
        {
            public Point Source { get; }
            public Point Target { get; }
            public PieceType PieceType { get; }

            public Move(Point source, Point target, PieceType pieceType)
            {
                Source = source;
                Target = target;
                PieceType = pieceType;
            }

            public override string ToString()
            {
                return $"{PieceType} from ({Source.X},{Source.Y}) to ({Target.X},{Target.Y})";
            }
        }

        /// <summary>
        /// Solves the chess puzzle using breadth-first search
        /// </summary>
        /// <param name="startBoard">The initial board configuration</param>
        /// <param name="targetBoard">The target board configuration</param>
        /// <returns>A list of moves that solve the puzzle, or null if no solution exists</returns>
        public List<Move> SolvePuzzle(PieceType[,] startBoard, PieceType[,] targetBoard)
        {
            // Create a queue for BFS
            Queue<BoardState> queue = new Queue<BoardState>();

            // Create a set to track visited states
            HashSet<string> visited = new HashSet<string>();

            // Create the initial state
            BoardState initialState = new BoardState(startBoard, new List<Move>());
            queue.Enqueue(initialState);
            visited.Add(initialState.GetBoardHash());

            while (queue.Count > 0)
            {
                BoardState currentState = queue.Dequeue();

                // Check if we've reached the target state
                if (IsBoardMatch(currentState.Board, targetBoard))
                {
                    return currentState.Moves;
                }

                // Generate all possible next states
                foreach (var nextState in GetNextStates(currentState))
                {
                    string boardHash = nextState.GetBoardHash();
                    if (!visited.Contains(boardHash))
                    {
                        visited.Add(boardHash);
                        queue.Enqueue(nextState);
                    }
                }
            }

            // No solution found
            return null;
        }

        /// <summary>
        /// Checks if two boards match
        /// </summary>
        private bool IsBoardMatch(PieceType[,] board1, PieceType[,] board2)
        {
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    if (board1[row, col] != board2[row, col])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Generates all possible next states from the current state
        /// </summary>
        private List<BoardState> GetNextStates(BoardState currentState)
        {
            List<BoardState> nextStates = new List<BoardState>();

            // Find all pieces on the board
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    if (currentState.Board[row, col] != PieceType.Empty)
                    {
                        // Try moving this piece to all possible positions
                        Point source = new Point(col, row);
                        PieceType pieceType = currentState.Board[row, col];

                        // Generate all possible moves based on piece type
                        List<Point> possibleMoves = GetPossibleMoves(currentState.Board, source);

                        foreach (Point target in possibleMoves)
                        {
                            // Create a new board state with this move
                            BoardState newState = ApplyMove(currentState, source, target, pieceType);
                            nextStates.Add(newState);
                        }
                    }
                }
            }

            return nextStates;
        }

        /// <summary>
        /// Gets all possible moves for a piece at the given position
        /// </summary>
        private List<Point> GetPossibleMoves(PieceType[,] board, Point source)
        {
            List<Point> moves = new List<Point>();
            PieceType pieceType = board[source.Y, source.X];

            switch (pieceType)
            {
                case PieceType.King:
                    // King moves one square in any direction
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue; // Skip the current position

                            int newCol = source.X + dx;
                            int newRow = source.Y + dy;

                            if (IsValidPosition(newRow, newCol) &&
                                board[newRow, newCol] == PieceType.Empty)
                            {
                                moves.Add(new Point(newCol, newRow));
                            }
                        }
                    }
                    break;

                case PieceType.Bishop:
                    // Bishop moves diagonally
                    // Check all four diagonal directions
                    int[] dxBishop = { -1, -1, 1, 1 };
                    int[] dyBishop = { -1, 1, -1, 1 };

                    for (int dir = 0; dir < 4; dir++)
                    {
                        for (int step = 1; step < Math.Max(ROWS, COLS); step++)
                        {
                            int newCol = source.X + dxBishop[dir] * step;
                            int newRow = source.Y + dyBishop[dir] * step;

                            if (!IsValidPosition(newRow, newCol)) break;

                            if (board[newRow, newCol] == PieceType.Empty)
                            {
                                moves.Add(new Point(newCol, newRow));
                            }
                            else
                            {
                                break; // Can't move through pieces
                            }
                        }
                    }
                    break;

                case PieceType.Rook:
                    // Rook moves horizontally or vertically
                    // Check all four directions
                    int[] dxRook = { -1, 0, 1, 0 };
                    int[] dyRook = { 0, -1, 0, 1 };

                    for (int dir = 0; dir < 4; dir++)
                    {
                        for (int step = 1; step < Math.Max(ROWS, COLS); step++)
                        {
                            int newCol = source.X + dxRook[dir] * step;
                            int newRow = source.Y + dyRook[dir] * step;

                            if (!IsValidPosition(newRow, newCol)) break;

                            if (board[newRow, newCol] == PieceType.Empty)
                            {
                                moves.Add(new Point(newCol, newRow));
                            }
                            else
                            {
                                break; // Can't move through pieces
                            }
                        }
                    }
                    break;
            }

            return moves;
        }

        /// <summary>
        /// Checks if a position is valid on the board
        /// </summary>
        private bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < ROWS && col >= 0 && col < COLS;
        }

        /// <summary>
        /// Applies a move to create a new board state
        /// </summary>
        private BoardState ApplyMove(BoardState currentState, Point source, Point target, PieceType pieceType)
        {
            // Create a new board state
            BoardState newState = new BoardState(currentState.Board, currentState.Moves);

            // Add this move to the history
            newState.Moves.Add(new Move(source, target, pieceType));

            // Move the piece
            newState.Board[target.Y, target.X] = newState.Board[source.Y, source.X];
            newState.Board[source.Y, source.X] = PieceType.Empty;

            return newState;
        }
    }
}