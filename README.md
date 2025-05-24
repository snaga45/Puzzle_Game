# Chess Puzzle Game

A chess puzzle game implemented in C# for EKKE university assignment.

## Game Description

- 2x3 chess board with King, Bishop, and Rook pieces
- Move pieces from start position to match target position
- Follow standard chess movement rules
- Features: Auto Play AI solver, move counter, reset/undo buttons

## How to Use

1. **Select piece** by clicking on it (piece will be highlighted in blue)
2. **Move piece** by clicking on a valid destination square
3. **Goal**: Arrange pieces to match the target board layout
4. **Reset**: Start over from beginning position
5. **Undo**: Reverse the last move made
6. **Auto Play**: Watch AI solve the puzzle automatically

## AI Solving Methods

The game includes multiple AI algorithms for solving the puzzle:

- **Breadth-First Search (BFS)** - Guaranteed optimal solution
- **Trial-Error** - Random move generation with multiple attempts
- **Trial-Error with Depth** - Limited depth random search
- **Backtracking** - Systematic exploration with backtracking
- **Depth-First Search (DFS)** - Deep exploration of move sequences
- **A\* Algorithm** - Heuristic-based optimal pathfinding

## Technical Implementation

- **Platform**: C# Windows Forms Application
- **Framework**: .NET Framework 4.7.2+
- **Architecture**: Modular design with separate classes:
  - `BoardManager` - Game logic and board state management
  - `ChessSolver` - AI algorithms implementation
  - `ChessPiece` - Piece representation and properties
  - `PieceType` - Enumeration for piece types
- **Development**: Visual Studio required for compilation

## Project Structure

```
ChessPuzzleGame/
├── Form1.cs              # Main game interface
├── BoardManager.cs       # Board state and game logic
├── ChessSolver.cs        # AI solving algorithms
├── ChessPiece.cs         # Chess piece class
├── PieceType.cs          # Piece type enumeration
└── Program.cs            # Application entry point
```

## Installation and Setup

### Prerequisites
- Visual Studio 2019 or later
- .NET Framework 4.7.2 or later
- Windows Operating System

### Running the Application
1. Clone or download the project files
2. Open `ChessPuzzleGame.sln` in Visual Studio
3. Build the solution (Ctrl+Shift+B)
4. Run the application (F5)

## Chess Piece Movement Rules

- **King**: Moves one square in any direction (horizontal, vertical, diagonal)
- **Bishop**: Moves diagonally any number of squares
- **Rook**: Moves horizontally or vertically any number of squares

## Game Features

- Interactive chess board with click-to-move functionality
- Visual feedback for selected pieces
- Move counter to track progress
- Automatic puzzle completion detection
- Multiple AI solving algorithms with performance comparison
- Auto-play mode with pause/resume functionality

---
*Submitted for Professor Ede's class review*

*EKKE University Project - 2025*
