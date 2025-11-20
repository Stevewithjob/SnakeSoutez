using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Snake_soutez
{
    public class Bullet : IMovable, IDrawable
    {
        public Point Position { get; set; }
        public Direction CurrentDirection { get; set; }
        public int Shooter { get; set; }

        public Bullet(Point startPosition, Direction direction, int shooter)
        {
            Position = startPosition;
            CurrentDirection = direction;
            Shooter = shooter;
        }

        public void Move()
        {
            switch (CurrentDirection)
            {
                case Direction.Up: Position = new Point(Position.X, Position.Y - 1); break;
                case Direction.Down: Position = new Point(Position.X, Position.Y + 1); break;
                case Direction.Left: Position = new Point(Position.X - 1, Position.Y); break;
                case Direction.Right: Position = new Point(Position.X + 1, Position.Y); break;
            }
        }

        public bool IsOutOfBounds(int gridSize)
        {
            return Position.X < 0 || Position.X >= gridSize || Position.Y < 0 || Position.Y >= gridSize;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture, int cellSize)
        {
            Color bulletColor = Shooter == 1 ? Color.Orange : Color.Yellow;
            spriteBatch.Draw(texture,
                new Rectangle(Position.X * cellSize + 8, Position.Y * cellSize + 8, 9, 9),
                bulletColor);
        }
    }
}
