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
    private readonly SpriteFont _font;
    private readonly Color[] _discoPalette;
    
    private float _discoTimer;
    private bool _discoActive;
    
    // Zoom punch effect
    private float _zoomPunch;
    private float _zoomPunchTimer;

    public GameRenderer(SpriteBatch spriteBatch, Texture2D pixel, GraphicsDeviceManager graphics, SpriteFont font)
    {
        _spriteBatch = spriteBatch;
        _pixel = pixel;
        _graphics = graphics;
        _font = font;
        // Softer, more muted colors for epilepsy safety
        _discoPalette = [
            new Color(255, 182, 193),  // Light Pink
            new Color(135, 206, 235),  // Light Sky Blue
            new Color(255, 255, 153),  // Light Yellow
            new Color(144, 238, 144),  // Light Green
            new Color(221, 160, 221),  // Plum
            new Color(255, 218, 185),  // Peach Puff
            new Color(176, 196, 222),  // Light Steel Blue
            new Color(173, 216, 230)   // Light Blue
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
        
        // Update zoom punch
        if (_zoomPunchTimer > 0)
        {
            _zoomPunchTimer -= deltaTime;
            float t = _zoomPunchTimer / GameConfig.Juice.ZoomPunchDuration;
            _zoomPunch = t * t * GameConfig.Juice.ZoomPunchAmount; // Ease out quad
        }
        else
        {
            _zoomPunch = 0;
        }
    }

    /// <summary>
    /// Trigger zoom punch effect
    /// </summary>
    public void TriggerZoomPunch()
    {
        if (GameConfig.Juice.EnableZoomPunch)
        {
            _zoomPunch = GameConfig.Juice.ZoomPunchAmount;
            _zoomPunchTimer = GameConfig.Juice.ZoomPunchDuration;
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
        ParticleSystem particleSystem,
        PowerUpManager powerUpManager,
        Systems.ComboSystem comboSystem,
        Systems.ScorePopupSystem scorePopupSystem,
        Systems.ScreenShake screenShake,
        Systems.ScreenFlash screenFlash,
        bool isGameOver, 
        string debugInfo = null)
    {
        // Calculate disco colors
        var (bg, playerCol, obstacleCol, floorCol) = GetColors(powerUpManager.ShieldActive, player.IsDashing);

        _graphics.GraphicsDevice.Clear(bg);

        // Apply screen shake offset + zoom punch
        float zoom = 1f + _zoomPunch;
        var transform = Matrix.CreateScale(zoom) * Matrix.CreateTranslation(screenShake.Offset.X, screenShake.Offset.Y, 0);
        _spriteBatch.Begin(transformMatrix: transform);

        // Draw player with dash effect
        var finalPlayerCol = player.IsDashing ? Color.Cyan : playerCol;
        _spriteBatch.Draw(_pixel, player.Rect, finalPlayerCol);

        // Draw bullets
        foreach (var bullet in bullets)
        {
            _spriteBatch.Draw(_pixel, bullet.Rect, Color.White);
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

        // Draw score popups with actual text
        foreach (var popup in scorePopupSystem.Popups)
        {
            var alpha = Math.Clamp(popup.Life / 1.5f, 0f, 1f);
            var textMeasure = _font.MeasureString(popup.Text);
            var origin = textMeasure / 2f;
            
            // Apply scale animation (especially for combo popups)
            float animScale = GameConfig.Juice.EnablePopupAnimation ? popup.Scale : 1f;
            var baseScale = popup.Text.Contains("COMBO") ? GameConfig.FontScaleCombo : GameConfig.FontScaleLarge;
            var finalScale = baseScale * animScale;
            
            _spriteBatch.DrawString(_font, popup.Text, popup.Position, popup.Color * alpha, 0f, origin, finalScale, SpriteEffects.None, 0f);
        }

        // Draw HUD
        DrawHUD(player, powerUpManager, comboSystem);

        // Draw instructions (only when not game over)
        if (!isGameOver)
        {
            DrawInstructions();
        }

        // Draw game over overlay
        if (isGameOver)
        {
            DrawGameOverOverlay();
        }

        // Draw debug info (top-right)
        if (!string.IsNullOrEmpty(debugInfo))
        {
            var size = _font.MeasureString(debugInfo);
            var pos = new Vector2(_graphics.PreferredBackBufferWidth - 10 - size.X, 10);
            _spriteBatch.DrawString(_font, debugInfo, pos, Color.White, 0f, Vector2.Zero, GameConfig.FontScaleSmall, SpriteEffects.None, 0f);
        }

        _spriteBatch.End();        
        // Draw screen flash (last, on top of everything)
        if (screenFlash.IsFlashing)
        {
            _spriteBatch.Begin();
            screenFlash.Draw(_spriteBatch, _pixel, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _spriteBatch.End();
        }    }

    /// <summary>
    /// Draws the HUD (lives, shield bar, combo, dash, power-ups)
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

        // Draw shield indicator with text
        if (powerUpManager.ShieldActive)
        {
            var shieldTimer = powerUpManager.GetShieldTimer();
            var shieldBar = new Rectangle(10, 34, 
                (int)(Math.Max(0, shieldTimer) / GameConfig.ShieldDuration * 120), 8);
            _spriteBatch.Draw(_pixel, new Rectangle(10, 34, 120, 8), Color.DarkSlateGray);
            _spriteBatch.Draw(_pixel, shieldBar, Color.LightSkyBlue);
            
            var shieldText = $"SHIELD: {shieldTimer:F1}s";
            _spriteBatch.DrawString(_font, shieldText, new Vector2(135, 30), Color.LightSkyBlue, 0f, Vector2.Zero, GameConfig.FontScaleMedium, SpriteEffects.None, 0f);
        }

        // Draw combo meter with text
        if (comboSystem.HasCombo)
        {
            var comboPercent = comboSystem.GetComboTimePercent();
            var comboBar = new Rectangle(10, 48, (int)(comboPercent * 120), 6);
            _spriteBatch.Draw(_pixel, new Rectangle(10, 48, 120, 6), Color.DarkGray);
            _spriteBatch.Draw(_pixel, comboBar, Color.Orange);
            
            var comboText = $"COMBO: {comboSystem.ComboCount}x (x{comboSystem.ComboMultiplier})";
            _spriteBatch.DrawString(_font, comboText, new Vector2(135, 44), Color.Orange, 0f, Vector2.Zero, GameConfig.FontScaleMedium, SpriteEffects.None, 0f);
        }

        // Draw dash cooldown with text
        var dashPercent = player.GetDashCooldownPercent();
        var dashBar = new Rectangle(10, 60, (int)(dashPercent * 120), 6);
        _spriteBatch.Draw(_pixel, new Rectangle(10, 60, 120, 6), Color.DarkSlateGray);
        _spriteBatch.Draw(_pixel, dashBar, Color.Cyan);
        
        var dashText = dashPercent >= 1f ? "DASH READY" : $"DASH: {dashPercent:P0}";
        var dashColor = dashPercent >= 1f ? Color.Cyan : Color.Gray;
        _spriteBatch.DrawString(_font, dashText, new Vector2(135, 56), dashColor, 0f, Vector2.Zero, GameConfig.FontScaleMedium, SpriteEffects.None, 0f);

        // Draw active power-up status
        var powerUpY = 72f;
        if (powerUpManager.ShotgunActive)
        {
            _spriteBatch.DrawString(_font, "SHOTGUN ACTIVE!", new Vector2(10, powerUpY), Color.OrangeRed, 0f, Vector2.Zero, GameConfig.FontScaleLarge, SpriteEffects.None, 0f);
            powerUpY += 16f;
        }
        if (powerUpManager.SpeedMultiplier < 1f)
        {
            _spriteBatch.DrawString(_font, "SLOW MOTION", new Vector2(10, powerUpY), Color.CornflowerBlue, 0f, Vector2.Zero, GameConfig.FontScaleLarge, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Draws game instructions at the bottom of the screen
    /// </summary>
    private void DrawInstructions()
    {
        var yPos = _graphics.PreferredBackBufferHeight - 20;
        var controls = "Arrows/A+D: Move | Space/Up: Shoot | Shift+Dir: Dash | G: Disco | Esc: Quit";
        var size = _font.MeasureString(controls);
        var pos = new Vector2(_graphics.PreferredBackBufferWidth / 2 - size.X / 2, yPos);
        _spriteBatch.DrawString(_font, controls, pos, Color.White * 0.7f, 0f, Vector2.Zero, GameConfig.FontScaleSmall, SpriteEffects.None, 0f);
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
        
        // Draw text
        var gameOverText = "GAME OVER";
        var restartText = "Press R to Restart";
        
        var gameOverSize = _font.MeasureString(gameOverText);
        var restartSize = _font.MeasureString(restartText);
        
        var gameOverPos = new Vector2(
            box.X + box.Width / 2 - gameOverSize.X / 2,
            box.Y + 15
        );
        
        var restartPos = new Vector2(
            box.X + box.Width / 2 - restartSize.X / 2,
            box.Y + 50
        );
        
        _spriteBatch.DrawString(_font, gameOverText, gameOverPos, Color.White, 0f, Vector2.Zero, GameConfig.FontScaleCombo, SpriteEffects.None, 0f);
        _spriteBatch.DrawString(_font, restartText, restartPos, Color.White, 0f, Vector2.Zero, GameConfig.FontScaleMedium, SpriteEffects.None, 0f);
    }

    /// <summary>
    /// Gets color scheme based on disco mode
    /// </summary>
    private (Color bg, Color player, Color obstacle, Color floor) GetColors(bool shieldActive, bool isDashing)
    {
        if (_discoActive)
        {
            // Use smooth color transitions instead of instant jumps
            var smoothTimer = _discoTimer * GameConfig.DiscoColorTransitionSpeed;
            var idx = (int)smoothTimer % _discoPalette.Length;
            var nextIdx = (idx + 1) % _discoPalette.Length;
            var blend = smoothTimer - (int)smoothTimer; // 0 to 1 for smooth blending
            
            // Blend between current and next color for smoothness
            var bg = Color.Lerp(_discoPalette[idx], _discoPalette[nextIdx], blend);
            var player = Color.Lerp(_discoPalette[(idx + 2) % _discoPalette.Length], _discoPalette[(idx + 3) % _discoPalette.Length], blend);
            var obstacle = Color.Lerp(_discoPalette[(idx + 4) % _discoPalette.Length], _discoPalette[(idx + 5) % _discoPalette.Length], blend);
            var floor = Color.Lerp(_discoPalette[(idx + 6) % _discoPalette.Length], _discoPalette[(idx + 7) % _discoPalette.Length], blend);
            
            // Reduce brightness/saturation for epilepsy safety
            bg = Color.Lerp(bg, Color.Gray, 0.3f);
            player = Color.Lerp(player, Color.Gray, 0.2f);
            obstacle = Color.Lerp(obstacle, Color.Gray, 0.2f);
            floor = Color.Lerp(floor, Color.Gray, 0.3f);
            
            return (bg, player, obstacle, floor);
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
        PowerType.Shotgun => Color.OrangeRed,
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
