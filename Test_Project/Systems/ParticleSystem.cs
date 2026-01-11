using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Test_Project.Entities;

namespace Test_Project.Systems
{
    public class ParticleSystem
    {
        private readonly List<Particle> _particles = new();

        public IEnumerable<Particle> Particles => _particles;

        public void SpawnParticles(Vector2 pos, int count, Color col, System.Random rand)
        {
            for (int i = 0; i < count; i++)
            {
                var p = new Particle
                {
                    Pos = pos,
                    Vel = new Vector2((float)(rand.NextDouble() * 2 - 1) * 120f, (float)(rand.NextDouble() * -1) * 80f),
                    Life = 0.6f + (float)rand.NextDouble() * 0.6f,
                    Col = col
                };
                _particles.Add(p);
            }
        }

        public void SpawnConfetti(Vector2 pos, int count, System.Random rand)
        {
            var palette = new[] { Color.Cyan, Color.Magenta, Color.Yellow, Color.LimeGreen, Color.Orange, Color.Pink };
            for (int i = 0; i < count; i++)
            {
                var col = palette[rand.Next(palette.Length)];
                var p = new Particle
                {
                    Pos = pos,
                    Vel = new Vector2((float)(rand.NextDouble() * 2 - 1) * 220f, (float)(rand.NextDouble() * -1) * 160f),
                    Life = 0.7f + (float)rand.NextDouble() * 0.9f,
                    Col = col
                };
                _particles.Add(p);
            }
        }

        public void Update(float dt)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Update(dt);
                if (_particles[i].Life <= 0f) _particles.RemoveAt(i);
            }
        }

        public void Clear() => _particles.Clear();
    }
}
