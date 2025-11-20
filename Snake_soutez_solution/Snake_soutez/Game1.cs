// ========== MAIN GAME CLASS ==========
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct2D1.Effects;
using System.Collections.Generic;

namespace Snake_soutez
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private const int GridSize = 30;
        private const int CellSize = 30;
        private Texture2D pixelTexture;

        private Snake snake1;
        private Snake snake2;
        private Food food;
        private List<Bullet> bullets;
        private GameState gameState;

        private float moveTimer = 0f;
        private float moveInterval = 0.3f;
        private float bulletMoveInterval = 0.05f;
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
            // Initialize game objects
            snake1 = new Snake(1, new Point(5, 15), Direction.Right, Color.DarkRed, Color.Firebrick);
            snake2 = new Snake(2, new Point(24, 15), Direction.Left, Color.DarkBlue, Color.Navy);

            food = new Food();
            bullets = new List<Bullet>();
            gameState = new GameState();

            SpawnFood();

            base.Initialize();
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
            List<Point> occupied = new List<Point>();
            occupied.AddRange(snake1.Body);
            occupied.AddRange(snake2.Body);
            food.Spawn(occupied, GridSize);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState keyState = Keyboard.GetState();

            // Restart game
            if (gameState.IsGameOver && keyState.IsKeyDown(Keys.R) && previousKeyState.IsKeyUp(Keys.R))
            {
                RestartGame();
            }

            if (gameState.IsGameOver)
            {
                previousKeyState = keyState;
                base.Update(gameTime);
                return;
            }

            // Snake 1 controls (WASD)
            HandleSnakeInput(snake1, keyState, Keys.W, Keys.S, Keys.A, Keys.D, Keys.Space);

            // Snake 2 controls (Arrows)
            HandleSnakeInput(snake2, keyState, Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Enter);

            previousKeyState = keyState;

            // Timers
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

        private void HandleSnakeInput(Snake snake, KeyboardState keyState,
            Keys up, Keys down, Keys left, Keys right, Keys shoot)
        {
            if (keyState.IsKeyDown(up) && snake.CurrentDirection != Direction.Down)
                snake.NextDirection = Direction.Up;
            else if (keyState.IsKeyDown(down) && snake.CurrentDirection != Direction.Up)
                snake.NextDirection = Direction.Down;
            else if (keyState.IsKeyDown(left) && snake.CurrentDirection != Direction.Right)
                snake.NextDirection = Direction.Left;
            else if (keyState.IsKeyDown(right) && snake.CurrentDirection != Direction.Left)
                snake.NextDirection = Direction.Right;

            if (keyState.IsKeyDown(shoot) && previousKeyState.IsKeyUp(shoot) && snake.Ammo > 0)
            {
                bullets.Add(new Bullet(snake.Body[0], snake.CurrentDirection, snake.PlayerNumber));
                snake.Ammo--;
            }
        }

        private void MoveBullets()
        {
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i].Move();

                if (bullets[i].IsOutOfBounds(GridSize))
                {
                    bullets.RemoveAt(i);
                    continue;
                }

                // Check bullet collision with snakes
                if (bullets[i].Shooter == 1 && snake2.CheckCollision(bullets[i].Position))
                {
                    snake2.LoseLife();
                    bullets.RemoveAt(i);

                    if (!snake2.IsAlive())
                    {
                        gameState.SetWinner("CERVENY HAD (WASD) VYHRAL!");
                    }
                    continue;
                }
                else if (bullets[i].Shooter == 2 && snake1.CheckCollision(bullets[i].Position))
                {
                    snake1.LoseLife();
                    bullets.RemoveAt(i);

                    if (!snake1.IsAlive())
                    {
                        gameState.SetWinner("MODRY HAD (SIPKY) VYHRAL!");
                    }
                    continue;
                }
            }
        }

        private void GameLoop()
        {
            // Move snakes
            snake1.Move();
            snake2.Move();

            // Check food collision
            if (food.CheckCollision(snake1.Body[0]))
            {
                snake1.Ammo++;
                snake1.Body.Add(snake1.Body[snake1.Body.Count - 1]); // Grow
                SpawnFood();
            }

            if (food.CheckCollision(snake2.Body[0]))
            {
                snake2.Ammo++;
                snake2.Body.Add(snake2.Body[snake2.Body.Count - 1]); // Grow
                SpawnFood();
            }

            // Check wall collisions
            if (snake1.CheckWallCollision(GridSize))
            {
                gameState.SetWinner("MODRY HAD VYHRAL! Cerveny narazil do zdi!");
            }

            if (snake2.CheckWallCollision(GridSize))
            {
                gameState.SetWinner("CERVENY HAD VYHRAL! Modry narazil do zdi!");
            }

            // Check self collision
            if (snake1.CheckSelfCollision())
            {
                gameState.SetWinner("MODRY HAD VYHRAL! Cerveny se kousl!");
            }

            if (snake2.CheckSelfCollision())
            {
                gameState.SetWinner("CERVENY HAD VYHRAL! Modry se kousl!");
            }

            // Check collision with other snake
            if (snake1.CheckCollision(snake2.Body[0]))
            {
                gameState.SetWinner("CERVENY HAD VYHRAL! Modry narazil do cerveneho!");
            }

            if (snake2.CheckCollision(snake1.Body[0]))
            {
                gameState.SetWinner("MODRY HAD VYHRAL! Cerveny narazil do modreho!");
            }

            // Check head-to-head collision
            if (snake1.Body[0] == snake2.Body[0])
            {
                gameState.SetWinner("REMIZA! Oba hadi se srazili!");
            }
        }

        private void RestartGame()
        {
            snake1 = new Snake(1, new Point(5, 15), Direction.Right, Color.DarkRed, Color.Firebrick);
            snake2 = new Snake(2, new Point(24, 15), Direction.Left, Color.DarkBlue, Color.Navy);
            bullets.Clear();
            gameState.Reset();
            SpawnFood();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // Draw background
            _spriteBatch.Draw(pixelTexture,
                new Rectangle(0, 0, GridSize * CellSize, GridSize * CellSize),
                Color.CornflowerBlue);

            // Draw game objects
            snake1.Draw(_spriteBatch, pixelTexture, CellSize);
            snake2.Draw(_spriteBatch, pixelTexture, CellSize);
            food.Draw(_spriteBatch, pixelTexture, CellSize);

            foreach (var bullet in bullets)
            {
                bullet.Draw(_spriteBatch, pixelTexture, CellSize);
            }

            // Draw HUD
            if (font != null)
            {
                _spriteBatch.DrawString(font, $"Cerveny naboje: {snake1.Ammo}",
                    new Vector2(10, 10), Color.Red);
                _spriteBatch.DrawString(font, $"Modry naboje: {snake2.Ammo}",
                    new Vector2(GridSize * CellSize - 200, 10), Color.Blue);

                if (gameState.IsGameOver)
                {
                    _spriteBatch.DrawString(font, $"Cerveny naboje: {snake1.Ammo}",
                        new Vector2(10, 10), Color.Red);
                    _spriteBatch.DrawString(font, $"Cerveny zivoty: {snake1.Lives}",   // NOVÉ!
                        new Vector2(10, 30), Color.Red);                                // NOVÉ!

                    _spriteBatch.DrawString(font, $"Modry naboje: {snake2.Ammo}",
                        new Vector2(GridSize * CellSize - 200, 10), Color.Blue);
                    _spriteBatch.DrawString(font, $"Modry zivoty: {snake2.Lives}",     // NOVÉ!
                        new Vector2(GridSize * CellSize - 200, 30), Color.Blue);        // NOVÉ!
                    Vector2 textSize = font.MeasureString(gameState.Winner);
                    Vector2 position = new Vector2((GridSize * CellSize - textSize.X) / 2,
                        (GridSize * CellSize - textSize.Y) / 2);

                    _spriteBatch.Draw(pixelTexture,
                        new Rectangle(0, 0, GridSize * CellSize, GridSize * CellSize),
                        Color.Black * 0.7f);

                    _spriteBatch.DrawString(font, gameState.Winner, position, Color.White);

                    string restartText = "Stiskni R pro restart";
                    Vector2 restartSize = font.MeasureString(restartText);
                    _spriteBatch.DrawString(font, restartText,
                        new Vector2((GridSize * CellSize - restartSize.X) / 2, position.Y + 40),
                        Color.Yellow);


                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}