using Microsoft.Xna.Framework;

namespace Test_Project.Entities
{
    public class Particle
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public float Life;
        public Color Col;

        public void Update(float dt)
        {
            Pos += Vel * dt;
            Life -= dt;
        }
    }
}
