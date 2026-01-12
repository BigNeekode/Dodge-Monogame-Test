using Microsoft.Xna.Framework;

namespace Test_Project.Entities
{
    public class Bullet
    {
        public Rectangle Rect;
        public Vector2 Vel;

        public Bullet(Rectangle rect, Vector2 velocity)
        {
            Rect = rect;
            Vel = velocity;
        }

        public void Update(float dt)
        {
            Rect = new Rectangle((int)(Rect.X + Vel.X * dt), (int)(Rect.Y + Vel.Y * dt), Rect.Width, Rect.Height);
        }
    }
}
