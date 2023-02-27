// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Skinning;

namespace osu.Game.Graphics
{
    public abstract partial class KiaiFountainOverlay : BeatSyncedContainer
    {
        protected Fountain LeftFountain = null!;
        protected Fountain RightFountain = null!;

        private bool wasKiai;

        protected KiaiFountainOverlay()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(SkinManager skinManager)
        {
            LoadFountains(skinManager);
            if (LeftFountain == null! || RightFountain == null!)
                throw new InvalidOperationException("Some fountains were not initialized.");
        }

        protected abstract void LoadFountains(SkinManager skinManager);

        protected virtual void Shoot()
        {
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            if (wasKiai == effectPoint.KiaiMode)
                return;

            wasKiai = effectPoint.KiaiMode;

            if (wasKiai)
                Shoot();
        }
    }
}
