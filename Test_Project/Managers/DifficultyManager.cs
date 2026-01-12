using System;
using Test_Project.Core;

namespace Test_Project.Managers;

/// <summary>
/// Centralizes dynamic difficulty multipliers (spawn rate, obstacle speed, mutators)
/// </summary>
public class DifficultyManager
{
    private float _spawnMultiplier = 1f;
    private float _speedMultiplier = 1f;

    // Manual runtime adjustments (for Dev HUD)
    public float ManualSpawnAdjust { get; private set; } = 1f;
    public float ManualSpeedAdjust { get; private set; } = 1f;

    public float SpawnMultiplier => _spawnMultiplier * ManualSpawnAdjust;
    public float SpeedMultiplier => _speedMultiplier * ManualSpeedAdjust;

    public DifficultyManager()
    {
        _spawnMultiplier = 1f;
        _speedMultiplier = 1f;
    }

    /// <summary>
    /// Update difficulty based on elapsed time (seconds) and time-based score
    /// </summary>
    public void Update(float deltaTime, float timeScore)
    {
        // scale per minute
        var minutes = timeScore / 60f;
        var targetSpawn = 1f + minutes * GameConfig.DifficultySpawnRatePerMinute;
        var targetSpeed = 1f + minutes * GameConfig.DifficultySpeedPerMinute;

        // smooth towards target over DifficultySmoothing seconds (portable implementation)
        var smooth = deltaTime / GameConfig.DifficultySmoothing;
        if (smooth < 0f) smooth = 0f;
        if (smooth > 1f) smooth = 1f;

        _spawnMultiplier += (targetSpawn - _spawnMultiplier) * smooth;
        _speedMultiplier += (targetSpeed - _speedMultiplier) * smooth;
    }

    public void Reset()
    {
        _spawnMultiplier = 1f;
        _speedMultiplier = 1f;
        ResetManualAdjust();
    }

    public void AdjustManualSpawn(float delta)
    {
        ManualSpawnAdjust = MathF.Max(0.1f, ManualSpawnAdjust + delta);
    }

    public void AdjustManualSpeed(float delta)
    {
        ManualSpeedAdjust = MathF.Max(0.1f, ManualSpeedAdjust + delta);
    }

    public void ResetManualAdjust()
    {
        ManualSpawnAdjust = 1f;
        ManualSpeedAdjust = 1f;
    }
}