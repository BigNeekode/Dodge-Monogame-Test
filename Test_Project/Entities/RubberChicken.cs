using Microsoft.Xna.Framework;

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
            Rect = new Rectangle(Rect.X + (int)(Vel.X * dt), Rect.Y + (int)(Vel.Y * dt), Rect.Width, Rect.Height);
            // basic gravity
            Vel += new Vector2(0, 400f * dt);
        }
    }
}
