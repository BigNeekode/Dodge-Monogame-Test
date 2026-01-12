using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Test_Project.Systems;

/// <summary>
/// Screen flash effect for impactful moments (kills, hits, etc.)
/// </summary>
public class ScreenFlash
{
    private float _flashTimer;
    private float _flashDuration;
    private Color _flashColor;

    public bool IsFlashing => _flashTimer > 0;

    /// <summary>
    /// Trigger a screen flash with specified color and duration
    /// </summary>
    public void Flash(Color color, float duration)
    {
        _flashColor = color;
        _flashDuration = duration;
        _flashTimer = duration;
    }

    /// <summary>
    /// Quick white flash (for kills/impacts)
    /// </summary>
    public void FlashWhite(float duration = 0.1f)
    {
        Flash(Color.White, duration);
    }

    public void Update(float deltaTime)
    {
        if (_flashTimer > 0)
        {
            _flashTimer -= deltaTime;
        }
    }

    /// <summary>
    /// Draw the flash overlay (call last in draw order)
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, int screenWidth, int screenHeight)
    {
        if (_flashTimer <= 0) return;

        float alpha = _flashTimer / _flashDuration;
        Color drawColor = _flashColor * alpha;

        spriteBatch.Draw(pixel, new Rectangle(0, 0, screenWidth, screenHeight), drawColor);
    }
}
