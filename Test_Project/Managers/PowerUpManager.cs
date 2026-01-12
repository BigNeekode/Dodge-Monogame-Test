using Test_Project.Core;
using Test_Project.Entities;

namespace Test_Project.Managers;

/// <summary>
/// Manages power-up state and active effects
/// </summary>
public class PowerUpManager
{
    public bool ShieldActive { get; private set; }
    public bool ShotgunActive { get; private set; }
    public float SpeedMultiplier { get; private set; }
    
    private float _shieldTimer;
    private float _slowTimer;
    private float _shotgunTimer;

    public PowerUpManager()
    {
        SpeedMultiplier = 1f;
    }

    /// <summary>
    /// Applies a power-up effect
    /// </summary>
    public void ApplyPowerUp(PowerType type, Player player)
    {
        switch (type)
        {
            case PowerType.Slow:
                _slowTimer = GameConfig.SlowDuration;
                SpeedMultiplier = GameConfig.SlowMultiplier;
                break;
            case PowerType.Shield:
                ShieldActive = true;
                _shieldTimer = GameConfig.ShieldDuration;
                break;
            case PowerType.ExtraLife:
                player.AddLife();
                break;
            case PowerType.Shotgun:
                ShotgunActive = true;
                _shotgunTimer = GameConfig.ShotgunDuration;
                break;
        }
    }

    /// <summary>
    /// Updates all active power-up timers
    /// </summary>
    public void Update(float deltaTime)
    {
        // Shield timer
        if (ShieldActive)
        {
            _shieldTimer -= deltaTime;
            if (_shieldTimer <= 0f)
            {
                ShieldActive = false;
            }
        }

        // Slow timer
        if (_slowTimer > 0f)
        {
            _slowTimer -= deltaTime;
            if (_slowTimer <= 0f)
            {
                SpeedMultiplier = 1f;
            }
        }

        // Shotgun timer
        if (ShotgunActive)
        {
            _shotgunTimer -= deltaTime;
            if (_shotgunTimer <= 0f)
            {
                ShotgunActive = false;
            }
        }
    }

    /// <summary>
    /// Gets the remaining shield time for UI display
    /// </summary>
    public float GetShieldTimer() => _shieldTimer;

    /// <summary>
    /// Resets all power-up states
    /// </summary>
    public void Reset()
    {
        ShieldActive = false;
        ShotgunActive = false;
        SpeedMultiplier = 1f;
        _shieldTimer = 0f;
        _slowTimer = 0f;
        _shotgunTimer = 0f;
    }
}
