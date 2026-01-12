namespace Test_Project.Core;

/// <summary>
/// Central configuration for game constants and tuning values
/// </summary>
public static class GameConfig
{
    // Window settings
    public const int WindowWidth = 1366;
    public const int WindowHeight = 728;

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
    public const float DiscoSpeedMultiplier = 0.3f; // Much slower for epilepsy safety
    public const float DiscoColorTransitionSpeed = 0.5f; // Smooth color transitions

    // Scoring
    public const int PointsPerKill = 10;
    
    // Font settings
    public const float FontScaleSmall = 0.6f;
    public const float FontScaleMedium = 0.7f;
    public const float FontScaleLarge = 0.8f;
    public const float FontScaleCombo = 1.2f;

    // Juice/Polish settings
    public static class Juice
    {
        public const bool EnableSlowMotionOnKill = true;
        public const float SlowMotionDuration = 0.08f;
        public const float SlowMotionTimeScale = 0.3f;
        
        public const bool EnableScreenFlash = true;
        public const float ScreenFlashDuration = 0.1f;
        
        public const bool EnableEnhancedParticles = true;
        public const int KillParticleMultiplier = 3;
        
        public const bool EnableZoomPunch = true;
        public const float ZoomPunchAmount = 0.05f;
        public const float ZoomPunchDuration = 0.15f;
        
        public const bool EnablePopupAnimation = true;
        public const float PopupBounceScale = 1.3f;
        
        public const bool EnableDashParticles = true;
        public const int DashParticleCount = 8;
        
        public const float KillShakeTrauma = 0.3f;
    }
}
