using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Test_Project.Entities;
using Test_Project.Systems;

namespace Test_Project.Managers;

/// <summary>
/// Manages all collision detection and response
/// </summary>
public class CollisionManager
{
    private readonly Random _rand;
    private readonly ParticleSystem _particleSystem;

    public CollisionManager(Random random, ParticleSystem particleSystem)
    {
        _rand = random;
        _particleSystem = particleSystem;
    }

    /// <summary>
    /// Checks bullet-obstacle collisions, spawns confetti, and returns points earned
    /// </summary>
    public int CheckBulletCollisions(List<Bullet> bullets, List<Obstacle> obstacles)
    {
        var pointsEarned = 0;
        
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var bullet = bullets[i];
            
            for (int j = obstacles.Count - 1; j >= 0; j--)
            {
                if (bullet.Rect.Intersects(obstacles[j].Rect))
                {
                    // Spawn confetti at impact point
                    _particleSystem.SpawnConfetti(
                        new Vector2(bullet.Rect.X + bullet.Rect.Width / 2f, 
                                  bullet.Rect.Y + bullet.Rect.Height / 2f), 
                        18, _rand);
                    
                    obstacles.RemoveAt(j);
                    bullets.RemoveAt(i);
                    pointsEarned++;
                    break;
                }
            }
        }
        
        return pointsEarned;
    }

    /// <summary>
    /// Checks rubber chicken-obstacle collisions, spawns confetti, and handles bouncing
    /// </summary>
    public void CheckChickenCollisions(List<RubberChicken> chickens, List<Obstacle> obstacles)
    {
        for (int i = chickens.Count - 1; i >= 0; i--)
        {
            var chicken = chickens[i];
            
            for (int j = obstacles.Count - 1; j >= 0; j--)
            {
                if (chicken.Rect.Intersects(obstacles[j].Rect))
                {
                    _particleSystem.SpawnConfetti(
                        new Vector2(chicken.Rect.Center.X, chicken.Rect.Center.Y), 
                        24, _rand);
                    
                    // Bounce the chicken
                    chicken.Vel = new Vector2(chicken.Vel.X, -Math.Abs(chicken.Vel.Y) * 0.6f);
                    chicken.BouncesLeft--;
                    
                    Console.WriteLine("HONK!");
                    Console.Out.Flush();
                    
                    obstacles.RemoveAt(j);
                    break;
                }
            }
            
            // Remove chickens that ran out of bounces
            if (i < chickens.Count && chickens[i].BouncesLeft <= 0)
            {
                chickens.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Checks power-up collection and returns the collected power-up type (or null)
    /// </summary>
    public PowerType? CheckPowerUpCollection(Rectangle playerRect, List<PowerUp> powerUps)
    {
        for (int i = powerUps.Count - 1; i >= 0; i--)
        {
            if (powerUps[i].Rect.Intersects(playerRect))
            {
                var type = powerUps[i].Type;
                powerUps.RemoveAt(i);
                return type;
            }
        }
        return null;
    }

    /// <summary>
    /// Checks player-obstacle collisions, returns true if player was hit
    /// </summary>
    public bool CheckPlayerCollisions(Rectangle playerRect, List<Obstacle> obstacles, 
        bool shieldActive, Action onShieldHit, Action onPlayerHit)
    {
        for (int i = obstacles.Count - 1; i >= 0; i--)
        {
            if (obstacles[i].Rect.Intersects(playerRect))
            {
                if (shieldActive)
                {
                    _particleSystem.SpawnParticles(
                        new Vector2(obstacles[i].Rect.Center.X, obstacles[i].Rect.Center.Y), 
                        12, Color.Yellow, _rand);
                    obstacles.RemoveAt(i);
                    onShieldHit?.Invoke();
                }
                else
                {
                    _particleSystem.SpawnParticles(
                        new Vector2(playerRect.Center.X, playerRect.Center.Y), 
                        18, Color.Red, _rand);
                    obstacles.RemoveAt(i);
                    onPlayerHit?.Invoke();
                    return true;
                }
            }
        }
        return false;
    }
}
