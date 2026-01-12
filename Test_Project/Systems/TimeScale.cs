namespace Test_Project.Systems;

/// <summary>
/// Manages time scale for slow-motion effects
/// </summary>
public class TimeScale
{
    private float _slowMotionTimer;
    private float _targetScale = 1f;
    private float _slowMotionScale = 1f;

    public float Scale { get; private set; } = 1f;

    /// <summary>
    /// Apply slow-motion effect for specified duration
    /// </summary>
    public void ApplySlowMotion(float scale, float duration)
    {
        _slowMotionScale = scale;
        _targetScale = scale;
        _slowMotionTimer = duration;
        Scale = scale;
    }

    public void Update(float deltaTime)
    {
        if (_slowMotionTimer > 0)
        {
            _slowMotionTimer -= deltaTime;
            
            if (_slowMotionTimer <= 0)
            {
                _targetScale = 1f;
                Scale = 1f;
            }
        }
    }

    /// <summary>
    /// Get scaled delta time for game updates
    /// </summary>
    public float GetScaledDelta(float deltaTime)
    {
        return deltaTime * Scale;
    }
}
