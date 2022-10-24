// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public class ArgonSongProgress : SongProgress
    {
        private readonly ArgonSongProgressBar playfieldBar; // Tracking playfield's clock
        private readonly ArgonSongProgressBar catchupBar; // Tracking Track clock
        private readonly ArgonSongProgressBar seekBar; // For seeking

        private readonly SongProgressInfo info;
        private readonly ArgonSongProgressGraph graph;

        private const float bar_height = 10;
        private const float transition_duration = 200;
        private Color4 seekColour;

        public readonly Bindable<bool> AllowSeeking = new BindableBool();

        [Resolved]
        private DrawableRuleset? drawableRuleset { get; set; }

        [Resolved]
        private Player? player { get; set; }

        public ArgonSongProgress()
        {
            Anchor = Anchor.BottomCentre;
            Children = new Drawable[]
            {
                info = new SongProgressInfo
                {
                    Origin = Anchor.TopLeft,
                    Name = "Info",
                    Anchor = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.X,
                    ShowProgress = false
                },
                graph = new ArgonSongProgressGraph
                {
                    Name = "Difficulty graph",
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Alpha = 0.5f,
                    Masking = true,
                    CornerRadius = 5,
                    LowestSegmentColour = Colour4.FromHex("#333333"),
                    LowSegmentColour = Colour4.FromHex("#4F4F4F"),
                    MidSegmentColour = Colour4.FromHex("#828282"),
                    HighSegmentColour = Colour4.FromHex("#BDBDBD"),
                    HighestSegmentColour = Colour4.FromHex("#E0E0E0"),
                    Depth = 2
                },
                playfieldBar = new ArgonSongProgressBar(bar_height)
                {
                    Name = "Playfield bar",
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    FillColour = Color4.White,
                    Depth = 0f
                },
                catchupBar = new ArgonSongProgressBar(bar_height)
                {
                    Name = "Catch-up bar",
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    AlwaysPresent = true,
                },
                seekBar = new ArgonSongProgressBar(bar_height)
                {
                    Name = "Seek bar",
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    AlwaysPresent = true,
                    Alpha = 0f,
                    OnSeek = time => player?.Seek(time),
                    Depth = -2
                }
            };
            Origin = Anchor.BottomCentre;
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            base.LoadComplete();

            if (drawableRuleset != null)
            {
                if (player?.Configuration.AllowUserInteraction == true)
                    ((IBindable<bool>)AllowSeeking).BindTo(drawableRuleset.HasReplayLoaded);
            }

            info.ShowProgress = false;
            info.TextColour = Colour4.White;
            info.Font = OsuFont.Torus.With(size: 18, weight: FontWeight.Bold);
            catchupBar.FillColour = seekColour = colours.BlueLight;
        }

        protected override void LoadComplete()
        {
            AllowSeeking.BindValueChanged(_ => updateBarVisibility(), true);
        }

        protected override void UpdateObjects(IEnumerable<HitObject> objects)
        {
            graph.Objects = objects;

            info.StartTime = FirstHitTime;
            info.EndTime = LastHitTime;
            catchupBar.StartTime = FirstHitTime;
            catchupBar.EndTime = LastHitTime;
            seekBar.StartTime = FirstHitTime;
            seekBar.EndTime = LastHitTime;
        }

        private void updateBarVisibility()
        {
            catchupBar.AllowHover = AllowSeeking.Value;
            playfieldBar.AllowHover = AllowSeeking.Value;
            seekBar.ShowHandle = AllowSeeking.Value;
        }

        protected override void Update()
        {
            base.Update();
            Height = playfieldBar.Height + bar_height + info.Height;
            graph.Height = playfieldBar.Height;

            IClock referenceClock = drawableRuleset?.FrameStableClock ?? GameplayClock;

            float timeDiff = (float)(GameplayClock.CurrentTime - referenceClock.CurrentTime);
            ChangeChildDepth(catchupBar, MathHelper.Clamp(timeDiff, -1, 1));

            double alphaThreshold = (LastHitTime - FirstHitTime) * 0.03;

            catchupBar.Alpha = (float)(MathHelper.Clamp(MathF.Abs(timeDiff), 0, alphaThreshold) / alphaThreshold);

            if (MathF.Abs(timeDiff) > 1f)
            {
                catchupBar.FillColour = seekColour;
            }
        }

        protected override void PopIn()
        {
            this.FadeIn(500, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(100);
        }

        protected override void UpdateProgress(double progress, bool isIntro)
        {
            catchupBar.CurrentTime = GameplayClock.CurrentTime;

            if (isIntro)
                playfieldBar.CurrentTime = 0;
            else
                playfieldBar.CurrentTime = progress;
        }
    }
}
