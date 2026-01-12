using System;
using Microsoft.Xna.Framework;
using Test_Project.Core;

namespace Test_Project.Entities
{
    public class RubberChicken
    {
        public Rectangle Rect;
        public Vector2 Vel;
        public int BouncesLeft;

        public RubberChicken(Rectangle rect, Vector2 initialVelocity, int bounces = 3)
        {
            Rect = rect;
            Vel = initialVelocity;
            BouncesLeft = bounces;
        }

        public void Update(float dt)
        {
            // Integrate motion
            Rect = new Rectangle(Rect.X + (int)(Vel.X * dt), Rect.Y + (int)(Vel.Y * dt), Rect.Width, Rect.Height);
            // basic gravity
            Vel += new Vector2(0, 400f * dt);

            // Bounce off screen sides with damping
            if (Rect.X < 0)
            {
                Rect = new Rectangle(0, Rect.Y, Rect.Width, Rect.Height);
                Vel = new Vector2(-Vel.X * 0.7f, Vel.Y);
            }
            else if (Rect.X + Rect.Width > GameConfig.WindowWidth)
            {
                Rect = new Rectangle(GameConfig.WindowWidth - Rect.Width, Rect.Y, Rect.Width, Rect.Height);
                Vel = new Vector2(-Vel.X * 0.7f, Vel.Y);
            }

            // Bounce off floor (prevent sticking and reduce energy)
            var floorY = GameConfig.WindowHeight - 6; // matches renderer floor
            if (Rect.Y + Rect.Height >= floorY)
            {
                Rect = new Rectangle(Rect.X, floorY - Rect.Height, Rect.Width, Rect.Height);
                Vel = new Vector2(Vel.X * 0.8f, -Math.Abs(Vel.Y) * 0.6f);
                BouncesLeft--;
            }
        }
    }
}
