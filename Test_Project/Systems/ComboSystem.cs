using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Test_Project.Systems;

/// <summary>
/// Manages combo multipliers for consecutive kills
/// </summary>
public class ComboSystem
{
    private int _comboCount;
    private float _comboTimer;
    private const float ComboWindow = 2.5f; // seconds to maintain combo
    
    public int ComboCount => _comboCount;
    public int ComboMultiplier => 1 + (_comboCount / 5); // +1x every 5 kills
    public bool HasCombo => _comboCount > 0;

    /// <summary>
    /// Registers a kill and extends combo window
    /// </summary>
    public void AddKill()
    {
        _comboCount++;
        _comboTimer = ComboWindow;
    }

    /// <summary>
    /// Updates combo timer and resets if expired
    /// </summary>
    public void Update(float deltaTime)
    {
        if (_comboTimer > 0f)
        {
            _comboTimer -= deltaTime;
            if (_comboTimer <= 0f)
            {
                _comboCount = 0;
            }
        }
    }

    /// <summary>
    /// Gets remaining combo time for UI
    /// </summary>
    public float GetComboTimePercent()
    {
        return _comboTimer / ComboWindow;
    }

    /// <summary>
    /// Resets combo system
    /// </summary>
    public void Reset()
    {
        _comboCount = 0;
        _comboTimer = 0f;
    }
}
