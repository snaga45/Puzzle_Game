using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ChessPuzzleGame
{
    public partial class Form1 : Form
    {
        // Constants for the board layout
        private const int BOARD_MARGIN = 20;
        private const int BOARD_SPACING = 40;
        private const int CELL_SIZE = 80;
        private const int ROWS = 2;
        private const int COLS = 3;

        // Piece class
        private class ChessPiece
        {
            public PieceType Type { get; set; }
            public Image Image { get; set; }

            public ChessPiece(PieceType type, Image image)
            {
                Type = type;
                Image = image;
            }
        }

        // Board representation - initialize to avoid warnings
        private ChessPiece[,] startBoard = new ChessPiece[ROWS, COLS];
        private ChessPiece[,] currentBoard = new ChessPiece[ROWS, COLS];
        private ChessPiece[,] targetBoard = new ChessPiece[ROWS, COLS];

        // Images for pieces - use null! to suppress warnings
        private Image kingImage = null!;
        private Image bishopImage = null!;
        private Image rookImage = null!;

        // Tracking selected piece
        private Point? selectedCell = null;
        private bool puzzleSolved = false;

        // Move counter
        private int moveCount = 0;
        private Label moveCountLabel = null!;

        // AI solver
        private ChessSolver solver = null!;
        private List<Tuple<Point, Point>> solutionMoves = null!;
        private int currentSolutionMoveIndex = -1;
        private System.Windows.Forms.Timer solutionTimer = null!;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Size = new Size(
                BOARD_MARGIN * 3 + CELL_SIZE * COLS * 2 + BOARD_SPACING,
                BOARD_MARGIN * 2 + CELL_SIZE * ROWS + 100);
            this.Text = "Chess Puzzle Game";

            // Load images
            LoadImages();

            // Initialize boards
            InitializeBoards();

            // Add buttons and labels
            AddControls();
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
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    currentBoard[row, col] = new ChessPiece(startBoard[row, col].Type, startBoard[row, col].Image);
                }
            }
        }

        private void AddControls()
        {
            // Reset button
            Button resetButton = new Button
            {
                Text = "Reset",
                Location = new Point(BOARD_MARGIN, BOARD_MARGIN * 2 + CELL_SIZE * ROWS + 20),
                Size = new Size(80, 30)
            };
            resetButton.Click += ResetButton_Click;
            this.Controls.Add(resetButton);

            // Move counter label - Adjusted position
            moveCountLabel = new Label
            {
                Text = "Moves: 0",
                Location = new Point(BOARD_MARGIN + 100, BOARD_MARGIN * 2 + CELL_SIZE * ROWS + 25),
                Size = new Size(100, 20),
                Font = new Font("Arial", 10)
            };
            this.Controls.Add(moveCountLabel);

            // Auto-play button - Adjusted position
            Button autoPlayButton = new Button
            {
                Text = "Auto Play",
                Location = new Point(BOARD_MARGIN + 200, BOARD_MARGIN * 2 + CELL_SIZE * ROWS + 20),
                Size = new Size(80, 30)
            };
            autoPlayButton.Click += AutoPlayButton_Click;
            this.Controls.Add(autoPlayButton);

            // Initialize the solution timer
            solutionTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000 // 1 second between moves
            };
            solutionTimer.Tick += SolutionTimer_Tick;
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            ResetPuzzle();
        }

        private void AutoPlayButton_Click(object sender, EventArgs e)
        {
            if (solutionMoves == null)
            {
                // If no solution has been found yet, find one directly
                // Create the solver
                solver = new ChessSolver();

                // Convert your board representation to PieceType[,]
                PieceType[,] currentBoardState = new PieceType[ROWS, COLS];
                PieceType[,] targetBoardState = new PieceType[ROWS, COLS];

                // Fill the arrays with your current and target board states
                for (int row = 0; row < ROWS; row++)
                {
                    for (int col = 0; col < COLS; col++)
                    {
                        currentBoardState[row, col] = currentBoard[row, col].Type;
                        targetBoardState[row, col] = targetBoard[row, col].Type;
                    }
                }

                // Find a solution
                List<ChessSolver.Move> solution = solver.SolvePuzzle(currentBoardState, targetBoardState);

                if (solution != null)
                {
                    // Convert the solution to your Tuple format
                    solutionMoves = new List<Tuple<Point, Point>>();
                    foreach (var move in solution)
                    {
                        solutionMoves.Add(new Tuple<Point, Point>(move.Source, move.Target));
                    }

                    // Reset the current board
                    ResetPuzzle();

                    // Reset the solution index
                    currentSolutionMoveIndex = -1;
                }
                else
                {
                    MessageBox.Show("No solution found!");
                    return;
                }
            }

            // Toggle the timer
            if (solutionTimer.Enabled)
            {
                solutionTimer.Stop();
            }
            else
            {
                solutionTimer.Start();
            }
        }

        private void SolutionTimer_Tick(object sender, EventArgs e)
        {
            if (!PlayNextSolutionMove())
            {
                // Stop the timer when we reach the end of the solution
                solutionTimer.Stop();
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Draw both boards
            DrawBoard(g, currentBoard, BOARD_MARGIN, BOARD_MARGIN);
            DrawBoard(g, targetBoard, BOARD_MARGIN * 2 + CELL_SIZE * COLS + BOARD_SPACING, BOARD_MARGIN);

            // Draw labels
            using (Font font = new Font("Arial", 12, FontStyle.Bold))
            {
                g.DrawString("Current", font, Brushes.Black,
                    BOARD_MARGIN + (CELL_SIZE * COLS) / 2 - 30,
                    BOARD_MARGIN * 2 + CELL_SIZE * ROWS + 5);

                g.DrawString("Target", font, Brushes.Black,
                    BOARD_MARGIN * 2 + CELL_SIZE * COLS + BOARD_SPACING + (CELL_SIZE * COLS) / 2 - 30,
                    BOARD_MARGIN * 2 + CELL_SIZE * ROWS + 5);
            }

            // Draw message if puzzle is solved
            if (puzzleSolved)
            {
                using (Font font = new Font("Arial", 16, FontStyle.Bold))
                using (SolidBrush brush = new SolidBrush(Color.Green))
                {
                    g.DrawString("Puzzle Solved!", font, brush,
                        BOARD_MARGIN + CELL_SIZE * COLS + BOARD_SPACING / 2 - 60,
                        BOARD_MARGIN + CELL_SIZE * ROWS / 2 - 10);
                }
            }
        }

        private void DrawBoard(Graphics g, ChessPiece[,] board, int startX, int startY)
        {
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
                    if (board == currentBoard && selectedCell.HasValue &&
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

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            // Only handle clicks on the current board
            if (e.X < BOARD_MARGIN || e.X >= BOARD_MARGIN + CELL_SIZE * COLS ||
                e.Y < BOARD_MARGIN || e.Y >= BOARD_MARGIN + CELL_SIZE * ROWS ||
                puzzleSolved)
            {
                return;
            }

            // Calculate which cell was clicked
            int col = (e.X - BOARD_MARGIN) / CELL_SIZE;
            int row = (e.Y - BOARD_MARGIN) / CELL_SIZE;

            // Handle the click
            HandleCellClick(row, col);
        }

        private void HandleCellClick(int row, int col)
        {
            if (!selectedCell.HasValue)
            {
                // No piece selected yet, try to select one
                if (currentBoard[row, col].Type != PieceType.Empty)
                {
                    selectedCell = new Point(col, row);
                    Invalidate();
                }
            }
            else
            {
                // A piece is already selected, try to move it
                Point source = selectedCell.Value;
                Point target = new Point(col, row);

                if (source.X == target.X && source.Y == target.Y)
                {
                    // Clicked the same cell, deselect
                    selectedCell = null;
                    Invalidate();
                    return;
                }

                // Check if the move is valid
                if (IsValidMove(source, target))
                {
                    // Make the move
                    MakeMove(source, target);

                    // Check if puzzle is solved
                    CheckIfPuzzleSolved();

                    // Update move count
                    moveCount++;
                    moveCountLabel.Text = $"Moves: {moveCount}";

                    // Reset selection
                    selectedCell = null;
                    Invalidate();
                }
            }
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
            // Move the piece
            currentBoard[target.Y, target.X] = new ChessPiece(
                currentBoard[source.Y, source.X].Type,
                currentBoard[source.Y, source.X].Image);
            currentBoard[source.Y, source.X] = new ChessPiece(PieceType.Empty, null);
        }

        private void ResetPuzzle()
        {
            // Reset the current board to the start board
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    currentBoard[row, col] = new ChessPiece(startBoard[row, col].Type, startBoard[row, col].Image);
                }
            }

            // Reset move count
            moveCount = 0;
            moveCountLabel.Text = $"Moves: {moveCount}";

            // Reset selection and solved state
            selectedCell = null;
            puzzleSolved = false;

            Invalidate();
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
            puzzleSolved = true;
        }

        private bool PlayNextSolutionMove()
        {
            if (solutionMoves == null || currentSolutionMoveIndex >= solutionMoves.Count - 1)
            {
                return false;
            }

            currentSolutionMoveIndex++;
            Tuple<Point, Point> move = solutionMoves[currentSolutionMoveIndex];

            // Make the move on the current board
            MakeMove(move.Item1, move.Item2);

            // Check if puzzle is solved
            CheckIfPuzzleSolved();

            // Update move count
            moveCount++;
            moveCountLabel.Text = $"Moves: {moveCount}";

            // Redraw the board
            Invalidate();

            return true;
        }
    }
}