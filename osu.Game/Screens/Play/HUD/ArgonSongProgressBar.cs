// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonSongProgressBar : SliderBar<double>, ISongProgressBar
    {
        private readonly float baseHeight;
        private readonly float catchupBaseDepth;

        private readonly RoundedBar playfieldBar;
        private readonly RoundedBar catchupBar;

        private readonly Box background;

        private readonly BindableBool showBackground = new BindableBool();

        public bool ShowBackground
        {
            get => showBackground.Value;
            set => showBackground.Value = value;
        }

        private const float alpha_threshold = 2500;

        public Action<double>? OnSeek { get; set; }

        public double StartTime
        {
            private get => CurrentNumber.MinValue;
            set => CurrentNumber.MinValue = value;
        }

        public double EndTime
        {
            private get => CurrentNumber.MaxValue;
            set => CurrentNumber.MaxValue = value;
        }

        public double CurrentTime
        {
            private get => CurrentNumber.Value;
            set => CurrentNumber.Value = value;
        }

        public double ReferenceTime
        {
            private get => currentReference.Value;
            set => currentReference.Value = value;
        }

        private double length => EndTime - StartTime;

        private readonly BindableNumber<double> currentReference;

        public bool Interactive { get; set; }

        public ArgonSongProgressBar(float barHeight)
        {
            currentReference = new BindableDouble();
            setupAlternateValue();

            StartTime = 0;
            EndTime = 1;

            RelativeSizeAxes = Axes.X;
            baseHeight = barHeight;
            Height = baseHeight;

            CornerRadius = 5;
            Masking = true;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                },
                catchupBar = new RoundedBar
                {
                    Name = "Audio bar",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    CornerRadius = 5,
                    AlwaysPresent = true,
                    RelativeSizeAxes = Axes.Both
                },
                playfieldBar = new RoundedBar
                {
                    Name = "Playfield bar",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    CornerRadius = 5,
                    AccentColour = Color4.White,
                    RelativeSizeAxes = Axes.Both
                },
            };
            catchupBaseDepth = catchupBar.Depth;
        }

        private void setupAlternateValue()
        {
            CurrentNumber.MaxValueChanged += v => currentReference.MaxValue = v;
            CurrentNumber.MinValueChanged += v => currentReference.MinValue = v;
            CurrentNumber.PrecisionChanged += v => currentReference.Precision = v;
        }

        private float normalizedReference
        {
            get
            {
                if (EndTime - StartTime == 0)
                    return 1;

                return (float)((ReferenceTime - StartTime) / length);
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            catchupBar.AccentColour = colours.BlueLight;
            showBackground.BindValueChanged(_ => updateBackground(), true);
        }

        private void updateBackground()
        {
            background.FadeTo(showBackground.Value ? 1 : 0, 200, Easing.In);
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (Interactive)
                this.ResizeHeightTo(baseHeight * 3.5f, 200, Easing.Out);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (Interactive)
                this.ResizeHeightTo(baseHeight, 200, Easing.In);

            base.OnHoverLost(e);
        }

        protected override void UpdateValue(float value)
        {
            //
        }

        protected override void Update()
        {
            base.Update();

            playfieldBar.Length = (float)Interpolation.Lerp(playfieldBar.Length, NormalizedValue, Math.Clamp(Time.Elapsed / 40, 0, 1));
            catchupBar.Length = (float)Interpolation.Lerp(catchupBar.Length, normalizedReference, Math.Clamp(Time.Elapsed / 40, 0, 1));

            if (ReferenceTime < CurrentTime)
                ChangeChildDepth(catchupBar, playfieldBar.Depth - 0.1f);
            else
                ChangeChildDepth(catchupBar, catchupBaseDepth);

            float timeDelta = (float)(Math.Abs(CurrentTime - ReferenceTime));
            catchupBar.Alpha = MathHelper.Clamp(timeDelta, 0, alpha_threshold) / alpha_threshold;
        }

        private ScheduledDelegate? scheduledSeek;

        protected override void OnUserChange(double value)
        {
            scheduledSeek?.Cancel();
            scheduledSeek = Schedule(() =>
            {
                if (Interactive)
                    OnSeek?.Invoke(value);
            });
        }

        private partial class RoundedBar : Container
        {
            private readonly Box fill;
            private readonly Container mask;
            private float length;

            public RoundedBar()
            {
                Masking = true;
                Children = new[]
                {
                    mask = new Container
                    {
                        Masking = true,
                        RelativeSizeAxes = Axes.Y,
                        Size = new Vector2(1),
                        Child = fill = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.White
                        }
                    }
                };
            }

            public float Length
            {
                get => length;
                set
                {
                    length = value;
                    mask.Width = value * DrawWidth;
                    fill.Width = value * DrawWidth;
                }
            }

            public new float CornerRadius
            {
                get => base.CornerRadius;
                set
                {
                    base.CornerRadius = value;
                    mask.CornerRadius = value;
                }
            }

            public ColourInfo AccentColour
            {
                get => fill.Colour;
                set => fill.Colour = value;
            }
        }
    }
}
