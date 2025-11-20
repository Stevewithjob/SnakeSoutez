using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Snake_soutez
{
    public class Food : ICollidable, IDrawable
    {
        public Point Position { get; private set; }
        public FoodType Type { get; private set; }
        private System.Random random;

        public Food()
        {
            random = new System.Random();
            Type = FoodType.Ammo;
        }

        public void Spawn(List<Point> occupiedPositions, int gridSize)
        {
            do
            {
                Position = new Point(random.Next(0, gridSize), random.Next(0, gridSize));
            }
            while (occupiedPositions.Contains(Position));
        }

        public bool CheckCollision(Point position)
        {
            return Position == position;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture, int cellSize)
        {
            Color foodColor = Type == FoodType.Ammo ? Color.Red : Color.Green;
            spriteBatch.Draw(texture,
                new Rectangle(Position.X * cellSize + 5, Position.Y * cellSize + 5, cellSize - 10, cellSize - 10),
                foodColor);
        }
    }
}
