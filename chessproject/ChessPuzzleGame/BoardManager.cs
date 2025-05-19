using System;
using System.Collections.Generic;
using System.Drawing;

namespace ChessPuzzleGame
{
    public class BoardManager
    {
        // Constants for the board
        public const int ROWS = 2;
        public const int COLS = 3;
        public const int CELL_SIZE = 80;

        // Board representation
        private ChessPiece[,] startBoard;
        private ChessPiece[,] currentBoard;
        private ChessPiece[,] targetBoard;

        // Images for pieces
        private Image kingImage;
        private Image bishopImage;
        private Image rookImage;

        // Move history for undo
        private Stack<Tuple<Point, Point>> moveHistory = new Stack<Tuple<Point, Point>>();

        public bool PuzzleSolved { get; private set; } = false;

        public BoardManager()
        {
            // Load images
            LoadImages();

            // Initialize boards
            InitializeBoards();
        }

        private void LoadImages()
        {
            // Create simple representations of chess pieces
            kingImage = CreatePieceImage('♔');
            bishopImage = CreatePieceImage('♗');
            rookImage = CreatePieceImage('♖');
        }

        private Image CreatePieceImage(char symbol)
        {
            // Create a simple image with the chess symbol
            Bitmap bmp = new Bitmap(CELL_SIZE - 10, CELL_SIZE - 10);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using (Font font = new Font("Arial", 36, FontStyle.Bold))
                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(symbol.ToString(), font, Brushes.Black,
                        new RectangleF(0, 0, bmp.Width, bmp.Height), sf);
                }
            }
            return bmp;
        }

        private void InitializeBoards()
        {
            startBoard = new ChessPiece[ROWS, COLS];
            currentBoard = new ChessPiece[ROWS, COLS];
            targetBoard = new ChessPiece[ROWS, COLS];

            // Initialize with empty cells
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    startBoard[row, col] = new ChessPiece(PieceType.Empty, null);
                    currentBoard[row, col] = new ChessPiece(PieceType.Empty, null);
                    targetBoard[row, col] = new ChessPiece(PieceType.Empty, null);
                }
            }

            // Set up start board based on the image
            startBoard[0, 0] = new ChessPiece(PieceType.King, kingImage);
            startBoard[0, 1] = new ChessPiece(PieceType.Bishop, bishopImage);
            startBoard[0, 2] = new ChessPiece(PieceType.Bishop, bishopImage);
            startBoard[1, 0] = new ChessPiece(PieceType.Rook, rookImage);
            startBoard[1, 1] = new ChessPiece(PieceType.Rook, rookImage);

            // Set up target board based on the image
            targetBoard[0, 0] = new ChessPiece(PieceType.Empty, null);
            targetBoard[0, 1] = new ChessPiece(PieceType.Bishop, bishopImage);
            targetBoard[0, 2] = new ChessPiece(PieceType.Bishop, bishopImage);
            targetBoard[1, 0] = new ChessPiece(PieceType.Rook, rookImage);
            targetBoard[1, 1] = new ChessPiece(PieceType.Rook, rookImage);
            targetBoard[1, 2] = new ChessPiece(PieceType.King, kingImage);

            // Copy start board to current board
            CopyBoard(startBoard, currentBoard);
        }

        private void CopyBoard(ChessPiece[,] source, ChessPiece[,] destination)
        {
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    destination[row, col] = source[row, col].Clone();
                }
            }
        }

        public void DrawBoard(Graphics g, bool isCurrentBoard, int startX, int startY, Point? selectedCell = null)
        {
            ChessPiece[,] board = isCurrentBoard ? currentBoard : targetBoard;

            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    // Calculate cell position
                    int x = startX + col * CELL_SIZE;
                    int y = startY + row * CELL_SIZE;

                    // Draw cell background
                    bool isLightSquare = (row + col) % 2 == 0;
                    using (Brush brush = new SolidBrush(isLightSquare ? Color.White : Color.LightGray))
                    {
                        g.FillRectangle(brush, x, y, CELL_SIZE, CELL_SIZE);
                    }

                    // Draw cell border
                    g.DrawRectangle(Pens.Black, x, y, CELL_SIZE, CELL_SIZE);

                    // Draw piece if present
                    ChessPiece piece = board[row, col];
                    if (piece.Type != PieceType.Empty && piece.Image != null)
                    {
                        g.DrawImage(piece.Image, x + 5, y + 5, CELL_SIZE - 10, CELL_SIZE - 10);
                    }

                    // Highlight selected cell
                    if (isCurrentBoard && selectedCell.HasValue &&
                        selectedCell.Value.X == col && selectedCell.Value.Y == row)
                    {
                        using (Pen pen = new Pen(Color.Blue, 3))
                        {
                            g.DrawRectangle(pen, x + 1, y + 1, CELL_SIZE - 2, CELL_SIZE - 2);
                        }
                    }
                }
            }
        }

        public bool TryMove(Point source, Point target)
        {
            // Check if the move is valid
            if (IsValidMove(source, target))
            {
                // Make the move
                MakeMove(source, target);

                // Check if puzzle is solved
                CheckIfPuzzleSolved();

                return true;
            }

            return false;
        }

        private bool IsValidMove(Point source, Point target)
        {
            // Get the piece type
            PieceType pieceType = currentBoard[source.Y, source.X].Type;

            // Check if target is empty
            if (currentBoard[target.Y, target.X].Type != PieceType.Empty)
            {
                return false;
            }

            // Check move validity based on piece type
            switch (pieceType)
            {
                case PieceType.King:
                    // King moves one square in any direction
                    int dx = Math.Abs(target.X - source.X);
                    int dy = Math.Abs(target.Y - source.Y);
                    return dx <= 1 && dy <= 1 && (dx > 0 || dy > 0);

                case PieceType.Bishop:
                    // Bishop moves diagonally
                    return Math.Abs(target.X - source.X) == Math.Abs(target.Y - source.Y);

                case PieceType.Rook:
                    // Rook moves horizontally or vertically
                    return (target.X == source.X || target.Y == source.Y);

                default:
                    return false;
            }
        }

        private void MakeMove(Point source, Point target)
        {
            // Save move for undo
            moveHistory.Push(new Tuple<Point, Point>(source, target));

            // Move the piece
            currentBoard[target.Y, target.X] = currentBoard[source.Y, source.X];
            currentBoard[source.Y, source.X] = new ChessPiece(PieceType.Empty, null);
        }

        public bool UndoMove()
        {
            if (moveHistory.Count > 0 && !PuzzleSolved)
            {
                var move = moveHistory.Pop();
                Point source = move.Item1;
                Point target = move.Item2;

                // Undo the move
                currentBoard[source.Y, source.X] = currentBoard[target.Y, target.X];
                currentBoard[target.Y, target.X] = new ChessPiece(PieceType.Empty, null);

                return true;
            }

            return false;
        }

        public void ResetPuzzle()
        {
            // Reset the current board to the start board
            CopyBoard(startBoard, currentBoard);

            // Clear move history
            moveHistory.Clear();

            // Reset solved state
            PuzzleSolved = false;
        }

        private void CheckIfPuzzleSolved()
        {
            // Check if current board matches target board
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    if (currentBoard[row, col].Type != targetBoard[row, col].Type)
                    {
                        return;
                    }
                }
            }

            // If we get here, the puzzle is solved
            PuzzleSolved = true;
        }

        public ChessPiece GetPieceAt(int row, int col)
        {
            return currentBoard[row, col];
        }
    }
}