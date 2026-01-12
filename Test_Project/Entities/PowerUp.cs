using Microsoft.Xna.Framework;

namespace Test_Project.Entities
{
    public enum PowerType { Slow, Shield, ExtraLife, Shotgun }

    public class PowerUp
    {
        public Rectangle Rect;
        public PowerType Type;

        public PowerUp(Rectangle rect, PowerType type)
        {
            Rect = rect;
            Type = type;
        }

        public void Update(float dt, float speed)
        {
            Rect = new Rectangle(Rect.X, Rect.Y + (int)(speed * dt), Rect.Width, Rect.Height);
        }
    }
}
