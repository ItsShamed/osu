// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD.ClicksPerSecond
{
    public abstract class GameplayClicksPerSecondCounter : RollingCounter<int>, ISkinnableDrawable
    {
        [Resolved]
        private ClicksPerSecondCalculator calculator { get; set; } = null!;

        protected override double RollingDuration => 350;

        public bool UsesFixedAnchor { get; set; }

        protected GameplayClicksPerSecondCounter()
        {
            Current.Value = 0;
        }

        protected override void Update()
        {
            base.Update();

            Current.Value = calculator.Value;
        }
    }
}
