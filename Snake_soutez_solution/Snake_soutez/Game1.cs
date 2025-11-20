using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace Snake_soutez
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private const int GridSize = 30;
        private const int CellSize = 50;
        private Texture2D pixelTexture;

        // Snake 1 (WASD)
        private List<Point> snake1 = new List<Point>();
        private Direction direction1 = Direction.Right;
        private Direction nextDirection1 = Direction.Right;
        private int ammo1 = 0;
        private Color snake1Color = Color.DarkRed;

        // Snake 2 (Arrows)
        private List<Point> snake2 = new List<Point>();
        private Direction direction2 = Direction.Left;
        private Direction nextDirection2 = Direction.Left;
        private int ammo2 = 0;
        private Color snake2Color = Color.DarkBlue;

        // Food and bullets
        private Point food;
        private List<Bullet> bullets = new List<Bullet>();

        private bool gameOver = false;
        private string winner = "";

        private float moveTimer = 0f;
        private float moveInterval = 0.3f; // pomalí hadi
        private float bulletMoveInterval = 0.05f; // rychlé střely
        private float bulletMoveTimer = 0f;

        private KeyboardState previousKeyState;
        private SpriteFont font;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = GridSize * CellSize;
            _graphics.PreferredBackBufferHeight = GridSize * CellSize;
        }

        protected override void Initialize()
        {
            // Initialize snakes
            snake1.Add(new Point(5, 12));
            snake1.Add(new Point(4, 12));
            snake1.Add(new Point(3, 12));

            snake2.Add(new Point(19, 12));
            snake2.Add(new Point(20, 12));
            snake2.Add(new Point(21, 12));

            SpawnFood();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create 1x1 white pixel texture
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });

            // Try to load font, if not available we'll draw without text
            try
            {
                font = Content.Load<SpriteFont>("Font");
            }
            catch
            {
                font = null;
            }
        }

        private void SpawnFood()
        {
            var random = new System.Random();
            do
            {
                food = new Point(random.Next(0, GridSize), random.Next(0, GridSize));
            }
            while (snake1.Contains(food) || snake2.Contains(food));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (gameOver)
            {
                base.Update(gameTime);
                return;
            }

            KeyboardState keyState = Keyboard.GetState();

            // Snake 1 controls (WASD)
            if (keyState.IsKeyDown(Keys.W) && direction1 != Direction.Down)
                nextDirection1 = Direction.Up;
            else if (keyState.IsKeyDown(Keys.S) && direction1 != Direction.Up)
                nextDirection1 = Direction.Down;
            else if (keyState.IsKeyDown(Keys.A) && direction1 != Direction.Right)
                nextDirection1 = Direction.Left;
            else if (keyState.IsKeyDown(Keys.D) && direction1 != Direction.Left)
                nextDirection1 = Direction.Right;

            if (keyState.IsKeyDown(Keys.Space) && previousKeyState.IsKeyUp(Keys.Space) && ammo1 > 0)
            {
                ShootBullet(snake1, direction1, 1);
                ammo1--;
            }

            // Snake 2 controls (Arrows)
            if (keyState.IsKeyDown(Keys.Up) && direction2 != Direction.Down)
                nextDirection2 = Direction.Up;
            else if (keyState.IsKeyDown(Keys.Down) && direction2 != Direction.Up)
                nextDirection2 = Direction.Down;
            else if (keyState.IsKeyDown(Keys.Left) && direction2 != Direction.Right)
                nextDirection2 = Direction.Left;
            else if (keyState.IsKeyDown(Keys.Right) && direction2 != Direction.Left)
                nextDirection2 = Direction.Right;

            if (keyState.IsKeyDown(Keys.Enter) && previousKeyState.IsKeyUp(Keys.Enter) && ammo2 > 0)
            {
                ShootBullet(snake2, direction2, 2);
                ammo2--;
            }

            previousKeyState = keyState;

            // Game loop timer
            moveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            bulletMoveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (moveTimer >= moveInterval)
            {
                moveTimer = 0f;
                GameLoop();
            }

            // Samostatný pohyb pro střely - rychlejší
            if (bulletMoveTimer >= bulletMoveInterval)
            {
                bulletMoveTimer = 0f;
                MoveBullets();
            }


            base.Update(gameTime);
        }


        private void ShootBullet(List<Point> snake, Direction dir, int shooter)
        {
            bullets.Add(new Bullet
            {
                Position = new Point(snake[0].X, snake[0].Y),
                Direction = dir,
                Shooter = shooter
            });
        }
        private void MoveBullets()
        {
            if (gameOver) return;

            // Move bullets
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i].Position = GetNewHead(bullets[i].Position, bullets[i].Direction);

                // Remove bullets out of bounds
                if (bullets[i].Position.X < 0 || bullets[i].Position.X >= GridSize ||
                    bullets[i].Position.Y < 0 || bullets[i].Position.Y >= GridSize)
                {
                    bullets.RemoveAt(i);
                    continue;
                }

                // Check bullet collision with snakes
                if (bullets[i].Shooter == 1 && snake2.Contains(bullets[i].Position))
                {
                    gameOver = true;
                    winner = "CERVENY HAD (WASD) VYHRAL!";
                }
                else if (bullets[i].Shooter == 2 && snake1.Contains(bullets[i].Position))
                {
                    gameOver = true;
                    winner = "ZELENY HAD (SIPKY) VYHRAL!";
                }
            }
        }

        private void GameLoop()
        {
            direction1 = nextDirection1;
            direction2 = nextDirection2;

            // Move snake 1
            Point newHead1 = GetNewHead(snake1[0], direction1);
            snake1.Insert(0, newHead1);

            // Check if snake 1 ate food
            if (newHead1 == food)
            {
                ammo1++;
                SpawnFood();
            }
            else
            {
                snake1.RemoveAt(snake1.Count - 1);
            }

            // Move snake 2
            Point newHead2 = GetNewHead(snake2[0], direction2);
            snake2.Insert(0, newHead2);

            // Check if snake 2 ate food
            if (newHead2 == food)
            {
                ammo2++;
                SpawnFood();
            }
            else
            {
                snake2.RemoveAt(snake2.Count - 1);
            }

            

            // Check collisions with walls
            if (newHead1.X < 0 || newHead1.X >= GridSize || newHead1.Y < 0 || newHead1.Y >= GridSize)
            {
                gameOver = true;
                winner = "MODRY HAD VYHRAL! Cerveny narazil do zdi!";
            }

            if (newHead2.X < 0 || newHead2.X >= GridSize || newHead2.Y < 0 || newHead2.Y >= GridSize)
            {
                gameOver = true;
                winner = "CERVENY HAD VYHRAL! Modry narazil do zdi!";
            }

            // Check self collision
            if (snake1.Skip(1).Contains(newHead1))
            {
                gameOver = true;
                winner = "MODRY HAD VYHRAL! Cerveny se kousl!";
            }

            if (snake2.Skip(1).Contains(newHead2))
            {
                gameOver = true;
                winner = "CERVENY HAD VYHRAL! Modry se kousl!";
            }

            // Check head-to-head collision
            if (newHead1 == newHead2)
            {
                gameOver = true;
                winner = "REMIZA! Oba hadi se srazili!";
            }
        }

        private Point GetNewHead(Point current, Direction dir)
        {
            Point newHead = current;
            switch (dir)
            {
                case Direction.Up: newHead.Y--; break;
                case Direction.Down: newHead.Y++; break;
                case Direction.Left: newHead.X--; break;
                case Direction.Right: newHead.X++; break;
            }
            return newHead;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SandyBrown);

            _spriteBatch.Begin();

            // Draw checkerboard background
            //for (int i = 0; i < GridSize; i++)
            //{
            //    for (int j = 0; j < GridSize; j++)
            //    {
            //        Color bgColor = (i + j) % 2 == 0 ? Color.SandyBrown : Color.Peru;
            //        _spriteBatch.Draw(pixelTexture, new Rectangle(i * CellSize, j * CellSize, CellSize, CellSize), bgColor);
            //    }
            //}

            // Blue background
            _spriteBatch.Draw(pixelTexture,
                new Rectangle(0, 0, GridSize * CellSize, GridSize * CellSize),
                Color.CornflowerBlue); // nebo třeba Color.Blue

            // Draw snake 1
            for (int i = 0; i < snake1.Count; i++)
            {
                Color color = i == 0 ? Color.Firebrick : snake1Color;
                _spriteBatch.Draw(pixelTexture, new Rectangle(snake1[i].X * CellSize + 1, snake1[i].Y * CellSize + 1, CellSize - 2, CellSize - 2), color);
            }

            // Draw snake 2
            for (int i = 0; i < snake2.Count; i++)
            {
                Color color = i == 0 ? Color.Navy : snake2Color;
                _spriteBatch.Draw(pixelTexture, new Rectangle(snake2[i].X * CellSize + 1, snake2[i].Y * CellSize + 1, CellSize - 2, CellSize - 2), color);
            }

            // Draw food (apple)
            _spriteBatch.Draw(pixelTexture, new Rectangle(food.X * CellSize + 5, food.Y * CellSize + 5, CellSize - 10, CellSize - 10), Color.Red);

            // Draw bullets
            foreach (var bullet in bullets)
            {
                Color bulletColor = bullet.Shooter == 1 ? Color.Orange : Color.Yellow;
                _spriteBatch.Draw(pixelTexture, new Rectangle(bullet.Position.X * CellSize + 8, bullet.Position.Y * CellSize + 8, 9, 9), bulletColor);
            }

            // Draw HUD
            if (font != null)
            {
                _spriteBatch.DrawString(font, $"Cerveny naboje: {ammo1}", new Vector2(10, 10), Color.Red);
                _spriteBatch.DrawString(font, $"Modry naboje: {ammo2}", new Vector2(GridSize * CellSize - 200, 10), Color.Blue);

                if (gameOver)
                {
                    Vector2 textSize = font.MeasureString(winner);
                    Vector2 position = new Vector2((GridSize * CellSize - textSize.X) / 2, (GridSize * CellSize - textSize.Y) / 2);

                    // Draw semi-transparent background
                    _spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, GridSize * CellSize, GridSize * CellSize), Color.Black * 0.7f);

                    _spriteBatch.DrawString(font, winner, position, Color.White);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public enum Direction
        {
            Up, Down, Left, Right
        }

        public class Bullet
        {
            public Point Position { get; set; }
            public Direction Direction { get; set; }
            public int Shooter { get; set; }
        }
    }
}