using System;
using Test_Project.Core;
using Test_Project.Services;

namespace Test_Project.Managers;

/// <summary>
/// Manages game score, points, and high score persistence
/// </summary>
public class ScoreManager
{
    public float TimeScore { get; private set; }
    public int Points { get; private set; }
    public int HighScore { get; private set; }

    private readonly HighScoreService _highScoreService;

    public ScoreManager(HighScoreService highScoreService)
    {
        _highScoreService = highScoreService;
        HighScore = _highScoreService.Load();
    }

    /// <summary>
    /// Updates the time-based score
    /// </summary>
    public void UpdateTime(float deltaTime)
    {
        TimeScore += deltaTime;
    }

    /// <summary>
    /// Adds points for kills
    /// </summary>
    public void AddPoints(int points)
    {
        Points += points;
    }

    /// <summary>
    /// Gets the total score (time + points)
    /// </summary>
    public int GetTotalScore()
    {
        return (int)TimeScore + (Points * GameConfig.PointsPerKill);
    }

    /// <summary>
    /// Calculates current obstacle speed based on difficulty
    /// </summary>
    public float GetObstacleSpeed(float speedMultiplier)
    {
        return (GameConfig.InitialObstacleSpeed + 
               (TimeScore / GameConfig.ObstacleSpeedDifficulty) * GameConfig.ObstacleSpeedIncrease) 
               * speedMultiplier;
    }

    /// <summary>
    /// Updates high score if current score is higher
    /// </summary>
    public void UpdateHighScore()
    {
        var total = GetTotalScore();
        if (total > HighScore)
        {
            HighScore = total;
            _highScoreService.Save(HighScore);
        }
    }

    /// <summary>
    /// Resets score for a new game
    /// </summary>
    public void Reset()
    {
        TimeScore = 0f;
        Points = 0;
    }
}
