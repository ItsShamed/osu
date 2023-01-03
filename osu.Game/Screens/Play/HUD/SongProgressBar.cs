// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Threading;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public abstract partial class SongProgressBar : SliderBar<double>
    {
        public Action<double>? OnSeek;

        protected abstract Drawable Fill { get; set; }

        public abstract bool ShowHandle { get; set; }

        public virtual Color4 FillColour
        {
            set => Fill.Colour = value;
        }

        public double StartTime
        {
            set => CurrentNumber.MinValue = value;
        }

        public double EndTime
        {
            set => CurrentNumber.MaxValue = value;
        }

        public double CurrentTime
        {
            set => CurrentNumber.Value = value;
        }

        private ScheduledDelegate? scheduledSeek;

        protected override void OnUserChange(double value)
        {
            scheduledSeek?.Cancel();
            scheduledSeek = Schedule(() =>
            {
                if (ShowHandle)
                    OnSeek?.Invoke(value);
            });
        }
    }
}
