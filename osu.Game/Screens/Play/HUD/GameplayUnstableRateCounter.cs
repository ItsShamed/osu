// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public abstract class GameplayUnstableRateCounter : RollingCounter<int>, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        protected override double RollingDuration => 750;

        protected readonly Bindable<bool> Valid = new BindableBool();

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; }

        protected GameplayUnstableRateCounter()
        {
            Current.Value = 0;
        }

        private bool changesUnstableRate(JudgementResult judgement)
            => !(judgement.HitObject.HitWindows is HitWindows.EmptyHitWindows) && judgement.IsHit;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoreProcessor.NewJudgement += updateDisplay;
            scoreProcessor.JudgementReverted += updateDisplay;
            updateDisplay();
        }

        private void updateDisplay(JudgementResult _) => Scheduler.AddOnce(updateDisplay);

        private void updateDisplay()
        {
            double? unstableRate = scoreProcessor.HitEvents.CalculateUnstableRate();

            Valid.Value = unstableRate != null;
            if (unstableRate != null)
                Current.Value = (int)Math.Round(unstableRate.Value);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (scoreProcessor == null) return;

            scoreProcessor.NewJudgement -= updateDisplay;
            scoreProcessor.JudgementReverted -= updateDisplay;
        }
    }
}
