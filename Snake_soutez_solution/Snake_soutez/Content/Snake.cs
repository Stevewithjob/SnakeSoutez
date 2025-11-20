using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Snake_soutez
{
    public class Snake : IMovable, ICollidable, IDrawable
    {
        public List<Point> Body { get; private set; }
        public Direction CurrentDirection { get; set; }
        public Direction NextDirection { get; set; }
        public int Ammo { get; set; }
        public Color HeadColor { get; set; }
        public Color BodyColor { get; set; }
        public int PlayerNumber { get; private set; }

        public Snake(int playerNumber, Point startPosition, Direction startDirection, Color bodyColor, Color headColor)
        {
            PlayerNumber = playerNumber;
            Body = new List<Point>();

            // Initialize 3-segment snake
            for (int i = 0; i < 3; i++)
            {
                Body.Add(new Point(startPosition.X - i * GetDirectionOffset(startDirection).X,
                                  startPosition.Y - i * GetDirectionOffset(startDirection).Y));
            }

            CurrentDirection = startDirection;
            NextDirection = startDirection;
            BodyColor = bodyColor;
            HeadColor = headColor;
            Ammo = 0;
        }

        public void Move()
        {
            CurrentDirection = NextDirection;
            Point newHead = GetNewHead(Body[0], CurrentDirection);
            Body.Insert(0, newHead);
            Body.RemoveAt(Body.Count - 1);
        }

        public void Grow()
        {
            Point newHead = GetNewHead(Body[0], CurrentDirection);
            Body.Insert(0, newHead);
        }

        public bool CheckCollision(Point position)
        {
            return Body.Contains(position);
        }

        public bool CheckSelfCollision()
        {
            return Body.Skip(1).Contains(Body[0]);
        }

        public bool CheckWallCollision(int gridSize)
        {
            Point head = Body[0];
            return head.X < 0 || head.X >= gridSize || head.Y < 0 || head.Y >= gridSize;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture, int cellSize)
        {
            for (int i = 0; i < Body.Count; i++)
            {
                Color color = i == 0 ? HeadColor : BodyColor;
                spriteBatch.Draw(texture,
                    new Rectangle(Body[i].X * cellSize + 1, Body[i].Y * cellSize + 1, cellSize - 2, cellSize - 2),
                    color);
            }
        }

        private Point GetNewHead(Point current, Direction dir)
        {
            Point offset = GetDirectionOffset(dir);
            return new Point(current.X + offset.X, current.Y + offset.Y);
        }

        private Point GetDirectionOffset(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up: return new Point(0, -1);
                case Direction.Down: return new Point(0, 1);
                case Direction.Left: return new Point(-1, 0);
                case Direction.Right: return new Point(1, 0);
                default: return new Point(0, 0);
            }
        }
    }
}
