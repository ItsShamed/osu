// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Graphics
{
    public partial class Fountain : ParticleSpewer
    {
        private float angleOffset;
        private float timeRemaining;

        public float BaseAngle = 0;
        public float SpreadAngle = 35;
        public float AngleVariation = 5f;
        public float ReachRadius = 1.6f;
        public float Gravity = 0.75f;

        public Vector2 BasePosition = new Vector2(0.5f);

        public Fountain(Texture? texture, int spawnRate = 40)
            : base(texture, spawnRate, 1000)
        {
        }

        protected override void Update()
        {
            base.Update();

            if (timeRemaining <= 0)
            {
                Stop();
            }
        }

        protected override FallingParticle CreateParticle()
        {
            (float sin, float cos) =
                MathF.SinCos(MathHelper.DegreesToRadians(BaseAngle + angleOffset + RNG.NextSingle(-AngleVariation, AngleVariation)));

            return new FallingParticle
            {
                Velocity = new Vector2(ReachRadius) * new Vector2(sin, cos),
                StartPosition = BasePosition,
                Duration = 1000,
                StartAngle = BaseAngle,
                StartScale = 2f,
                EndAngle = BaseAngle + RNG.NextSingle(-AngleVariation, AngleVariation),
                EndScale = RNG.NextSingle(1.5f, 3f)
            };
        }

        protected override float ParticleGravity => Gravity;

        public void Shoot(FountainAnimation fountainAnimation, float duration)
        {
            ClearTransforms();

            timeRemaining = duration;
            Active.Value = true;
            this.TransformTo(nameof(timeRemaining), 0f, duration);

            switch (fountainAnimation)
            {
                case FountainAnimation.ClockwiseTurn:
                    angleOffset = -SpreadAngle;
                    this.TransformTo(nameof(angleOffset), SpreadAngle, duration);
                    break;

                case FountainAnimation.CounterClockwiseTurn:
                    angleOffset = SpreadAngle;
                    this.TransformTo(nameof(angleOffset), -SpreadAngle, duration);
                    break;

                default:
                    angleOffset = 0;
                    break;
            }
        }

        public void Stop()
        {
            ClearTransforms();
            Active.Value = false;
            timeRemaining = 0;
            angleOffset = 0;
        }
    }

    public enum FountainAnimation
    {
        Idle,
        ClockwiseTurn,
        CounterClockwiseTurn,
    }

    public static class FountainAnimationExtensions
    {
        public static FountainAnimation Invert(this FountainAnimation animation) => animation switch
        {
            FountainAnimation.ClockwiseTurn => FountainAnimation.CounterClockwiseTurn,
            FountainAnimation.CounterClockwiseTurn => FountainAnimation.ClockwiseTurn,
            _ => animation
        };
    }
}
