// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModScaleToBeat : Mod, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => @"Scale To the Beat";
        public override string Description => @"Now the circles groove too!";
        public override double ScoreMultiplier => 1;
        public override string Acronym => @"S2B";

        private IFrameStableClock? clock;
        private float currentAmplitude;

        [SettingSource("Size multiplier", "How big the circles should be.")]
        public BindableFloat SizeMultiplier { get; } = new BindableFloat(1.25f)
        {
            Precision = 0.01f,
            Default = 1.25f,
            MaxValue = 2,
            MinValue = 0.5f
        };

        public void Update(Playfield playfield)
        {
            float amplitude = getAvgLowFreqAmplitude(playfield.Dependencies.Get(typeof(IBeatSyncProvider)) as IBeatSyncProvider);

            foreach (var drawableHitObject in playfield.HitObjectContainer.AliveObjects)
            {
                switch (drawableHitObject)
                {
                    case DrawableHitCircle:
                        updateSingleCircle(drawableHitObject, amplitude);
                        break;
                }
            }
        }

        private void updateSingleCircle(Drawable hitObject, float amplitude)
        {
            float size = (float)Interpolation.Lerp(1, SizeMultiplier.Value, amplitude);

            hitObject.ScaleTo(size, hitObject.Scale.X > 1 ? 0 : 500);
        }

        private float getAvgLowFreqAmplitude(IBeatSyncProvider? source)
        {
            if (source == null || clock == null)
                return 0;

            var amplitudes = source.CurrentAmplitudes.FrequencyAmplitudes.Span;

            float amplitudeSum = 0;

            for (int i = 0; i < 2; i++)
            {
                amplitudeSum += amplitudes[i];
            }

            float updatedAmplitude = amplitudeSum / 2f;
            float interpolatedAmplitude = (float)Interpolation.DampContinuously(currentAmplitude, updatedAmplitude, 500, clock.ElapsedFrameTime);

            currentAmplitude = updatedAmplitude;
            return interpolatedAmplitude;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            clock = drawableRuleset.FrameStableClock;
        }
    }
}
