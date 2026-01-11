using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Test_Project.Core;
using Test_Project.Entities;

namespace Test_Project.Managers;

/// <summary>
/// Manages player state, input, and shooting behavior
/// </summary>
public class Player
{
    public Rectangle Rect { get; set; }
    public int Lives { get; set; }
    public bool IsDashing { get; private set; }
    
    private float _shootTimer;
    private readonly float _shootCooldown;
    private readonly float _speed;
    private float _dashTimer;
    private float _dashCooldown;
    private int _dashDirection;

    public Player(int x, int y)
    {
        Rect = new Rectangle(x, y, GameConfig.PlayerWidth, GameConfig.PlayerHeight);
        Lives = GameConfig.MaxLives;
        _shootCooldown = GameConfig.ShootCooldown;
        _speed = GameConfig.PlayerSpeed;
        _shootTimer = 0f;
        _dashCooldown = 0f;
        _dashTimer = 0f;
        IsDashing = false;
    }

    /// <summary>
    /// Handles player input for movement
    /// </summary>
    public void HandleInput(KeyboardState keyboard, float deltaTime, int screenWidth)
    {
        var move = 0f;
        if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A)) move -= 1f;
        if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D)) move += 1f;
        
        var speed = IsDashing ? GameConfig.DashSpeed : _speed;
        if (IsDashing) move = _dashDirection;
        
        Rect = new Rectangle(
            Rect.X + (int)(move * speed * deltaTime),
            Rect.Y,
            Rect.Width,
            Rect.Height
        );
        
        // Clamp to screen bounds
        Rect = new Rectangle(
            Math.Clamp(Rect.X, 0, screenWidth - Rect.Width),
            Rect.Y,
            Rect.Width,
            Rect.Height
        );
    }

    /// <summary>
    /// Updates shoot cooldown and dash timers
    /// </summary>
    public void Update(float deltaTime)
    {
        _shootTimer -= deltaTime;
        _dashCooldown -= deltaTime;
        
        if (IsDashing)
        {
            _dashTimer -= deltaTime;
            if (_dashTimer <= 0f)
            {
                IsDashing = false;
            }
        }
    }

    /// <summary>
    /// Attempts to shoot, returns true if shoot was successful
    /// </summary>
    public bool TryShoot(KeyboardState keyboard)
    {
        if ((keyboard.IsKeyDown(Keys.Space) || keyboard.IsKeyDown(Keys.Up)) && _shootTimer <= 0f)
        {
            _shootTimer = _shootCooldown;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempts to dash, returns true if dash was triggered
    /// </summary>
    public bool TryDash(KeyboardState keyboard)
    {
        if ((keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift)) && _dashCooldown <= 0f && !IsDashing)
        {
            // Determine dash direction from current input
            var dir = 0;
            if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A)) dir = -1;
            else if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D)) dir = 1;
            
            if (dir != 0)
            {
                _dashDirection = dir;
                IsDashing = true;
                _dashTimer = GameConfig.DashDuration;
                _dashCooldown = GameConfig.DashCooldown;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the spawn position for bullets/chickens
    /// </summary>
    public Rectangle GetProjectileSpawnRect()
    {
        return new Rectangle(Rect.X + Rect.Width / 2 - 3, Rect.Y - 12, 6, 12);
    }

    /// <summary>
    /// Reduces lives and returns true if player is still alive
    /// </summary>
    public bool TakeDamage()
    {
        Lives--;
        return Lives > 0;
    }

    /// <summary>
    /// Adds a life up to the maximum
    /// </summary>
    public void AddLife()
    {
        Lives = Math.Min(Lives + 1, GameConfig.MaxLives);
    }

    /// <summary>
    /// Resets player to initial state
    /// </summary>
    public void Reset(int x, int y)
    {
        Rect = new Rectangle(x, y, GameConfig.PlayerWidth, GameConfig.PlayerHeight);
        Lives = GameConfig.MaxLives;
        _shootTimer = 0f;
        _dashCooldown = 0f;
        _dashTimer = 0f;
        IsDashing = false;
    }

    /// <summary>
    /// Gets dash cooldown percentage for UI
    /// </summary>
    public float GetDashCooldownPercent()
    {
        return Math.Clamp(1f - (_dashCooldown / GameConfig.DashCooldown), 0f, 1f);
    }
}
