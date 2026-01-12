using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Test_Project.Systems;

/// <summary>
/// Floating score popups for visual feedback
/// </summary>
public class ScorePopup
{
    public Vector2 Position { get; set; }
    public string Text { get; set; }
    public Color Color { get; set; }
    public float Life { get; set; }
    public float MaxLife { get; set; }
    public Vector2 Velocity { get; set; }
    public float Scale { get; set; }
    public bool IsCombo { get; set; }

    public ScorePopup(Vector2 position, string text, Color color, bool isCombo = false)
    {
        Position = position;
        Text = text;
        Color = color;
        Life = 1.5f;
        MaxLife = 1.5f;
        Velocity = new Vector2(0, -50f);
        Scale = 1f;
        IsCombo = isCombo;
    }

    public void Update(float deltaTime)
    {
        Position += Velocity * deltaTime;
        Life -= deltaTime;
        Velocity = new Vector2(Velocity.X, Velocity.Y + 20f * deltaTime); // slight gravity
        
        // Bounce animation for combo popups
        if (IsCombo)
        {
            float t = 1f - (Life / MaxLife);
            if (t < 0.2f)
            {
                // Bounce in: ease out elastic
                float bounce = t / 0.2f;
                Scale = 0.5f + bounce * 1.3f;
                if (bounce > 0.8f)
                {
                    Scale += (float)Math.Sin(bounce * Math.PI * 3) * 0.15f;
                }
            }
            else
            {
                Scale = 1.3f;
            }
        }
        else
        {
            // Simple scale pulse
            float t = Life / MaxLife;
            Scale = 0.7f + (t * 0.3f);
        }
    }
}

/// <summary>
/// Manages floating score popup text
/// </summary>
public class ScorePopupSystem
{
    private readonly List<ScorePopup> _popups = [];

    public IEnumerable<ScorePopup> Popups => _popups;

    /// <summary>
    /// Spawns a new score popup
    /// </summary>
    public void SpawnPopup(Vector2 position, int score, int multiplier)
    {
        var text = multiplier > 1 ? $"+{score} x{multiplier}!" : $"+{score}";
        var color = multiplier > 1 ? Color.Yellow : Color.White;
        _popups.Add(new ScorePopup(position, text, color, false));
    }

    /// <summary>
    /// Spawns a combo text popup
    /// </summary>
    public void SpawnComboPopup(Vector2 position, int combo)
    {
        var text = $"{combo} COMBO!";
        _popups.Add(new ScorePopup(position, text, Color.Orange, true));
    }

    /// <summary>
    /// Updates all popups
    /// </summary>
    public void Update(float deltaTime)
    {
        for (int i = _popups.Count - 1; i >= 0; i--)
        {
            _popups[i].Update(deltaTime);
            if (_popups[i].Life <= 0f)
            {
                _popups.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Clears all popups
    /// </summary>
    public void Clear()
    {
        _popups.Clear();
    }
}
