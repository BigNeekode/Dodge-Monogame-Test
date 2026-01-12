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
    private SpriteFont _font;

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
    private readonly TimeScale _timeScale = new();
    private readonly ScreenFlash _screenFlash = new();

    // Shared dependencies
    private readonly Random _rand = new();
    private KeyboardState _prevKeyboard;

    // Sound & tone debug
    private SoundService _soundService;
    private bool _toneDebugMode = false;
    private float _toneFreq = 440f;
    private Waveform _toneWave = Waveform.Sine;
    private bool _toneVibrato = false;
    private bool _toneBitCrush = false;

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
        _soundService = new SoundService();
        // Load sound presets (if available)
        string[] candidates = new[] {
            System.IO.Path.Combine(Content.RootDirectory, "sounds.json"),
            "sounds.json",
            System.IO.Path.Combine(AppContext.BaseDirectory, "Content", "sounds.json"),
            System.IO.Path.Combine(AppContext.BaseDirectory, "sounds.json")
        };
        SoundBank bank = null;
        foreach (var p in candidates)
        {
            if (System.IO.File.Exists(p))
            {
                Console.WriteLine($"Loading sound presets from: {p}");
                bank = SoundBank.LoadFromFile(p);
                break;
            }
        }
        if (bank == null)
        {
            Console.WriteLine("No sound preset file found. Continuing without presets.");
            bank = new SoundBank();
        }
        _soundService.RegisterBank(bank);

        _collisionManager = new CollisionManager(_rand, _particleSystem, _soundService);
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
        
        // Load spritefont
        _font = Content.Load<SpriteFont>("font");
        
        // Initialize renderer
        _renderer = new GameRenderer(_spriteBatch, _pixel, _graphics, _font);
        
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

        // Update time scale
        _timeScale.Update(deltaTime);
        var scaledDelta = _timeScale.GetScaledDelta(deltaTime);
        
        // Update screen flash
        _screenFlash.Update(deltaTime);

        if (_state == GameState.Playing)
        {
            UpdatePlaying(keyboard, scaledDelta);
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
                _soundService?.PlayPreset("chicken_shoot");
                Console.WriteLine("HONK!");
                Console.Out.Flush();
            }
            else
            {
                // Shoot regular bullet
                _bullets.Add(new Bullet(projectileRect, (int)GameConfig.BulletSpeed));
                _soundService?.PlayPreset("gun");
            }
        }

        // Handle dashing
        if (_player.TryDash(keyboard))
        {
            var playerCenter = new Vector2(_player.Rect.Center.X, _player.Rect.Center.Y);
            
            // Spawn dash particles with direction (if juice enabled)
            if (GameConfig.Juice.EnableDashParticles)
            {
                var dashDir = new Vector2(
                    keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D) ? 1 :
                    keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A) ? -1 : 0,
                    keyboard.IsKeyDown(Keys.Up) ? -1 :
                    keyboard.IsKeyDown(Keys.Down) ? 1 : 0
                );
                if (dashDir.LengthSquared() > 0)
                {
                    dashDir.Normalize();
                    _particleSystem.SpawnDashParticles(playerCenter, -dashDir, GameConfig.Juice.DashParticleCount, _rand);
                }
                else
                {
                    _particleSystem.SpawnParticles(playerCenter, 8, Color.Cyan, _rand);
                }
            }
            else
            {
                _particleSystem.SpawnParticles(playerCenter, 8, Color.Cyan, _rand);
            }
            
            _soundService?.PlayPreset("dash");
        }

        // Update spawning (play small sound on spawn)
        _spawnManager.Update(
            deltaTime, 
            _scoreManager.TimeScore,
            obstacle => { _obstacles.Add(obstacle); _soundService?.PlayPreset("spawn_obstacle"); },
            powerUp => { _powerUps.Add(powerUp); _soundService?.PlayPreset("spawn_powerup"); }
        );

        // (inlined entity updates are handled below)

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
            _soundService?.PlayPreset("disco_toggle");
        }

        // Tone debug mode toggle (press T). When active, use Up/Down to change freq, W to cycle waveform,
        // V to toggle vibrato, C to toggle bitcrush, K to play current tone
        if (keyboard.IsKeyDown(Keys.T) && !_prevKeyboard.IsKeyDown(Keys.T))
        {
            _toneDebugMode = !_toneDebugMode;
        }
        if (_toneDebugMode)
        {
            if (keyboard.IsKeyDown(Keys.Up) && !_prevKeyboard.IsKeyDown(Keys.Up)) _toneFreq += 10f;
            if (keyboard.IsKeyDown(Keys.Down) && !_prevKeyboard.IsKeyDown(Keys.Down)) _toneFreq = MathF.Max(20f, _toneFreq - 10f);
            if (keyboard.IsKeyDown(Keys.W) && !_prevKeyboard.IsKeyDown(Keys.W)) _toneWave = (Waveform)(((int)_toneWave + 1) % Enum.GetValues(typeof(Waveform)).Length);
            if (keyboard.IsKeyDown(Keys.V) && !_prevKeyboard.IsKeyDown(Keys.V)) _toneVibrato = !_toneVibrato;
            if (keyboard.IsKeyDown(Keys.C) && !_prevKeyboard.IsKeyDown(Keys.C)) _toneBitCrush = !_toneBitCrush;
            if (keyboard.IsKeyDown(Keys.K) && !_prevKeyboard.IsKeyDown(Keys.K))
            {
                _soundService?.PlayBeep(_toneFreq, 0.25f, 0.7f, _toneWave, _toneVibrato ? 5f : 0f, _toneVibrato ? 0.03f : 0f, new Envelope(0.01f, 0.06f, 0.8f, 0.05f), _toneBitCrush);
            }
        }

            // Quick audition keys (not part of the tone debug)
            if (keyboard.IsKeyDown(Keys.P) && !_prevKeyboard.IsKeyDown(Keys.P)) _soundService?.PlayPreset("powerup");
            if (keyboard.IsKeyDown(Keys.O) && !_prevKeyboard.IsKeyDown(Keys.O)) _soundService?.PlayPreset("coin");
            if (keyboard.IsKeyDown(Keys.I) && !_prevKeyboard.IsKeyDown(Keys.I)) _soundService?.PlayPreset("explosion");
        
        // Update bullets (remove if off-screen, spawn trails)
        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            _bullets[i].Update(deltaTime);
            
            // Spawn bullet trail particles for juice
            if (GameConfig.Juice.EnableEnhancedParticles && _rand.NextDouble() < 0.5)
            {
                _particleSystem.SpawnTrail(
                    new Vector2(_bullets[i].Rect.Center.X, _bullets[i].Rect.Center.Y),
                    Color.White * 0.6f);
            }
            
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

        // Calculate obstacle speed based on score and active power-ups
        var obstacleSpeed = _scoreManager.GetObstacleSpeed(_powerUpManager.SpeedMultiplier);

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
            
            // Juice effects on kill
            if (GameConfig.Juice.EnableSlowMotionOnKill)
            {
                _timeScale.ApplySlowMotion(GameConfig.Juice.SlowMotionTimeScale, GameConfig.Juice.SlowMotionDuration);
            }
            if (GameConfig.Juice.EnableScreenFlash)
            {
                _screenFlash.FlashWhite(GameConfig.Juice.ScreenFlashDuration);
            }
            if (GameConfig.Juice.EnableZoomPunch)
            {
                _renderer.TriggerZoomPunch();
            }
            
            // Enhanced screen shake
            _screenShake.AddTrauma(GameConfig.Juice.KillShakeTrauma);
            
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
                _soundService?.PlayPreset("combo");
            }
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

            // Play appropriate sound
            switch (collectedPowerUp.Value)
            {
                case PowerType.Slow:
                    _soundService?.PlayPreset("powerup");
                    break;
                case PowerType.Shield:
                    _soundService?.PlayPreset("shield");
                    break;
                case PowerType.ExtraLife:
                    _soundService?.PlayPreset("extra_life");
                    break;
                case PowerType.RubberChicken:
                    _soundService?.PlayPreset("rubber_chicken_pick");
                    break;
            }
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
                    _soundService?.PlayPreset("shield_hit");
                },
                onPlayerHit: () =>
                {
                    _screenShake.AddTrauma(0.6f);
                    _comboSystem.Reset(); // Reset combo on hit
                    _soundService?.PlayPreset("player_hit");
                    
                    if (!_player.TakeDamage())
                    {
                        _soundService?.PlayPreset("game_over");
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
        var debugInfo = _toneDebugMode ? $"TONE: {_toneFreq} Hz  Wave:{_toneWave}  Vib:{_toneVibrato}  Crush:{_toneBitCrush}" : null;

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
            _screenFlash,
            _state == GameState.GameOver,
            debugInfo
        );

        base.Draw(gameTime);
    }
}
