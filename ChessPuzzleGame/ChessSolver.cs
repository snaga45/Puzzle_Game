using System;
using System.Collections.Generic;
using System.Drawing;

namespace ChessPuzzleGame
{
    /// <summary>
    /// AI solver for the chess puzzle game that implements multiple search algorithms
    /// </summary>
    public class ChessSolver
    {
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
        public List<Move> SolvePuzzle(PieceType[,] startBoard, PieceType[,] targetBoard)
        {
            // Create a queue for BFS
            Queue<Tuple<PieceType[,], List<Move>>> queue = new Queue<Tuple<PieceType[,], List<Move>>>();
            queue.Enqueue(Tuple.Create(CloneBoard(startBoard), new List<Move>()));

            // Create a set to track visited states
            HashSet<string> visited = new HashSet<string>();
            visited.Add(BoardToString(startBoard));

            while (queue.Count > 0)
            {
                // Get the next state from the queue
                Tuple<PieceType[,], List<Move>> current = queue.Dequeue();
                PieceType[,] currentBoard = current.Item1;
                List<Move> currentMoves = current.Item2;

                // Check if we've reached the target state
                if (IsBoardMatch(currentBoard, targetBoard))
                {
                    return currentMoves;
                }

                // Generate all possible next states
                foreach (Move move in GetAllPossibleMoves(currentBoard))
                {
                    // Create a new board with this move applied
                    PieceType[,] newBoard = CloneBoard(currentBoard);
                    ApplyMoveToBoard(newBoard, move);

                    // Check if we've already visited this state
                    string boardHash = BoardToString(newBoard);
                    if (!visited.Contains(boardHash))
                    {
                        // Mark as visited
                        visited.Add(boardHash);

                        // Create new move list with this move added
                        List<Move> newMoves = new List<Move>(currentMoves);
                        newMoves.Add(move);

                        // Add to queue
                        queue.Enqueue(Tuple.Create(newBoard, newMoves));
                    }
                }
            }

            // No solution found
            return null;
        }

        /// <summary>
        /// Solves the puzzle using simple Trial-Error method
        /// </summary>
        public List<Move> SolveWithTrialError(PieceType[,] startBoard, PieceType[,] targetBoard, int maxAttempts)
        {
            Random random = new Random();

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Create a new board and move list for this attempt
                PieceType[,] currentBoard = CloneBoard(startBoard);
                List<Move> moves = new List<Move>();

                // Try up to 20 random moves per attempt
                for (int i = 0; i < 20; i++)
                {
                    // Get all possible moves from current state
                    List<Move> possibleMoves = GetAllPossibleMoves(currentBoard);
                    if (possibleMoves.Count == 0) break;

                    // Pick a random move
                    Move move = possibleMoves[random.Next(possibleMoves.Count)];
                    moves.Add(move);

                    // Apply the move
                    ApplyMoveToBoard(currentBoard, move);

                    // Check if we've reached the target
                    if (IsBoardMatch(currentBoard, targetBoard))
                        return moves;
                }
            }

            return null; // No solution found
        }

        /// <summary>
        /// Solves the puzzle using Trial-Error with restart and depth-bound
        /// </summary>
        public List<Move> SolveWithTrialErrorDepthBound(PieceType[,] startBoard, PieceType[,] targetBoard,
                                               int maxAttempts, int depthBound)
        {
            Random random = new Random();

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Create a new board and move list for this attempt
                PieceType[,] currentBoard = CloneBoard(startBoard);
                List<Move> moves = new List<Move>();

                // Try moves up to the depth bound
                for (int depth = 0; depth < depthBound; depth++)
                {
                    // Get all possible moves from current state
                    List<Move> possibleMoves = GetAllPossibleMoves(currentBoard);
                    if (possibleMoves.Count == 0) break;

                    // Pick a random move
                    Move move = possibleMoves[random.Next(possibleMoves.Count)];
                    moves.Add(move);

                    // Apply the move
                    ApplyMoveToBoard(currentBoard, move);

                    // Check if we've reached the target
                    if (IsBoardMatch(currentBoard, targetBoard))
                        return moves;
                }
            }

            return null; // No solution found
        }

        /// <summary>
        /// Solves the puzzle using Backtracking algorithm
        /// </summary>
        public List<Move> SolveWithBacktracking(PieceType[,] startBoard, PieceType[,] targetBoard, int maxDepth)
        {
            List<Move> moves = new List<Move>();
            HashSet<string> visited = new HashSet<string>();

            bool success = BacktrackHelper(startBoard, targetBoard, moves, visited, 0, maxDepth);

            return success ? moves : null;
        }

        private bool BacktrackHelper(PieceType[,] currentBoard, PieceType[,] targetBoard,
                            List<Move> moves, HashSet<string> visited,
                            int depth, int maxDepth)
        {
            // Check if we've reached the target
            if (IsBoardMatch(currentBoard, targetBoard))
                return true;

            // Check if we've exceeded the depth limit
            if (depth >= maxDepth)
                return false;

            // Mark current state as visited
            string boardHash = BoardToString(currentBoard);
            visited.Add(boardHash);

            // Get all possible moves from current state
            List<Move> possibleMoves = GetAllPossibleMoves(currentBoard);

            // Try each move
            foreach (Move move in possibleMoves)
            {
                // Create a new board with this move applied
                PieceType[,] newBoard = CloneBoard(currentBoard);
                ApplyMoveToBoard(newBoard, move);

                // Skip if we've already visited this state
                string newBoardHash = BoardToString(newBoard);
                if (visited.Contains(newBoardHash))
                    continue;

                // Add this move to our solution
                moves.Add(move);

                // Recursively try this path
                if (BacktrackHelper(newBoard, targetBoard, moves, visited, depth + 1, maxDepth))
                    return true;

                // Backtrack - remove the move
                moves.RemoveAt(moves.Count - 1);
            }

            return false;
        }

        /// <summary>
        /// Solves the puzzle using Depth-First Search algorithm
        /// </summary>
        public List<Move> SolveWithDFS(PieceType[,] startBoard, PieceType[,] targetBoard, int maxDepth)
        {
            // Stack for DFS (board state, moves so far, current depth)
            Stack<Tuple<PieceType[,], List<Move>, int>> stack = new Stack<Tuple<PieceType[,], List<Move>, int>>();

            // Set to track visited states
            HashSet<string> visited = new HashSet<string>();

            // Start with initial state
            stack.Push(Tuple.Create(CloneBoard(startBoard), new List<Move>(), 0));
            visited.Add(BoardToString(startBoard));

            while (stack.Count > 0)
            {
                // Get current state
                var current = stack.Pop();
                PieceType[,] currentBoard = current.Item1;
                List<Move> currentMoves = current.Item2;
                int currentDepth = current.Item3;

                // Check if we've reached the target
                if (IsBoardMatch(currentBoard, targetBoard))
                    return currentMoves;

                // Skip if we've reached max depth
                if (currentDepth >= maxDepth)
                    continue;

                // Get all possible moves from current state
                List<Move> possibleMoves = GetAllPossibleMoves(currentBoard);

                // Try each move (in reverse order for DFS)
                for (int i = possibleMoves.Count - 1; i >= 0; i--)
                {
                    Move move = possibleMoves[i];

                    // Create a new board with this move applied
                    PieceType[,] newBoard = CloneBoard(currentBoard);
                    ApplyMoveToBoard(newBoard, move);

                    // Skip if we've already visited this state
                    string newBoardHash = BoardToString(newBoard);
                    if (visited.Contains(newBoardHash))
                        continue;

                    // Mark as visited
                    visited.Add(newBoardHash);

                    // Create new move list with this move added
                    List<Move> newMoves = new List<Move>(currentMoves);
                    newMoves.Add(move);

                    // Push to stack
                    stack.Push(Tuple.Create(newBoard, newMoves, currentDepth + 1));
                }
            }

            return null; // No solution found
        }

        /// <summary>
        /// Solves the puzzle using A* algorithm
        /// </summary>
        public List<Move> SolveWithAStar(PieceType[,] startBoard, PieceType[,] targetBoard)
        {
            // Priority queue for A* (simulated with a list that we'll sort)
            PriorityQueue<Tuple<PieceType[,], List<Move>>, int> openSet = new PriorityQueue<Tuple<PieceType[,], List<Move>>, int>();

            // Set to track visited states
            HashSet<string> closedSet = new HashSet<string>();

            // Start with initial state
            openSet.Enqueue(Tuple.Create(CloneBoard(startBoard), new List<Move>()), CalculateHeuristic(startBoard, targetBoard));

            while (openSet.Count > 0)
            {
                // Get the node with lowest f(n)
                var current = openSet.Dequeue();
                PieceType[,] currentBoard = current.Item1;
                List<Move> currentMoves = current.Item2;

                // Check if we've reached the target
                if (IsBoardMatch(currentBoard, targetBoard))
                    return currentMoves;

                // Add to closed set
                string currentBoardHash = BoardToString(currentBoard);
                closedSet.Add(currentBoardHash);

                // Get all possible moves from current state
                List<Move> possibleMoves = GetAllPossibleMoves(currentBoard);

                // Try each move
                foreach (Move move in possibleMoves)
                {
                    // Create a new board with this move applied
                    PieceType[,] newBoard = CloneBoard(currentBoard);
                    ApplyMoveToBoard(newBoard, move);

                    // Skip if we've already processed this state
                    string newBoardHash = BoardToString(newBoard);
                    if (closedSet.Contains(newBoardHash))
                        continue;

                    // Create new move list with this move added
                    List<Move> newMoves = new List<Move>(currentMoves);
                    newMoves.Add(move);

                    // Calculate f(n) = g(n) + h(n)
                    // g(n) = cost so far (number of moves)
                    // h(n) = estimated cost to goal (heuristic)
                    int priority = newMoves.Count + CalculateHeuristic(newBoard, targetBoard);

                    // Add to open set
                    openSet.Enqueue(Tuple.Create(newBoard, newMoves), priority);
                }
            }

            return null; // No solution found
        }

        /// <summary>
        /// Calculates a heuristic value for A* algorithm (Manhattan distance)
        /// </summary>
        private int CalculateHeuristic(PieceType[,] currentBoard, PieceType[,] targetBoard)
        {
            int score = 0;
            int rows = currentBoard.GetLength(0);
            int cols = currentBoard.GetLength(1);

            // For each piece in the current board
            for (int row1 = 0; row1 < rows; row1++)
            {
                for (int col1 = 0; col1 < cols; col1++)
                {
                    if (currentBoard[row1, col1] != PieceType.Empty)
                    {
                        // Find where this piece should be in the target board
                        for (int row2 = 0; row2 < rows; row2++)
                        {
                            for (int col2 = 0; col2 < cols; col2++)
                            {
                                if (targetBoard[row2, col2] == currentBoard[row1, col1])
                                {
                                    // Add Manhattan distance
                                    score += Math.Abs(row1 - row2) + Math.Abs(col1 - col2);
                                }
                            }
                        }
                    }
                }
            }

            return score;
        }

        /// <summary>
        /// Gets all possible moves from the current board state
        /// </summary>
        private List<Move> GetAllPossibleMoves(PieceType[,] board)
        {
            List<Move> moves = new List<Move>();
            int rows = board.GetLength(0);
            int cols = board.GetLength(1);

            // For each piece on the board
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (board[row, col] != PieceType.Empty)
                    {
                        Point source = new Point(col, row);
                        PieceType pieceType = board[row, col];

                        // Get all possible moves for this piece
                        List<Point> targets = GetPossibleMoves(board, source);

                        // Add each move to the list
                        foreach (Point target in targets)
                        {
                            moves.Add(new Move(source, target, pieceType));
                        }
                    }
                }
            }

            return moves;
        }

        /// <summary>
        /// Gets all possible moves for a piece at the given position
        /// </summary>
        private List<Point> GetPossibleMoves(PieceType[,] board, Point source)
        {
            List<Point> moves = new List<Point>();
            PieceType pieceType = board[source.Y, source.X];
            int rows = board.GetLength(0);
            int cols = board.GetLength(1);

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

                            if (IsValidPosition(newRow, newCol, rows, cols) &&
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
                        for (int step = 1; step < Math.Max(rows, cols); step++)
                        {
                            int newCol = source.X + dxBishop[dir] * step;
                            int newRow = source.Y + dyBishop[dir] * step;

                            if (!IsValidPosition(newRow, newCol, rows, cols)) break;

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
                        for (int step = 1; step < Math.Max(rows, cols); step++)
                        {
                            int newCol = source.X + dxRook[dir] * step;
                            int newRow = source.Y + dyRook[dir] * step;

                            if (!IsValidPosition(newRow, newCol, rows, cols)) break;

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
        private bool IsValidPosition(int row, int col, int rows, int cols)
        {
            return row >= 0 && row < rows && col >= 0 && col < cols;
        }

        /// <summary>
        /// Applies a move to a board
        /// </summary>
        private void ApplyMoveToBoard(PieceType[,] board, Move move)
        {
            // Move the piece
            board[move.Target.Y, move.Target.X] = board[move.Source.Y, move.Source.X];
            board[move.Source.Y, move.Source.X] = PieceType.Empty;
        }

        /// <summary>
        /// Checks if two boards match
        /// </summary>
        private bool IsBoardMatch(PieceType[,] board1, PieceType[,] board2)
        {
            int rows = board1.GetLength(0);
            int cols = board1.GetLength(1);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
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
        /// Converts a board to a string for hashing
        /// </summary>
        private string BoardToString(PieceType[,] board)
        {
            string hash = "";
            int rows = board.GetLength(0);
            int cols = board.GetLength(1);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    hash += (int)board[row, col];
                }
            }

            return hash;
        }

        /// <summary>
        /// Creates a deep copy of a board
        /// </summary>
        private PieceType[,] CloneBoard(PieceType[,] board)
        {
            int rows = board.GetLength(0);
            int cols = board.GetLength(1);

            PieceType[,] newBoard = new PieceType[rows, cols];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    newBoard[row, col] = board[row, col];
                }
            }

            return newBoard;
        }
    }
}
