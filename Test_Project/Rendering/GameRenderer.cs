using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test_Project.Core;
using Test_Project.Entities;
using Test_Project.Managers;
using Test_Project.Systems;

namespace Test_Project.Rendering;

/// <summary>
/// Handles all rendering logic for the game
/// </summary>
public class GameRenderer
{
    private readonly SpriteBatch _spriteBatch;
    private readonly Texture2D _pixel;
    private readonly GraphicsDeviceManager _graphics;
    private readonly Color[] _discoPalette;
    
    private float _discoTimer;
    private bool _discoActive;

    public GameRenderer(SpriteBatch spriteBatch, Texture2D pixel, GraphicsDeviceManager graphics)
    {
        _spriteBatch = spriteBatch;
        _pixel = pixel;
        _graphics = graphics;
        _discoPalette = [
            Color.HotPink, Color.Cyan, Color.Yellow, Color.Lime,
            Color.Magenta, Color.Orange, Color.Purple, Color.DeepSkyBlue
        ];
    }

    /// <summary>
    /// Toggles disco mode on/off
    /// </summary>
    public void ToggleDiscoMode()
    {
        _discoActive = !_discoActive;
    }

    /// <summary>
    /// Gets whether disco mode is currently active
    /// </summary>
    public bool IsDiscoActive() => _discoActive;

    /// <summary>
    /// Updates disco timer
    /// </summary>
    public void Update(float deltaTime)
    {
        if (_discoActive)
        {
            _discoTimer += deltaTime * GameConfig.DiscoSpeedMultiplier;
        }
    }

    /// <summary>
    /// Draws all game elements
    /// </summary>
    public void Draw(
        Player player,
        List<Bullet> bullets,
        List<Obstacle> obstacles,
        List<PowerUp> powerUps,
        List<RubberChicken> chickens,
        ParticleSystem particleSystem,
        PowerUpManager powerUpManager,
        Systems.ComboSystem comboSystem,
        Systems.ScorePopupSystem scorePopupSystem,
        Systems.ScreenShake screenShake,
        bool isGameOver)
    {
        // Calculate disco colors
        var (bg, playerCol, obstacleCol, floorCol) = GetColors(powerUpManager.ShieldActive, player.IsDashing);

        _graphics.GraphicsDevice.Clear(bg);

        // Apply screen shake offset
        var transform = Matrix.CreateTranslation(screenShake.Offset.X, screenShake.Offset.Y, 0);
        _spriteBatch.Begin(transformMatrix: transform);

        // Draw player with dash effect
        var finalPlayerCol = player.IsDashing ? Color.Cyan : playerCol;
        _spriteBatch.Draw(_pixel, player.Rect, finalPlayerCol);

        // Draw bullets
        foreach (var bullet in bullets)
        {
            _spriteBatch.Draw(_pixel, bullet.Rect, Color.White);
        }

        // Draw rubber chickens
        foreach (var chicken in chickens)
        {
            _spriteBatch.Draw(_pixel, chicken.Rect, Color.Peru);
        }

        // Draw obstacles
        foreach (var obstacle in obstacles)
        {
            _spriteBatch.Draw(_pixel, obstacle.Rect, obstacleCol);
        }

        // Draw power-ups
        foreach (var powerUp in powerUps)
        {
            var color = GetPowerUpColor(powerUp.Type);
            _spriteBatch.Draw(_pixel, powerUp.Rect, color);
        }

        // Draw particles
        foreach (var particle in particleSystem.Particles)
        {
            var alpha = Math.Clamp(particle.Life, 0f, 1f);
            _spriteBatch.Draw(_pixel, 
                new Rectangle((int)particle.Pos.X, (int)particle.Pos.Y, 3, 3), 
                particle.Col * alpha);
        }

        // Draw floor line
        var floorRect = new Rectangle(0, 
            _graphics.PreferredBackBufferHeight - 6, 
            _graphics.PreferredBackBufferWidth, 6);
        _spriteBatch.Draw(_pixel, floorRect, floorCol);

        // Draw score popups (simple rectangles as text substitute)
        foreach (var popup in scorePopupSystem.Popups)
        {
            var alpha = Math.Clamp(popup.Life / 1.5f, 0f, 1f);
            var size = popup.Text.Contains("COMBO") ? 40 : 20;
            var width = popup.Text.Length * 6;
            var rect = new Rectangle((int)popup.Position.X - width/2, (int)popup.Position.Y, width, size);
            _spriteBatch.Draw(_pixel, rect, popup.Color * alpha);
        }

        // Draw HUD
        DrawHUD(player, powerUpManager, comboSystem);

        // Draw game over overlay
        if (isGameOver)
        {
            DrawGameOverOverlay();
        }

        _spriteBatch.End();
    }

    /// <summary>
    /// Draws the HUD (lives, shield bar, combo, dash)
    /// </summary>
    private void DrawHUD(Player player, PowerUpManager powerUpManager, Systems.ComboSystem comboSystem)
    {
        // Draw lives as hearts
        for (int i = 0; i < GameConfig.MaxLives; i++)
        {
            var rect = new Rectangle(10 + i * 18, 10, 14, 14);
            var color = i < player.Lives ? Color.LimeGreen : Color.Gray;
            _spriteBatch.Draw(_pixel, rect, color);
        }

        // Draw shield indicator
        if (powerUpManager.ShieldActive)
        {
            var shieldTimer = powerUpManager.GetShieldTimer();
            var shieldBar = new Rectangle(10, 34, 
                (int)(Math.Max(0, shieldTimer) / GameConfig.ShieldDuration * 120), 8);
            _spriteBatch.Draw(_pixel, new Rectangle(10, 34, 120, 8), Color.DarkSlateGray);
            _spriteBatch.Draw(_pixel, shieldBar, Color.LightSkyBlue);
        }

        // Draw combo meter
        if (comboSystem.HasCombo)
        {
            var comboPercent = comboSystem.GetComboTimePercent();
            var comboBar = new Rectangle(10, 48, (int)(comboPercent * 120), 6);
            _spriteBatch.Draw(_pixel, new Rectangle(10, 48, 120, 6), Color.DarkGray);
            _spriteBatch.Draw(_pixel, comboBar, Color.Orange);
            
            // Combo count indicator
            var comboBox = new Rectangle(135, 48, 12, 6);
            _spriteBatch.Draw(_pixel, comboBox, Color.Orange);
        }

        // Draw dash cooldown
        var dashPercent = player.GetDashCooldownPercent();
        var dashBar = new Rectangle(10, 60, (int)(dashPercent * 120), 6);
        _spriteBatch.Draw(_pixel, new Rectangle(10, 60, 120, 6), Color.DarkSlateGray);
        _spriteBatch.Draw(_pixel, dashBar, Color.Cyan);
    }

    /// <summary>
    /// Draws the game over overlay
    /// </summary>
    private void DrawGameOverOverlay()
    {
        var overlay = new Rectangle(0, 0, 
            _graphics.PreferredBackBufferWidth, 
            _graphics.PreferredBackBufferHeight);
        _spriteBatch.Draw(_pixel, overlay, Color.Black * 0.5f);
        
        var box = new Rectangle(
            _graphics.PreferredBackBufferWidth / 2 - 160, 
            _graphics.PreferredBackBufferHeight / 2 - 60, 
            320, 120);
        _spriteBatch.Draw(_pixel, box, Color.DarkRed * 0.9f);
        
        // Simple text substitute: draw white boxes
        _spriteBatch.Draw(_pixel, 
            new Rectangle(box.X + 10, box.Y + 12, box.Width - 20, 18), 
            Color.White);
        _spriteBatch.Draw(_pixel, 
            new Rectangle(box.X + 10, box.Y + 36, box.Width - 20, 18), 
            Color.White);
    }

    /// <summary>
    /// Gets color scheme based on disco mode
    /// </summary>
    private (Color bg, Color player, Color obstacle, Color floor) GetColors(bool shieldActive, bool isDashing)
    {
        if (_discoActive)
        {
            var idx = (int)(_discoTimer * 6) % _discoPalette.Length;
            return (
                _discoPalette[idx],
                _discoPalette[(idx + 2) % _discoPalette.Length],
                _discoPalette[(idx + 4) % _discoPalette.Length],
                _discoPalette[(idx + 6) % _discoPalette.Length]
            );
        }

        var playerColor = isDashing ? Color.Cyan : (shieldActive ? Color.LightSkyBlue : Color.LimeGreen);
        
        return (
            Color.CornflowerBlue,
            playerColor,
            Color.IndianRed,
            Color.DarkSlateGray
        );
    }

    /// <summary>
    /// Gets the color for a power-up type
    /// </summary>
    private static Color GetPowerUpColor(PowerType type) => type switch
    {
        PowerType.Slow => Color.CornflowerBlue,
        PowerType.Shield => Color.Gold,
        PowerType.ExtraLife => Color.MediumPurple,
        PowerType.RubberChicken => Color.Peru,
        _ => Color.White
    };

    /// <summary>
    /// Resets disco mode state
    /// </summary>
    public void Reset()
    {
        _discoActive = false;
        _discoTimer = 0f;
    }
}
