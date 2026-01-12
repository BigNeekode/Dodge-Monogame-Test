using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Test_Project.Core;

namespace Test_Project.Managers;

/// <summary>
/// Simple developer HUD for inspecting and tweaking difficulty at runtime.
/// Toggle with F1. Use PageUp/PageDown to change spawn multiplier, Home/End to change speed. Delete resets.
/// </summary>
public class DevHud
{
    public bool Enabled { get; private set; } = false;

    public void Update(KeyboardState ks, KeyboardState prev, DifficultyManager difficulty)
    {
        // Toggle HUD
        if (ks.IsKeyDown(Keys.F1) && !prev.IsKeyDown(Keys.F1)) Enabled = !Enabled;

        if (!Enabled || difficulty == null) return;

        // Adjust spawn multiplier (PageUp/PageDown)
        if (ks.IsKeyDown(Keys.PageUp) && !prev.IsKeyDown(Keys.PageUp)) difficulty.AdjustManualSpawn(0.1f);
        if (ks.IsKeyDown(Keys.PageDown) && !prev.IsKeyDown(Keys.PageDown)) difficulty.AdjustManualSpawn(-0.1f);

        // Adjust speed multiplier (Home/End)
        if (ks.IsKeyDown(Keys.Home) && !prev.IsKeyDown(Keys.Home)) difficulty.AdjustManualSpeed(0.05f);
        if (ks.IsKeyDown(Keys.End) && !prev.IsKeyDown(Keys.End)) difficulty.AdjustManualSpeed(-0.05f);

        // Reset manual adjustments (Delete)
        if (ks.IsKeyDown(Keys.Delete) && !prev.IsKeyDown(Keys.Delete)) difficulty.ResetManualAdjust();
    }

    public void Draw(SpriteBatch sb, SpriteFont font, int screenWidth)
    {
        if (!Enabled) return;

        var lines = new[]
        {
            "DEV HUD (F1 to toggle):",
            "PageUp/PageDown: Spawn multiplier +/- 0.1",
            "Home/End: Speed multiplier +/- 0.05",
            "Delete: Reset adjustments"
        };

        var y = 10f;
        sb.Begin();
        foreach (var line in lines)
        {
            sb.DrawString(font, line, new Vector2(10, y), Color.Yellow, 0f, Vector2.Zero, GameConfig.FontScaleSmall, SpriteEffects.None, 0f);
            y += 16f;
        }
        sb.End();
    }
}