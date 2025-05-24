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

        // Board manager to handle game logic
        private BoardManager boardManager;

        // Tracking selected piece
        private Point? selectedCell = null;

        // Move counter
        private int moveCount = 0;
        private Label moveCountLabel;

        // AI solver
        private ChessSolver solver;
        private List<ChessSolver.Move> solutionMoves;
        private int currentSolutionMoveIndex = -1;
        private System.Windows.Forms.Timer solutionTimer;

        // Algorithm controls
        private ComboBox algorithmComboBox;
        private Label algorithmLabel;
        private Label statsLabel;
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Size = new Size(
                BOARD_MARGIN * 3 + BoardManager.CELL_SIZE * BoardManager.COLS * 2 + BOARD_SPACING,
                BOARD_MARGIN * 2 + BoardManager.CELL_SIZE * BoardManager.ROWS + 150);
            this.Text = "Chess Puzzle Game";

            // Initialize board manager
            boardManager = new BoardManager();

            // Initialize solver
            solver = new ChessSolver();

            // Add controls
            AddControls();

            // Pre-calculate solution to avoid "no solution found" error
            PreCalculateSolution();
        }

        private void PreCalculateSolution()
        {
            try
            {
                // Convert board to PieceType arrays for solver
                PieceType[,] currentBoardState = new PieceType[BoardManager.ROWS, BoardManager.COLS];
                PieceType[,] targetBoardState = new PieceType[BoardManager.ROWS, BoardManager.COLS];

                // Get current board state
                for (int row = 0; row < BoardManager.ROWS; row++)
                {
                    for (int col = 0; col < BoardManager.COLS; col++)
                    {
                        currentBoardState[row, col] = boardManager.GetPieceAt(row, col).Type;
                    }
                }

                // Set target board state (based on your puzzle setup)
                targetBoardState[0, 0] = PieceType.Empty;
                targetBoardState[0, 1] = PieceType.Bishop;
                targetBoardState[0, 2] = PieceType.Bishop;
                targetBoardState[1, 0] = PieceType.Rook;
                targetBoardState[1, 1] = PieceType.Rook;
                targetBoardState[1, 2] = PieceType.King;

                // Find solution using BFS (most reliable)
                solutionMoves = solver.SolvePuzzle(currentBoardState, targetBoardState);
            }
            catch (Exception ex)
            {
                // If solution fails, create an empty list to prevent errors
                solutionMoves = new List<ChessSolver.Move>();
                System.Diagnostics.Debug.WriteLine($"Solution calculation failed: {ex.Message}");
            }
        }

        private void AddControls()
        {
            // Reset button
            Button resetButton = new Button
            {
                Text = "Reset",
                Location = new Point(BOARD_MARGIN, BOARD_MARGIN * 2 + BoardManager.CELL_SIZE * BoardManager.ROWS + 20),
                Size = new Size(80, 30)
            };
            resetButton.Click += ResetButton_Click;
            this.Controls.Add(resetButton);

            // Undo button
            Button undoButton = new Button
            {
                Text = "Undo",
                Location = new Point(BOARD_MARGIN + 100, BOARD_MARGIN * 2 + BoardManager.CELL_SIZE * BoardManager.ROWS + 20),
                Size = new Size(80, 30)
            };
            undoButton.Click += UndoButton_Click;
            this.Controls.Add(undoButton);

            // Move counter label
            moveCountLabel = new Label
            {
                Text = "Moves: 0",
                Location = new Point(BOARD_MARGIN + 200, BOARD_MARGIN * 2 + BoardManager.CELL_SIZE * BoardManager.ROWS + 25),
                Size = new Size(100, 20),
                Font = new Font("Arial", 10)
            };
            this.Controls.Add(moveCountLabel);

            // Auto-play button (kept but no solve button)
            Button autoPlayButton = new Button
            {
                Text = "Auto Play",
                Location = new Point(BOARD_MARGIN + 300, BOARD_MARGIN * 2 + BoardManager.CELL_SIZE * BoardManager.ROWS + 20),
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

            // Algorithm selection
            algorithmLabel = new Label
            {
                Text = "Algorithm:",
                Location = new Point(BOARD_MARGIN, BOARD_MARGIN * 2 + BoardManager.CELL_SIZE * BoardManager.ROWS + 60),
                Size = new Size(80, 20)
            };
            this.Controls.Add(algorithmLabel);

            algorithmComboBox = new ComboBox
            {
                Location = new Point(BOARD_MARGIN + 80, BOARD_MARGIN * 2 + BoardManager.CELL_SIZE * BoardManager.ROWS + 60),
                Size = new Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            algorithmComboBox.Items.AddRange(new object[] {
                "Breadth-First Search",
                "Trial-Error",
                "Trial-Error with Depth",
                "Backtracking",
                "Depth-First Search",
                "A* Algorithm"
            });

            algorithmComboBox.SelectedIndex = 0; // Default to BFS
            this.Controls.Add(algorithmComboBox);

            // Stats label
            statsLabel = new Label
            {
                Text = "Ready to play! Use Auto Play to see the solution.",
                Location = new Point(BOARD_MARGIN, BOARD_MARGIN * 2 + BoardManager.CELL_SIZE * BoardManager.ROWS + 90),
                Size = new Size(400, 40),
                Font = new Font("Arial", 9)
            };
            this.Controls.Add(statsLabel);
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            boardManager.ResetPuzzle();
            moveCount = 0;
            moveCountLabel.Text = $"Moves: {moveCount}";
            selectedCell = null;
            currentSolutionMoveIndex = -1;
            solutionTimer.Stop();
            Invalidate();
        }

        private void UndoButton_Click(object sender, EventArgs e)
        {
            if (boardManager.UndoMove())
            {
                moveCount = Math.Max(0, moveCount - 1);
                moveCountLabel.Text = $"Moves: {moveCount}";
                Invalidate();
            }
        }

        private void AutoPlayButton_Click(object sender, EventArgs e)
        {
            if (solutionMoves == null || solutionMoves.Count == 0)
            {
                // Recalculate solution if needed
                PreCalculateSolution();

                if (solutionMoves == null || solutionMoves.Count == 0)
                {
                    MessageBox.Show("No solution available for auto-play!");
                    return;
                }
            }

            // Reset to start position for auto-play
            boardManager.ResetPuzzle();
            moveCount = 0;
            moveCountLabel.Text = $"Moves: {moveCount}";
            currentSolutionMoveIndex = -1;

            // Toggle the timer
            if (solutionTimer.Enabled)
            {
                solutionTimer.Stop();
                statsLabel.Text = "Auto-play paused. Click Auto Play to resume.";
            }
            else
            {
                solutionTimer.Start();
                statsLabel.Text = "Auto-play started. Click Auto Play to pause.";
            }
        }

        private void SolutionTimer_Tick(object sender, EventArgs e)
        {
            if (!PlayNextSolutionMove())
            {
                solutionTimer.Stop();
                statsLabel.Text = "Auto-play completed!";
            }
        }

        private bool PlayNextSolutionMove()
        {
            if (solutionMoves == null || currentSolutionMoveIndex >= solutionMoves.Count - 1)
            {
                return false;
            }

            currentSolutionMoveIndex++;
            ChessSolver.Move move = solutionMoves[currentSolutionMoveIndex];

            // Make the move using BoardManager
            if (boardManager.TryMove(move.Source, move.Target))
            {
                moveCount++;
                moveCountLabel.Text = $"Moves: {moveCount}";
                Invalidate();
                return true;
            }

            return false;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Draw current board
            boardManager.DrawBoard(g, true, BOARD_MARGIN, BOARD_MARGIN, selectedCell);

            // Draw target board
            boardManager.DrawBoard(g, false, BOARD_MARGIN * 2 + BoardManager.CELL_SIZE * BoardManager.COLS + BOARD_SPACING, BOARD_MARGIN);

            // Draw labels
            using (Font font = new Font("Arial", 12, FontStyle.Bold))
            {
                g.DrawString("Current", font, Brushes.Black,
                    BOARD_MARGIN + (BoardManager.CELL_SIZE * BoardManager.COLS) / 2 - 30,
                    BOARD_MARGIN * 2 + BoardManager.CELL_SIZE * BoardManager.ROWS + 5);

                g.DrawString("Target", font, Brushes.Black,
                    BOARD_MARGIN * 2 + BoardManager.CELL_SIZE * BoardManager.COLS + BOARD_SPACING + (BoardManager.CELL_SIZE * BoardManager.COLS) / 2 - 30,
                    BOARD_MARGIN * 2 + BoardManager.CELL_SIZE * BoardManager.ROWS + 5);
            }

            // Draw message if puzzle is solved
            if (boardManager.PuzzleSolved)
            {
                using (Font font = new Font("Arial", 16, FontStyle.Bold))
                using (SolidBrush brush = new SolidBrush(Color.Green))
                {
                    g.DrawString("Puzzle Solved!", font, brush,
                        BOARD_MARGIN + BoardManager.CELL_SIZE * BoardManager.COLS + BOARD_SPACING / 2 - 60,
                        BOARD_MARGIN + BoardManager.CELL_SIZE * BoardManager.ROWS / 2 - 10);
                }
            }
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            // Only handle clicks on the current board
            if (e.X < BOARD_MARGIN || e.X >= BOARD_MARGIN + BoardManager.CELL_SIZE * BoardManager.COLS ||
                e.Y < BOARD_MARGIN || e.Y >= BOARD_MARGIN + BoardManager.CELL_SIZE * BoardManager.ROWS ||
                boardManager.PuzzleSolved)
            {
                return;
            }

            // Calculate which cell was clicked
            int col = (e.X - BOARD_MARGIN) / BoardManager.CELL_SIZE;
            int row = (e.Y - BOARD_MARGIN) / BoardManager.CELL_SIZE;

            HandleCellClick(row, col);
        }

        private void HandleCellClick(int row, int col)
        {
            if (!selectedCell.HasValue)
            {
                // No piece selected yet, try to select one
                if (boardManager.GetPieceAt(row, col).Type != PieceType.Empty)
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

                // Try to make the move
                if (boardManager.TryMove(source, target))
                {
                    moveCount++;
                    moveCountLabel.Text = $"Moves: {moveCount}";
                    selectedCell = null;
                    Invalidate();
                }
            }
        }
    }

    // Simple priority queue implementation for A* algorithm
    public class PriorityQueue<T, TPriority> where TPriority : IComparable<TPriority>
    {
        private List<Tuple<T, TPriority>> elements = new List<Tuple<T, TPriority>>();

        public int Count => elements.Count;

        public void Enqueue(T item, TPriority priority)
        {
            elements.Add(Tuple.Create(item, priority));
        }

        public T Dequeue()
        {
            int bestIndex = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Item2.CompareTo(elements[bestIndex].Item2) < 0)
                {
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].Item1;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }
}