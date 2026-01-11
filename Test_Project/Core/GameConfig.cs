namespace Test_Project.Core;

/// <summary>
/// Central configuration for game constants and tuning values
/// </summary>
public static class GameConfig
{
    // Window settings
    public const int WindowWidth = 800;
    public const int WindowHeight = 600;

    // Player settings
    public const int PlayerWidth = 30;
    public const int PlayerHeight = 30;
    public const float PlayerSpeed = 350f;
    public const float ShootCooldown = 0.25f;
    public const float BulletSpeed = 400f;
    public const int MaxLives = 5;
    public const float DashSpeed = 800f;
    public const float DashDuration = 0.15f;
    public const float DashCooldown = 1.5f;

    // Obstacle settings
    public const float InitialObstacleSpeed = 180f;
    public const float ObstacleSpeedIncrease = 20f;
    public const float ObstacleSpeedDifficulty = 5f; // score divisor
    public const float InitialSpawnInterval = 1.0f;
    public const float MinSpawnInterval = 0.35f;
    public const float SpawnIntervalDifficulty = 30f; // score divisor
    public const int ObstacleMinWidth = 20;
    public const int ObstacleMaxWidth = 110;
    public const int ObstacleHeight = 20;

    // Power-up settings
    public const float PowerUpSpawnInterval = 5.5f;
    public const float PowerUpSpawnChance = 0.7f;
    public const int PowerUpSize = 22;
    public const float PowerUpSpeed = 108f; // 60% of base obstacle speed
    
    // Power-up effect durations
    public const float ShieldDuration = 6f;
    public const float SlowDuration = 5f;
    public const float SlowMultiplier = 0.6f;
    public const float RubberChickenDuration = 10f;
    public const int RubberChickenBounces = 4;

    // Disco mode settings
    public const float DiscoSpeedMultiplier = 1.6f;

    // Scoring
    public const int PointsPerKill = 10;
}
