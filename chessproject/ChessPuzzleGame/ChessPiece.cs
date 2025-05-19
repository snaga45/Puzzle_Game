using System.Drawing;

namespace ChessPuzzleGame  // Make sure this matches your project namespace
{
    public class ChessPiece
    {
        public PieceType Type { get; set; }
        public Image Image { get; set; }

        public ChessPiece(PieceType type, Image image)
        {
            Type = type;
            Image = image;
        }

        public ChessPiece Clone()
        {
            return new ChessPiece(Type, Image);
        }
    }
}