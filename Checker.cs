using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Checker
{
    public class Checker : Game
    {
        const int _TILESIZE = 75;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        enum PlayerTurn
        {
            RedTurn,
            YellowTurn
        }

        PlayerTurn _currentPlayerTurn;

        Point _selectedTile;

        // TODO: Game State Machine
        enum GameState
        {
            TurnBeginning,
            WaitingForSelection,
            ChipSelected,
            TurnEnded,
            GameEnded
        }

        GameState _currentGameState;

        MouseState _mouseState, _previousMouseState;

        Point _clickedPos;

        List<Point> _possibleClicked;

        // TODO: Image files and font
        Texture2D _chip, _horse, _rect;

        int[,] _gameTable;

        public Checker()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 600;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.ApplyChanges();

            _currentPlayerTurn = PlayerTurn.RedTurn;

            _currentGameState = GameState.TurnBeginning;

            _gameTable = new int[8, 8]
            {
                { 1,0,1,0,1,0,1,0},
                { 0,1,0,1,0,1,0,1},
                { 0,0,0,0,0,0,0,0},
                { 0,0,0,0,0,0,0,0},
                { 0,0,0,0,0,0,0,0},
                { 0,0,0,0,0,0,0,0},
                { -1,0,-1,0,-1,0,-1,0},
                { 0,-1,0,-1,0,-1,0,-1}
            };

            _possibleClicked = new List<Point>();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _chip = this.Content.Load<Texture2D>("Chip");
            _horse = this.Content.Load<Texture2D>("Horse");
            _rect = new Texture2D(_graphics.GraphicsDevice, _TILESIZE, _TILESIZE);

            Color[] data = new Color[_TILESIZE * _TILESIZE];
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.White;

            _rect.SetData(data);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _previousMouseState = _mouseState;

            // TODO: Add your update logic here

            switch (_currentGameState)
            {
                case GameState.TurnBeginning:
                    // Search for available move
                    _possibleClicked.Clear();
                    // Find Beatable Move
                    // Find Possible Move
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (_currentPlayerTurn == PlayerTurn.RedTurn)
                            {
                                // Red Turn
                                if (_gameTable[j, i] < 0)
                                {
                                    // Find Possible Move based on current position
                                    if (FindPossibleMoves(new Point(i, j)).Count > 0)
                                    {
                                        _possibleClicked.Add(new Point(i, j));
                                    }
                                }
                            }
                            else
                            {
                                // Yellow Turn
                                if (_gameTable[j, i] > 0)
                                {
                                    // Find Possible Move based on current position
                                    if (FindPossibleMoves(new Point(i, j)).Count > 0)
                                    {
                                        _possibleClicked.Add(new Point(i, j));
                                    }
                                }
                            }
                        }
                    }
                    // If no move available, this player lose
                    if (_possibleClicked.Count == 0) _currentGameState = GameState.GameEnded;
                    else _currentGameState = GameState.WaitingForSelection;
                    break;
                case GameState.WaitingForSelection:
                    _mouseState = Mouse.GetState();

                    if (_mouseState.LeftButton == ButtonState.Pressed &&
                        _previousMouseState.LeftButton == ButtonState.Released)
                    {
                        int xPos = _mouseState.X / _TILESIZE;
                        int yPos = _mouseState.Y / _TILESIZE;

                        if (_possibleClicked.Contains(new Point(xPos, yPos)))
                        {
                            _selectedTile = new Point(xPos, yPos);

                            _possibleClicked.Clear();

                            _possibleClicked.AddRange(FindPossibleMoves(_selectedTile));

                            _currentGameState = GameState.ChipSelected;
                        }
                    }
                    break;
                case GameState.ChipSelected:
                    _mouseState = Mouse.GetState();

                    if (_mouseState.LeftButton == ButtonState.Pressed &&
                        _previousMouseState.LeftButton == ButtonState.Released)
                    {
                        int xPos = _mouseState.X / _TILESIZE;
                        int yPos = _mouseState.Y / _TILESIZE;

                        _clickedPos = new Point(xPos, yPos);

                        if (_possibleClicked.Contains(_clickedPos))
                        {
                            _gameTable[yPos, xPos] = _gameTable[_selectedTile.Y, _selectedTile.X];
                            _gameTable[_selectedTile.Y, _selectedTile.X] = 0;

                            _currentGameState = GameState.TurnEnded;
                        }
                        else
                        {
                            _possibleClicked.Clear();
                            _currentGameState = GameState.TurnBeginning;
                        }
                    }
                    break;
                case GameState.TurnEnded:
                    if (_currentPlayerTurn == PlayerTurn.RedTurn)
                    {
                        if (_clickedPos.Y == 0) _gameTable[_clickedPos.Y, _clickedPos.X] = -2;
                    }
                    else
                    {
                        if (_clickedPos.Y == 7) _gameTable[_clickedPos.Y, _clickedPos.X] = 2;
                    }

                    if (_currentPlayerTurn == PlayerTurn.RedTurn) _currentPlayerTurn = PlayerTurn.YellowTurn;
                    else _currentPlayerTurn = PlayerTurn.RedTurn;

                    _currentGameState = GameState.TurnBeginning;
                    break;
                case GameState.GameEnded:
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            // TODO: Draw board
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i + j) % 2 == 0)
                        _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }

            // TODO: Draw chips

            switch (_currentGameState)
            {
                case GameState.TurnBeginning:
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            switch (_gameTable[i, j])
                            {
                                case 1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case -1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case 2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case -2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                            }
                        }
                    }
                    break;
                case GameState.WaitingForSelection:
                    // Draw possible clicked
                    foreach (Point p in _possibleClicked)
                    {
                        _spriteBatch.Draw(_rect, new Vector2(p.X * _TILESIZE, p.Y * _TILESIZE), null, Color.DarkGreen, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            switch (_gameTable[i, j])
                            {
                                case 1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case -1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case 2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case -2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                            }
                        }
                    }
                    break;
                case GameState.ChipSelected:
                    _spriteBatch.Draw(_rect, new Vector2(_selectedTile.X * _TILESIZE, _selectedTile.Y * _TILESIZE), null, Color.Blue, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                    foreach (Point p in _possibleClicked)
                    {
                        _spriteBatch.Draw(_rect, new Vector2(p.X * _TILESIZE, p.Y * _TILESIZE), null, Color.LimeGreen, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            switch (_gameTable[i, j])
                            {
                                case 1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case -1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case 2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case -2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                            }
                        }
                    }
                    break;
                case GameState.TurnEnded:
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            switch (_gameTable[i, j])
                            {
                                case 1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case -1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case 2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case -2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                            }
                        }
                    }
                    break;
                case GameState.GameEnded:
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            switch (_gameTable[i, j])
                            {
                                case 1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case -1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case 2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                                case -2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                                    break;
                            }
                        }
                    }
                    break;
            }

            _spriteBatch.End();

            _graphics.BeginDraw();

            base.Draw(gameTime);
        }

        protected List<Point> FindPossibleMoves(Point currentTile)
        {
            List<Point> returnPoints = new List<Point>();

            if (_gameTable[currentTile.Y, currentTile.X] == -1)
            {
                // red normal chip

                // check up left
                if (currentTile.X - 1 >= 0 && currentTile.Y - 1 >= 0)
                {
                    if (_gameTable[currentTile.Y - 1, currentTile.X - 1] == 0)
                    {
                        returnPoints.Add(new Point(currentTile.X - 1, currentTile.Y - 1));
                    }
                }

                // check up right
                if (currentTile.X + 1 < 8 && currentTile.Y - 1 >= 0)
                {
                    if (_gameTable[currentTile.Y - 1, currentTile.X + 1] == 0)
                    {
                        returnPoints.Add(new Point(currentTile.X + 1, currentTile.Y - 1));
                    }
                }
            }
            else if (_gameTable[currentTile.Y, currentTile.X] == 1)
            {
                // yellow normal chip

                // check down left
                if (currentTile.X - 1 >= 0 && currentTile.Y + 1 < 8)
                {
                    if (_gameTable[currentTile.Y + 1, currentTile.X - 1] == 0)
                    {
                        returnPoints.Add(new Point(currentTile.X - 1, currentTile.Y + 1));
                    }
                }

                // check down right
                if (currentTile.X + 1 < 8 && currentTile.Y + 1 < 8)
                {
                    if (_gameTable[currentTile.Y + 1, currentTile.X + 1] == 0)
                    {
                        returnPoints.Add(new Point(currentTile.X + 1, currentTile.Y + 1));
                    }
                }
            }

            return returnPoints;
        }
    }
}
