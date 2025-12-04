using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake_soutez
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private const int GridSize = 30;
        private const int CellSize = 30;
        private Texture2D pixelTexture;

        // Snake 1 (WASD)
        private List<Point> snake1 = new List<Point>();
        private Direction direction1 = Direction.Right;
        private Direction nextDirection1 = Direction.Right;
        private int ammo1 = 0;
        private int lives1 = 3;
        private Color snake1Color = new Color(139, 69, 19);

        // Snake 2 (Arrows)
        private List<Point> snake2 = new List<Point>();
        private Direction direction2 = Direction.Left;
        private Direction nextDirection2 = Direction.Left;
        private int ammo2 = 0;
        private int lives2 = 3;
        private Color snake2Color = new Color(85, 107, 47);

        // Food and bullets
        private Point food;
        private List<Bullet> bullets = new List<Bullet>();
        private List<ParticleEffect> particles = new List<ParticleEffect>();

        private bool gameOver = false;
        private string winner = "";

        private float moveTimer = 0f;
        private float moveInterval = 0.25f;
        private float bulletMoveInterval = 0.07f;
        private float bulletMoveTimer = 0f;

        private KeyboardState previousKeyState;
        private SpriteFont font;

        private Vector2 screenShake = Vector2.Zero;
        private Random random = new Random();

        private Color desertSand = new Color(210, 180, 140);
        private Color darkSand = new Color(180, 150, 110);
        private Color cactusGreen = new Color(60, 90, 40);

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
            InitializeGame();
            base.Initialize();
        }

        private void InitializeGame()
        {
            snake1.Clear();
            snake1.Add(new Point(5, 12));
            snake1.Add(new Point(4, 12));
            snake1.Add(new Point(3, 12));

            snake2.Clear();
            snake2.Add(new Point(19, 12));
            snake2.Add(new Point(20, 12));
            snake2.Add(new Point(21, 12));

            SpawnFood();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });

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
            do
            {
                food = new Point(random.Next(0, GridSize), random.Next(0, GridSize));
            }
            while (snake1.Contains(food) || snake2.Contains(food));
        }

        private void ResetGame()
        {
            snake1.Clear();
            snake2.Clear();
            bullets.Clear();
            particles.Clear();

            snake1.Add(new Point(5, 12));
            snake1.Add(new Point(4, 12));
            snake1.Add(new Point(3, 12));

            snake2.Add(new Point(19, 12));
            snake2.Add(new Point(20, 12));
            snake2.Add(new Point(21, 12));

            direction1 = Direction.Right;
            nextDirection1 = Direction.Right;
            direction2 = Direction.Left;
            nextDirection2 = Direction.Left;

            ammo1 = 0;
            ammo2 = 0;
            lives1 = 3;
            lives2 = 3;

            gameOver = false;
            winner = "";
            screenShake = Vector2.Zero;
            moveTimer = 0f;
            bulletMoveTimer = 0f;

            SpawnFood();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState keyState = Keyboard.GetState();

            screenShake *= 0.85f;
            if (screenShake.Length() < 0.1f)
                screenShake = Vector2.Zero;

            for (int i = particles.Count - 1; i >= 0; i--)
            {
                particles[i].Update();
                if (particles[i].IsDead)
                    particles.RemoveAt(i);
            }

            if (gameOver)
            {
                if (keyState.IsKeyDown(Keys.R) && previousKeyState.IsKeyUp(Keys.R))
                {
                    ResetGame();
                }

                previousKeyState = keyState;
                base.Update(gameTime);
                return;
            }

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
                CreateMuzzleFlash(snake1[0], 1);
                screenShake = new Vector2(random.Next(-3, 4), random.Next(-3, 4));
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

            if ((keyState.IsKeyDown(Keys.RightShift) && previousKeyState.IsKeyUp(Keys.RightShift) && ammo2 > 0) ||
                (keyState.IsKeyDown(Keys.Enter) && previousKeyState.IsKeyUp(Keys.Enter) && ammo2 > 0))
            {
                ShootBullet(snake2, direction2, 2);
                ammo2--;
                CreateMuzzleFlash(snake2[0], 2);
                screenShake = new Vector2(random.Next(-3, 4), random.Next(-3, 4));
            }

            previousKeyState = keyState;

            moveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            bulletMoveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (moveTimer >= moveInterval)
            {
                moveTimer = 0f;
                GameLoop();
            }

            if (bulletMoveTimer >= bulletMoveInterval)
            {
                bulletMoveTimer = 0f;
                MoveBullets();
            }

            base.Update(gameTime);
        }

        private void CreateMuzzleFlash(Point position, int shooter)
        {
            Color smokeColor = shooter == 1 ? new Color(255, 200, 100) : new Color(255, 220, 150);
            for (int i = 0; i < 8; i++)
            {
                particles.Add(new ParticleEffect
                {
                    Position = new Vector2(position.X * CellSize + CellSize / 2, position.Y * CellSize + CellSize / 2),
                    Velocity = new Vector2(random.Next(-30, 31) / 10f, random.Next(-30, 31) / 10f),
                    Color = smokeColor,
                    Life = 0.4f,
                    MaxLife = 0.4f,
                    Size = random.Next(3, 7)
                });
            }
        }

        private void CreateHitEffect(Point position)
        {
            screenShake = new Vector2(random.Next(-8, 9), random.Next(-8, 9));
            for (int i = 0; i < 20; i++)
            {
                particles.Add(new ParticleEffect
                {
                    Position = new Vector2(position.X * CellSize + CellSize / 2, position.Y * CellSize + CellSize / 2),
                    Velocity = new Vector2(random.Next(-60, 61) / 10f, random.Next(-60, 61) / 10f),
                    Color = new Color(180, 0, 0),
                    Life = 0.6f,
                    MaxLife = 0.6f,
                    Size = random.Next(2, 5)
                });
            }
        }

        private void CreateEatEffect(Point position)
        {
            for (int i = 0; i < 10; i++)
            {
                particles.Add(new ParticleEffect
                {
                    Position = new Vector2(position.X * CellSize + CellSize / 2, position.Y * CellSize + CellSize / 2),
                    Velocity = new Vector2(random.Next(-40, 41) / 10f, random.Next(-40, 41) / 10f),
                    Color = new Color(255, 215, 0),
                    Life = 0.5f,
                    MaxLife = 0.5f,
                    Size = random.Next(2, 4)
                });
            }
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

            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i].Position = GetNewHead(bullets[i].Position, bullets[i].Direction);

                particles.Add(new ParticleEffect
                {
                    Position = new Vector2(bullets[i].Position.X * CellSize + CellSize / 2,
                                          bullets[i].Position.Y * CellSize + CellSize / 2),
                    Velocity = Vector2.Zero,
                    Color = new Color(139, 69, 19, 100),
                    Life = 0.2f,
                    MaxLife = 0.2f,
                    Size = 3
                });

                if (bullets[i].Position.X < 0 || bullets[i].Position.X >= GridSize ||
                    bullets[i].Position.Y < 0 || bullets[i].Position.Y >= GridSize)
                {
                    bullets.RemoveAt(i);
                    continue;
                }

                // Zásah do Snake 2
                if (bullets[i].Shooter == 1 && snake2.Contains(bullets[i].Position))
                {
                    CreateHitEffect(bullets[i].Position);
                    bullets.RemoveAt(i);

                    lives2--;

                    // Odeber segment ze snake 2
                    if (snake2.Count > 1)
                    {
                        snake2.RemoveAt(snake2.Count - 1);
                    }

                    if (lives2 <= 0)
                    {
                        gameOver = true;
                        winner = "CERVENY HAD VYHRAL!";
                    }
                    continue;
                }

                // Zásah do Snake 1
                if (bullets[i].Shooter == 2 && snake1.Contains(bullets[i].Position))
                {
                    CreateHitEffect(bullets[i].Position);
                    bullets.RemoveAt(i);

                    lives1--;

                    // Odeber segment ze snake 1
                    if (snake1.Count > 1)
                    {
                        snake1.RemoveAt(snake1.Count - 1);
                    }

                    if (lives1 <= 0)
                    {
                        gameOver = true;
                        winner = "ZELENY HAD VYHRAL!";
                    }
                    continue;
                }
            }
        }

        private void GameLoop()
        {
            direction1 = nextDirection1;
            direction2 = nextDirection2;

            Point newHead1 = GetNewHead(snake1[0], direction1);
            snake1.Insert(0, newHead1);

            if (newHead1 == food)
            {
                ammo1++;
                CreateEatEffect(food);
                SpawnFood();
            }
            else
            {
                snake1.RemoveAt(snake1.Count - 1);
            }

            Point newHead2 = GetNewHead(snake2[0], direction2);
            snake2.Insert(0, newHead2);

            if (newHead2 == food)
            {
                ammo2++;
                CreateEatEffect(food);
                SpawnFood();
            }
            else
            {
                snake2.RemoveAt(snake2.Count - 1);
            }

            if (newHead1.X < 0 || newHead1.X >= GridSize || newHead1.Y < 0 || newHead1.Y >= GridSize)
            {
                gameOver = true;
                winner = "ZELENY HAD VYHRAL! Cerveny narazil!";
            }

            if (newHead2.X < 0 || newHead2.X >= GridSize || newHead2.Y < 0 || newHead2.Y >= GridSize)
            {
                gameOver = true;
                winner = "CERVENY HAD VYHRAL! Zeleny narazil!";
            }

            if (snake1.Skip(1).Contains(newHead1))
            {
                gameOver = true;
                winner = "ZELENY HAD VYHRAL! Cerveny se kousl!";
            }

            if (snake2.Skip(1).Contains(newHead2))
            {
                gameOver = true;
                winner = "CERVENY HAD VYHRAL! Zeleny se kousl!";
            }

            if (newHead1 == newHead2)
            {
                gameOver = true;
                winner = "REMIZA! Oba se srazili!";
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
            GraphicsDevice.Clear(desertSand);

            Matrix shakeMatrix = Matrix.CreateTranslation(screenShake.X, screenShake.Y, 0);
            _spriteBatch.Begin(transformMatrix: shakeMatrix);

            // Draw background
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    Color bgColor = (i + j) % 2 == 0 ? desertSand : darkSand;
                    _spriteBatch.Draw(pixelTexture, new Rectangle(i * CellSize, j * CellSize, CellSize, CellSize), bgColor);
                }
            }

            // Draw shadows for snake 1
            foreach (var segment in snake1)
            {
                _spriteBatch.Draw(pixelTexture,
                    new Rectangle(segment.X * CellSize + 3, segment.Y * CellSize + 3, CellSize - 2, CellSize - 2),
                    Color.Black * 0.3f);
            }

            // Draw snake 1
            for (int i = 0; i < snake1.Count; i++)
            {
                Color color = i == 0 ? new Color(180, 80, 20) : snake1Color;
                _spriteBatch.Draw(pixelTexture,
                    new Rectangle(snake1[i].X * CellSize + 1, snake1[i].Y * CellSize + 1, CellSize - 2, CellSize - 2),
                    color);

                // Border
                _spriteBatch.Draw(pixelTexture,
                    new Rectangle(snake1[i].X * CellSize + 1, snake1[i].Y * CellSize + 1, CellSize - 2, 1),
                    Color.Black * 0.5f);
                _spriteBatch.Draw(pixelTexture,
                    new Rectangle(snake1[i].X * CellSize + 1, snake1[i].Y * CellSize + CellSize - 2, CellSize - 2, 1),
                    Color.Black * 0.5f);
                _spriteBatch.Draw(pixelTexture,
                    new Rectangle(snake1[i].X * CellSize + 1, snake1[i].Y * CellSize + 1, 1, CellSize - 2),
                    Color.Black * 0.5f);
                _spriteBatch.Draw(pixelTexture,
                    new Rectangle(snake1[i].X * CellSize + CellSize - 2, snake1[i].Y * CellSize + 1, 1, CellSize - 2),
                    Color.Black * 0.5f);
            }

            // Draw shadows for snake 2
            foreach (var segment in snake2)
            {
                _spriteBatch.Draw(pixelTexture,
                    new Rectangle(segment.X * CellSize + 3, segment.Y * CellSize + 3, CellSize - 2, CellSize - 2),
                    Color.Black * 0.3f);
            }

            // Draw snake 2
            for (int i = 0; i < snake2.Count; i++)
            {
                Color color = i == 0 ? new Color(100, 130, 50) : snake2Color;
                _spriteBatch.Draw(pixelTexture,
                    new Rectangle(snake2[i].X * CellSize + 1, snake2[i].Y * CellSize + 1, CellSize - 2, CellSize - 2),
                    color);

                // Border
                _spriteBatch.Draw(pixelTexture,
                    new Rectangle(snake2[i].X * CellSize + 1, snake2[i].Y * CellSize + 1, CellSize - 2, 1),
                    Color.Black * 0.5f);
                _spriteBatch.Draw(pixelTexture,
                    new Rectangle(snake2[i].X * CellSize + 1, snake2[i].Y * CellSize + CellSize - 2, CellSize - 2, 1),
                    Color.Black * 0.5f);
                _spriteBatch.Draw(pixelTexture,
                    new Rectangle(snake2[i].X * CellSize + 1, snake2[i].Y * CellSize + 1, 1, CellSize - 2),
                    Color.Black * 0.5f);
                _spriteBatch.Draw(pixelTexture,
                    new Rectangle(snake2[i].X * CellSize + CellSize - 2, snake2[i].Y * CellSize + 1, 1, CellSize - 2),
                    Color.Black * 0.5f);
            }

            // Draw food shadow
            _spriteBatch.Draw(pixelTexture,
                new Rectangle(food.X * CellSize + 8, food.Y * CellSize + 8, CellSize - 10, CellSize - 10),
                Color.Black * 0.3f);

            // Draw food
            _spriteBatch.Draw(pixelTexture,
                new Rectangle(food.X * CellSize + 5, food.Y * CellSize + 5, CellSize - 10, CellSize - 10),
                cactusGreen);
            _spriteBatch.Draw(pixelTexture,
                new Rectangle(food.X * CellSize + 7, food.Y * CellSize + 7, CellSize - 14, CellSize - 14),
                new Color(80, 120, 60));

            // Draw bullets
            foreach (var bullet in bullets)
            {
                Color bulletColor = bullet.Shooter == 1 ? new Color(255, 140, 0) : new Color(255, 215, 0);

                _spriteBatch.Draw(pixelTexture,
                    new Rectangle(bullet.Position.X * CellSize + 6, bullet.Position.Y * CellSize + 6, 13, 13),
                    bulletColor * 0.3f);

                _spriteBatch.Draw(pixelTexture,
                    new Rectangle(bullet.Position.X * CellSize + 8, bullet.Position.Y * CellSize + 8, 9, 9),
                    bulletColor);
            }

            // Draw particles
            foreach (var particle in particles)
            {
                float alpha = particle.Life / particle.MaxLife;
                _spriteBatch.Draw(pixelTexture,
                    new Rectangle((int)particle.Position.X, (int)particle.Position.Y, particle.Size, particle.Size),
                    particle.Color * alpha);
            }

            _spriteBatch.End();

            // Draw HUD
            _spriteBatch.Begin();

            if (font != null)
            {
                // Zobraz životy a náboje
                _spriteBatch.DrawString(font, $"Cerveny: {ammo1} naboju | {lives1} zivoty", new Vector2(11, 11), Color.Black * 0.5f);
                _spriteBatch.DrawString(font, $"Cerveny: {ammo1} naboju | {lives1} zivoty", new Vector2(10, 10), new Color(180, 80, 20));

                string greenText = $"Zeleny: {ammo2} naboju | {lives2} zivoty";
                Vector2 greenSize = font.MeasureString(greenText);
                _spriteBatch.DrawString(font, greenText, new Vector2(GridSize * CellSize - greenSize.X - 9, 11), Color.Black * 0.5f);
                _spriteBatch.DrawString(font, greenText, new Vector2(GridSize * CellSize - greenSize.X - 10, 10), new Color(100, 130, 50));

                if (gameOver)
                {
                    _spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, GridSize * CellSize, GridSize * CellSize), Color.Black * 0.8f);

                    Vector2 textSize = font.MeasureString(winner);
                    Vector2 position = new Vector2((GridSize * CellSize - textSize.X) / 2, (GridSize * CellSize - textSize.Y) / 2 - 20);

                    _spriteBatch.DrawString(font, winner, position + new Vector2(2, 2), Color.Black);
                    _spriteBatch.DrawString(font, winner, position, new Color(255, 215, 0));

                    string restartText = "Stiskni R pro restart";
                    Vector2 restartSize = font.MeasureString(restartText);
                    Vector2 restartPos = new Vector2((GridSize * CellSize - restartSize.X) / 2, position.Y + 40);

                    _spriteBatch.DrawString(font, restartText, restartPos + new Vector2(2, 2), Color.Black);
                    _spriteBatch.DrawString(font, restartText, restartPos, Color.White);
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

        public class ParticleEffect
        {
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public Color Color { get; set; }
            public float Life { get; set; }
            public float MaxLife { get; set; }
            public int Size { get; set; }
            public bool IsDead => Life <= 0;

            public void Update()
            {
                Position += Velocity;
                Life -= 0.016f;
                Velocity *= 0.95f;
            }
        }
    }
}