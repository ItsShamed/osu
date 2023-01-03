// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonSongProgressBar : SongProgressBar
    {
        protected override Drawable Fill { get; set; }

        public override bool ShowHandle { get; set; }
        public bool AllowHover = false;
        private readonly float baseHeight;
        private float currentX;

        private readonly Box fillBlock;
        private readonly Container fillMask;

        public override Color4 FillColour
        {
            set => fillBlock.Colour = value;
        }

        public ArgonSongProgressBar(float barHeight)
        {
            StartTime = 0;
            EndTime = 1;
            baseHeight = barHeight;
            Height = baseHeight;
            RelativeSizeAxes = Axes.X;
            CornerRadius = 5;
            Masking = true;

            Children = new[]
            {
                Fill = new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Child = fillMask = new Container
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Height = barHeight,
                        CornerRadius = 5,
                        Masking = true,
                        Child = fillBlock = new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.White }
                    }
                },
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (ShowHandle || AllowHover)
            {
                this.ResizeHeightTo(baseHeight * 2.5f, 200, Easing.In);
                Fill.ResizeHeightTo(baseHeight * 2.5f, 200, Easing.In);
                fillMask.ResizeHeightTo(baseHeight * 2.5f, 200, Easing.In);
            }

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (ShowHandle || AllowHover)
            {
                this.ResizeHeightTo(baseHeight, 200, Easing.In);
                Fill.ResizeHeightTo(baseHeight, 200, Easing.In);
                fillMask.ResizeHeightTo(baseHeight, 200, Easing.In);
            }

            base.OnHoverLost(e);
        }

        protected override void UpdateValue(float value)
        {
            //
        }

        protected override void Update()
        {
            base.Update();

            float newX = (float)Interpolation.Lerp(currentX, NormalizedValue * UsableWidth, Math.Clamp(Time.Elapsed / 40, 0, 1));

            fillMask.Width = newX;
            currentX = newX;
        }
    }
}
