using Microsoft.Xna.Framework;

namespace Test_Project.Entities
{
    public class Bullet
    {
        public Rectangle Rect;
        public int Speed;

        public Bullet(Rectangle rect, int speed)
        {
            Rect = rect;
            Speed = speed;
        }

        public void Update(float dt)
        {
            Rect = new Rectangle(Rect.X, Rect.Y - (int)(Speed * dt), Rect.Width, Rect.Height);
        }
    }
}
