using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Client
{ 
    class Enemy : Entity
    {

        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int Damage { get; set; }

        public bool IsAlive => Health > 0;

        public Enemy(Texture2D texture, Vector2 position, Point size, float speed, int maxHealth, int damage) : base(texture, position, size, speed)
        {
            Health = maxHealth;
            MaxHealth = maxHealth;

            Damage = damage;
        }

        public Enemy(Texture2D texture, float x, float y, int width, int height, float speed, int maxHealth, int damage) : base(texture, x, y, width, height, speed)
        {
            Health = maxHealth;
            MaxHealth = maxHealth;

            Damage = damage;
        }

        public void TakeDamage(int amount)
        {
            Health = Math.Clamp(Health - amount, 0, MaxHealth);
        }
    }

    class Totem : Enemy
    {
        public Totem(Texture2D texture, Vector2 position, Point size, int maxHealth) : base(texture, position, size, 0, maxHealth, 0)
        { }

        public Totem(Texture2D texture, float x, float y, int width, int height, int maxHealth) : base(texture, x, y, width, height, 0, maxHealth, 0)
        { }

        public void Update()
        {
            if (!IsAlive)
                return;
        }

        public void Draw()
        {
            if (!IsAlive)
                return;

            Camera.Draw(Texture, Hitbox, Color.White);
        }

    }

    class Boss1 : Enemy
    {
        public bool StartedBossFight { get; private set; }
        public int phase { get; private set; }

        public List<Totem> InvulnerabilityTotems { get; private set; }
        public Boss1(Texture2D texture, Vector2 position, Point size, float speed, int maxHealth) : base(texture, position, size, speed, maxHealth, 0)
        {
            phase = 0;
            InvulnerabilityTotems = new List<Totem>();
        }

        public Boss1(Texture2D texture, float x, float y, int width, int height, float speed, int maxHealth) : base(texture, x, y, width, height, speed, maxHealth, 0)
        {
            phase = 0;
            InvulnerabilityTotems = new List<Totem>();
        }

        public void StartBossFight()
        {
            StartedBossFight = true;
        }

        public new void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            string message = Message.CreateUpdateBossMessage(Position, Health, MaxHealth);
            Server.SendGlobalMessage(message);
        }

        private bool[] startedPhase = { false, false, false };

        public void Update()
        {
            if (!StartedBossFight)
                phase = 0;
            else if (Health > 75)
                phase = 1;
            else if (Health > 33)
                phase = 2;
            else if (Health > 0)
                phase = 3;

            if(phase != 0)
                Phase(phase);
            UI.AddMessage($"Phase: {phase}");
            UI.AddMessage($"Attack: {AttackChargeUp}");
        }

        private int AttackChargeUp = 0;
        private int AttackCharge = (int)(NetworkManager.TICK_RATE * 10f);

        public void Attack()
        {
            if (AttackChargeUp < AttackCharge)
            {
                AttackChargeUp++;
                return;
            }
            Point boxCenter = new Point((int)Center.X, (int)Center.Y);
            int radius = Width * 2;
            int effect = Damage;
            int effectPeriod = 1;
            int lifespan = 1;
            EffectBoxTrigger trigger = EffectBoxTrigger.Attack;
            EffectBox box = new EffectBox(boxCenter, radius, effect, effectPeriod, lifespan, trigger);
            Dungeon.AddEffectBox(TilemapName.BossRoom, box);
            AttackChargeUp = 0;
        }


        public void StartPhase(int PhaseNumber)
        {
            int totemCount, totemHealth, totemDistance;
            switch (PhaseNumber)
            {
                case 1:
                    totemCount = 3;
                    totemHealth = 20;
                    totemDistance = 150;
                    break;
                case 2:
                    totemCount = 4;
                    totemHealth = 50;
                    totemDistance = 170;
                    break;
                case 3:
                    totemCount = 5;
                    totemHealth = 75;
                    totemDistance = 250;
                    break;
                default:
                    return;
            }
            
            startedPhase[PhaseNumber - 1] = true;
            InvulnerabilityTotems.Clear();
            Texture2D texture = Camera.EntityAssets[EntityType.Totem];
            Vector2 pos;
            Point size = new Point(texture.Width, texture.Height);
            for (int i = 0; i < totemCount; i++)
            {
                double angle = (2 * i * Math.PI / totemCount) + (Math.PI / 2);
                pos = new Vector2();
                pos.X = (float)(Center.X + totemDistance * Math.Cos(angle) - texture.Width / 2);
                pos.Y = (float)(Center.Y + totemDistance * Math.Sin(angle) - texture.Height / 2);
                InvulnerabilityTotems.Add(new Totem(Camera.EntityAssets[EntityType.Totem], pos, size, totemHealth));
            }
        }

        public void Phase(int PhaseNumber)
        {
            string message;
            int deadTotems = 0;

            switch (PhaseNumber)
            {
                case 1:
                    Damage = 30;
                    break;
                case 2:
                    Damage = 67;
                    break;
                case 3:
                    Damage = 99;
                    break;
                default:
                    Debug.WriteLine("Invalid Phase");
                    return;
            }
            if (!startedPhase[PhaseNumber-1])
                StartPhase(PhaseNumber);
            foreach (Totem totem in InvulnerabilityTotems)
            {
                totem.Update();
                if (!totem.IsAlive)
                    deadTotems++;
                message = Message.CreateUpdateTotemMessage(totem.Position, totem.Health, totem.MaxHealth);
            }
            UI.AddMessage($"Totems: {InvulnerabilityTotems.Count}");
            if (deadTotems >= InvulnerabilityTotems.Count)
                return;
            Attack();


        }

        

        public void Draw()
        {
            if (phase == 0)
                return;

            Camera.Draw(Texture, Hitbox, Color.White);
            foreach (Totem totem in InvulnerabilityTotems)
            {
                totem.Draw();
            }
        }
    }
}
