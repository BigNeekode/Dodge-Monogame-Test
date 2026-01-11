using Microsoft.Xna.Framework;

namespace Test_Project.Systems;

/// <summary>
/// Manages screen shake effects for visual feedback
/// </summary>
public class ScreenShake
{
    private float _trauma;
    private Vector2 _shakeOffset;
    
    private const float MaxShake = 10f;
    private const float TraumaDecay = 2f;

    /// <summary>
    /// Gets the current shake offset to apply to rendering
    /// </summary>
    public Vector2 Offset => _shakeOffset;

    /// <summary>
    /// Adds trauma to trigger screen shake
    /// </summary>
    /// <param name="amount">Trauma amount (0-1)</param>
    public void AddTrauma(float amount)
    {
        _trauma = MathHelper.Clamp(_trauma + amount, 0f, 1f);
    }

    /// <summary>
    /// Updates the screen shake effect
    /// </summary>
    public void Update(float deltaTime)
    {
        if (_trauma > 0f)
        {
            _trauma = MathHelper.Max(0f, _trauma - TraumaDecay * deltaTime);
            
            // Shake based on trauma squared (smoother falloff)
            var shake = _trauma * _trauma;
            
            _shakeOffset = new Vector2(
                (float)(System.Random.Shared.NextDouble() * 2 - 1) * MaxShake * shake,
                (float)(System.Random.Shared.NextDouble() * 2 - 1) * MaxShake * shake
            );
        }
        else
        {
            _shakeOffset = Vector2.Zero;
        }
    }

    /// <summary>
    /// Resets screen shake
    /// </summary>
    public void Reset()
    {
        _trauma = 0f;
        _shakeOffset = Vector2.Zero;
    }
}
