
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace Client
{
    public class EffectBox
    {
        public Rectangle Bounds { get; set; }
        public Point Position => new Point(Bounds.X, Bounds.Y);
        public Point Size => new Point(Bounds.Width, Bounds.Height);
        public int Effect { get; set; }
        public int EffectPeriod { get; set; }
        public int Lifespan { get; set; }

        public EffectBox(Rectangle bounds, int effect, int effectPeriod, int lifespan)
        {
            Bounds = bounds;
            Effect = effect;
            EffectPeriod = effectPeriod;
            Lifespan = lifespan;
        }

        public bool IsColliding(Rectangle hitbox) => Bounds.Intersects(hitbox);

        public void Update()
        {
            Lifespan--;
            Debug.WriteLine($"Box {Position} has lifespan of {Lifespan}");
        }

        public void UpdateCollision(int playerId)
        {
            if (!Server.IsRunning && !Server.PlayerManager.Contains(playerId))
                return; // Only server sided

            Player player = Server.PlayerManager.GetPlayer(playerId);
            if (!player.EffectBoxTimers.ContainsKey(Position))
                player.EffectBoxTimers.Add(Position, 0);
            if (IsColliding(player.Hitbox))
                player.EffectBoxTimers[Position]++;
            else
                player.EffectBoxTimers[Position] = 0;

            if (player.EffectBoxTimers[Position] >= EffectPeriod && player.EffectBoxTimers[Position] % EffectPeriod == 0)
                Server.PlayerManager.ChangePlayerHealth(playerId, Effect);
        }

    }
}
