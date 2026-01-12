using System;
using Microsoft.Xna.Framework;
using Test_Project.Core;
using Test_Project.Entities;

namespace Test_Project.Managers;

/// <summary>
/// Manages spawning of obstacles and power-ups
/// </summary>
public class SpawnManager
{
    private float _obstacleSpawnTimer;
    private float _powerUpSpawnTimer;
    private float _spawnInterval;
    private readonly Random _rand;
    private readonly int _screenWidth;

    public SpawnManager(Random random, int screenWidth)
    {
        _rand = random;
        _screenWidth = screenWidth;
        _spawnInterval = GameConfig.InitialSpawnInterval;
    }

    /// <summary>
    /// Updates spawn timers and spawns entities based on current difficulty
    /// </summary>
    public void Update(float deltaTime, float score, 
        Action<Obstacle> onObstacleSpawn, 
        Action<PowerUp> onPowerUpSpawn)
    {
        // Update spawn interval based on score
        _spawnInterval = MathF.Max(GameConfig.MinSpawnInterval, 
            GameConfig.InitialSpawnInterval - (score / GameConfig.SpawnIntervalDifficulty));

        // Spawn obstacles
        _obstacleSpawnTimer += deltaTime;
        if (_obstacleSpawnTimer >= _spawnInterval)
        {
            _obstacleSpawnTimer = 0f;
            var width = _rand.Next(GameConfig.ObstacleMinWidth, GameConfig.ObstacleMaxWidth);
            var x = _rand.Next(0, _screenWidth - width);
            var rect = new Rectangle(x, -20, width, GameConfig.ObstacleHeight);
            onObstacleSpawn(new Obstacle(rect));
        }

        // Spawn power-ups
        _powerUpSpawnTimer += deltaTime;
        if (_powerUpSpawnTimer >= GameConfig.PowerUpSpawnInterval)
        {
            _powerUpSpawnTimer = 0f;
            var chance = _rand.NextDouble();
            if (chance < GameConfig.PowerUpSpawnChance)
            {
                var x = _rand.Next(0, _screenWidth - GameConfig.PowerUpSize);
                var typeRoll = _rand.NextDouble();
                PowerType type;
                // Rubber Chicken is rare — only selected when typeRoll is very high
                if (typeRoll < 0.4)
                    type = PowerType.Slow;
                else if (typeRoll < 0.8)
                    type = PowerType.Shield;
                else if (typeRoll < 0.96)
                    type = PowerType.ExtraLife;
                else
                    type = PowerType.RubberChicken; // ~4% chance

                onPowerUpSpawn(new PowerUp(
                    new Rectangle(x, -30, GameConfig.PowerUpSize, GameConfig.PowerUpSize), 
                    type));
            }
        }
    }

    /// <summary>
    /// Resets all spawn timers
    /// </summary>
    public void Reset()
    {
        _obstacleSpawnTimer = 0f;
        _powerUpSpawnTimer = 0f;
        _spawnInterval = GameConfig.InitialSpawnInterval;
    }
}
