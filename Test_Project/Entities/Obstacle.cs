using Microsoft.Xna.Framework;

namespace Test_Project.Entities
{
    public class Obstacle
    {
        public Rectangle Rect;

        public Obstacle(Rectangle rect) => Rect = rect;

        public void Update(float dt, float speed)
        {
            Rect = new Rectangle(Rect.X, Rect.Y + (int)(speed * dt), Rect.Width, Rect.Height);
        }
    }
}
