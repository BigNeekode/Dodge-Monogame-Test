using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Test_Project.Core;
using Test_Project.Entities;
using Test_Project.Managers;
using Test_Project.Rendering;
using Test_Project.Services;
using Test_Project.Systems;

namespace Test_Project;

/// <summary>
/// Main game orchestrator - delegates to managers for specific responsibilities
/// </summary>
public class Game1 : Game
{
    // Core engine components
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixel;

    // Game state
    private enum GameState { Playing, GameOver }
    private GameState _state = GameState.Playing;

    // Entity collections
    private readonly List<Bullet> _bullets = [];
    private readonly List<Obstacle> _obstacles = [];
    private readonly List<PowerUp> _powerUps = [];
    private readonly List<RubberChicken> _chickens = [];

    // Managers
    private Player _player;
    private SpawnManager _spawnManager;
    private CollisionManager _collisionManager;
    private PowerUpManager _powerUpManager;
    private ScoreManager _scoreManager;
    private GameRenderer _renderer;
    
    // Systems
    private readonly ParticleSystem _particleSystem = new();
    private readonly ComboSystem _comboSystem = new();
    private readonly ScorePopupSystem _scorePopupSystem = new();
    private readonly ScreenShake _screenShake = new();

    // Shared dependencies
    private readonly Random _rand = new();
    private KeyboardState _prevKeyboard;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        _graphics.PreferredBackBufferWidth = GameConfig.WindowWidth;
        _graphics.PreferredBackBufferHeight = GameConfig.WindowHeight;
        
        Console.WriteLine("Game starting...");
        
        Exiting += (sender, args) => Console.WriteLine("Game exiting...");
    }

    protected override void Initialize()
    {
        // Initialize managers
        var highScoreService = new HighScoreService("highscore.txt");
        _scoreManager = new ScoreManager(highScoreService);
        _collisionManager = new CollisionManager(_rand, _particleSystem);
        _powerUpManager = new PowerUpManager();
        _spawnManager = new SpawnManager(_rand, GameConfig.WindowWidth);
        
        // Initialize player
        _player = new Player(
            GameConfig.WindowWidth / 2 - GameConfig.PlayerWidth / 2,
            GameConfig.WindowHeight - 100
        );

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Create 1x1 white pixel texture for rendering
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        
        // Initialize renderer
        _renderer = new GameRenderer(_spriteBatch, _pixel, _graphics);
        
        Console.WriteLine("LoadContent finished");
    }

    /// <summary>
    /// Resets game to initial state
    /// </summary>
    private void ResetGame()
    {
        _state = GameState.Playing;
        
        // Clear all entity collections
        _bullets.Clear();
        _obstacles.Clear();
        _powerUps.Clear();
        _chickens.Clear();
        _particleSystem.Clear();
        _scorePopupSystem.Clear();
        _comboSystem.Reset();
        _screenShake.Reset();
        
        // Reset all managers
        _player.Reset(
            GameConfig.WindowWidth / 2 - GameConfig.PlayerWidth / 2,
            GameConfig.WindowHeight - 100
        );
        _scoreManager.Reset();
        _powerUpManager.Reset();
        _spawnManager.Reset();
        _renderer.Reset();
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Escape))
            Exit();

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_state == GameState.Playing)
        {
            UpdatePlaying(keyboard, deltaTime);
        }
        else if (_state == GameState.GameOver)
        {
            // Restart with R
            if (keyboard.IsKeyDown(Keys.R))
            {
                ResetGame();
            }
        }

        // Update window title with current score
        UpdateWindowTitle();

        _prevKeyboard = keyboard;
        base.Update(gameTime);
    }

    /// <summary>
    /// Updates game logic during playing state
    /// </summary>
    private void UpdatePlaying(KeyboardState keyboard, float deltaTime)
    {
        // Update player input and movement
        _player.HandleInput(keyboard, deltaTime, GameConfig.WindowWidth);
        _player.Update(deltaTime);

        // Handle shooting
        if (_player.TryShoot(keyboard))
        {
            var projectileRect = _player.GetProjectileSpawnRect();
            
            if (_powerUpManager.RubberChickenActive)
            {
                // Launch rubber chicken
                var velocityX = (float)(_rand.NextDouble() * 200 - 100);
                var velocityY = -300f;
                _chickens.Add(new RubberChicken(
                    projectileRect, 
                    new Vector2(velocityX, velocityY), 
                    GameConfig.RubberChickenBounces));
                Console.WriteLine("HONK!");
                Console.Out.Flush();
            }
            else
            {
                // Shoot regular bullet
                _bullets.Add(new Bullet(projectileRect, (int)GameConfig.BulletSpeed));
            }
        }

        // Handle dashing
        if (_player.TryDash(keyboard))
        {
            _particleSystem.SpawnParticles(
                new Vector2(_player.Rect.Center.X, _player.Rect.Center.Y),
                8, Color.Cyan, _rand);
        }

        // Update spawning
        _spawnManager.Update(
            deltaTime, 
            _scoreManager.TimeScore,
            obstacle => _obstacles.Add(obstacle),
            powerUp => _powerUps.Add(powerUp)
        );

        // Update all entities
        UpdateEntities(deltaTime);

        // Handle all collisions
        HandleCollisions();

        // Update power-up timers
        _powerUpManager.Update(deltaTime);

        // Update score and difficulty
        _scoreManager.UpdateTime(deltaTime);

        // Update gameplay systems
        _comboSystem.Update(deltaTime);
        _scorePopupSystem.Update(deltaTime);
        _screenShake.Update(deltaTime);

        // Handle disco mode toggle
        if (keyboard.IsKeyDown(Keys.G) && !_prevKeyboard.IsKeyDown(Keys.G))
        {
            _renderer.ToggleDiscoMode();
        }
        _renderer.Update(deltaTime);
    }

    /// <summary>
    /// Updates all game entities
    /// </summary>
    private void UpdateEntities(float deltaTime)
    {
        var obstacleSpeed = _scoreManager.GetObstacleSpeed(_powerUpManager.SpeedMultiplier);

        // Update bullets (remove if off-screen)
        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            _bullets[i].Update(deltaTime);
            if (_bullets[i].Rect.Y + _bullets[i].Rect.Height < 0)
            {
                _bullets.RemoveAt(i);
            }
        }

        // Update rubber chickens (remove if off-screen)
        for (int i = _chickens.Count - 1; i >= 0; i--)
        {
            _chickens[i].Update(deltaTime);
            if (_chickens[i].Rect.Y > GameConfig.WindowHeight + 200)
            {
                _chickens.RemoveAt(i);
            }
        }

        // Update obstacles (remove if off-screen)
        for (int i = _obstacles.Count - 1; i >= 0; i--)
        {
            var obstacle = _obstacles[i];
            obstacle.Update(deltaTime, obstacleSpeed);
            _obstacles[i] = obstacle;
            
            if (obstacle.Rect.Y > GameConfig.WindowHeight)
            {
                _obstacles.RemoveAt(i);
            }
        }

        // Update power-ups (remove if off-screen)
        for (int i = _powerUps.Count - 1; i >= 0; i--)
        {
            var powerUp = _powerUps[i];
            powerUp.Update(deltaTime, GameConfig.PowerUpSpeed);
            _powerUps[i] = powerUp;
            
            if (powerUp.Rect.Y > GameConfig.WindowHeight)
            {
                _powerUps.RemoveAt(i);
            }
        }

        // Update particles
        _particleSystem.Update(deltaTime);
    }

    /// <summary>
    /// Handles all collision detection and response
    /// </summary>
    private void HandleCollisions()
    {
        // Bullet-obstacle collisions with combo system
        var pointsEarned = _collisionManager.CheckBulletCollisions(_bullets, _obstacles);
        if (pointsEarned > 0)
        {
            // Add to combo
            _comboSystem.AddKill();
            
            // Calculate score with multiplier
            var multiplier = _comboSystem.ComboMultiplier;
            var scoreGained = pointsEarned * multiplier;
            _scoreManager.AddPoints(scoreGained);
            
            // Show combo popup every 5 kills
            if (_comboSystem.ComboCount % 5 == 0 && _comboSystem.ComboCount > 0)
            {
                _scorePopupSystem.SpawnComboPopup(
                    new Vector2(GameConfig.WindowWidth / 2, GameConfig.WindowHeight / 3),
                    _comboSystem.ComboCount);
            }
            
            // Small screen shake on hit
            _screenShake.AddTrauma(0.2f);
        }

        // Chicken-obstacle collisions
        _collisionManager.CheckChickenCollisions(_chickens, _obstacles);

        // Power-up collection
        var collectedPowerUp = _collisionManager.CheckPowerUpCollection(_player.Rect, _powerUps);
        if (collectedPowerUp.HasValue)
        {
            _powerUpManager.ApplyPowerUp(collectedPowerUp.Value, _player);
            _particleSystem.SpawnParticles(
                new Vector2(_player.Rect.Center.X, _player.Rect.Center.Y),
                15, Color.Gold, _rand);
        }

        // Player-obstacle collisions (skip if dashing)
        if (!_player.IsDashing)
        {
            var playerHit = _collisionManager.CheckPlayerCollisions(
                _player.Rect,
                _obstacles,
                _powerUpManager.ShieldActive,
                onShieldHit: () => 
                { 
                    _screenShake.AddTrauma(0.3f);
                },
                onPlayerHit: () =>
                {
                    _screenShake.AddTrauma(0.6f);
                    _comboSystem.Reset(); // Reset combo on hit
                    
                    if (!_player.TakeDamage())
                    {
                        _state = GameState.GameOver;
                        _scoreManager.UpdateHighScore();
                    }
                }
            );
        }
    }

    /// <summary>
    /// Updates the window title with current game state
    /// </summary>
    private void UpdateWindowTitle()
    {
        var totalScore = _scoreManager.GetTotalScore();
        var title = $"Dodge - Time: {(int)_scoreManager.TimeScore}s  Points: {_scoreManager.Points}  Score: {totalScore}  High: {_scoreManager.HighScore}";
        
        if (_comboSystem.HasCombo)
        {
            title += $"  Combo: {_comboSystem.ComboCount}x (x{_comboSystem.ComboMultiplier})";
        }
        
        if (_renderer.IsDiscoActive())
        {
            title += "  - DISCO MODE!";
        }
        
        if (_state == GameState.GameOver)
        {
            title += " - GAME OVER (press R to restart)";
        }
        
        Window.Title = title;
    }

    protected override void Draw(GameTime gameTime)
    {
        _renderer.Draw(
            _player,
            _bullets,
            _obstacles,
            _powerUps,
            _chickens,
            _particleSystem,
            _powerUpManager,
            _comboSystem,
            _scorePopupSystem,
            _screenShake,
            _state == GameState.GameOver
        );

        base.Draw(gameTime);
    }
}
