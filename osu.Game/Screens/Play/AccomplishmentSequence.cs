// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play
{
    public abstract partial class AccomplishmentSequence : CompositeDrawable
    {
        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoreProcessor.HasCompleted.BindValueChanged(onScoreComplete);
        }

        protected abstract void StartSequence(Accomplishment accomplishment);

        private void onScoreComplete(ValueChangedEvent<bool> e)
        {
            if (!e.NewValue)
                return;

            // To account for osu!mania where 100% accuracy is rarely feasible on lazer
            // we count 300s (or greats) as being "perfect"
            int nonPerfectHits = scoreProcessor.Statistics.GetValueOrDefault(HitResult.Miss, 0)
                                 + scoreProcessor.Statistics.GetValueOrDefault(HitResult.Meh, 0)
                                 + scoreProcessor.Statistics.GetValueOrDefault(HitResult.Good, 0)
                                 + scoreProcessor.Statistics.GetValueOrDefault(HitResult.Ok, 0);

            if (nonPerfectHits == 0)
            {
                StartSequence(Accomplishment.Perfect);
                return;
            }

            if (scoreProcessor.HighestCombo.Value == scoreProcessor.MaximumCombo)
            {
                StartSequence(Accomplishment.FullCombo);
                return;
            }

            if (scoreProcessor.Statistics.Any(stat => stat.Key.BreaksCombo() && stat.Value > 0))
            {
                Expire();
                return;
            }

            StartSequence(Accomplishment.NoBreak);
        }

        /// <summary>
        /// Describes a notable accomplishment w.r.t the score
        /// </summary>
        protected enum Accomplishment
        {
            /// <summary>
            /// The score has no combo breaks
            /// </summary>
            [Description("No Break")] // this could use some better naming, but is fine for now as it's explicit
            NoBreak,

            /// <summary>
            /// The score has reached the maximum combo achievable, this means that all ticks have been hit
            /// </summary>
            [Description("Full Combo")]
            FullCombo,

            /// <summary>
            /// The score has only contains greats and perfects
            /// </summary>
            /// <remarks>
            /// Perfects (320) are only applicable to osu!mania
            /// </remarks>
            [Description("All Perfect")]
            Perfect
        }
    }
}
