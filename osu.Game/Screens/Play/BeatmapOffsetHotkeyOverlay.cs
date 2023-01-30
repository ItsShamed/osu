// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.OSD;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Screens.Play
{
    public partial class BeatmapOffsetHotkeyOverlay : BeatmapOffsetControl, IKeyBindingHandler<GlobalAction>
    {
        private const double step = 5;
        private const double precision_step = 1;

        private OffsetUpdate lastUpdate;

        private readonly bool restrictedInteraction;

        private readonly IBindable<bool> isPaused = new BindableBool();
        public IBindable<bool> IsBreakTime { get; } = new BindableBool();

        [Resolved(canBeNull: true)]
        private OnScreenDisplay? display { get; set; }

        public BeatmapOffsetHotkeyOverlay(bool restrictedInteraction = false)
        {
            RelativeSizeAxes = Axes.Both;
            this.restrictedInteraction = restrictedInteraction;
        }

        [BackgroundDependencyLoader(true)]
        private void load(IGameplayClock? gameplayClock)
        {
            if (gameplayClock != null)
                isPaused.BindTo(gameplayClock.IsPaused);

            lastUpdate = new OffsetUpdate
            {
                Time = Clock.CurrentTime,
                Shortcut = null,
                Offset = Current.Value
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            lastUpdate.Offset = Current.Value;
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.IncreaseBeatmapOffset:
                    adjustOffset(step, e.Action);
                    return true;

                case GlobalAction.IncreaseBeatmapOffsetFine:
                    adjustOffset(precision_step, e.Action);
                    return true;

                case GlobalAction.DecreaseBeatmapOffset:
                    adjustOffset(-step, e.Action);
                    return true;

                case GlobalAction.DecreaseBeatmapOffsetFine:
                    adjustOffset(-precision_step, e.Action);
                    return true;

                case GlobalAction.ResetBeatmapOffset:
                    resetOffset();
                    return true;
            }

            return false;
        }

        private void adjustOffset(double amount, GlobalAction action)
        {
            if (restrictedInteraction)
            {
                if (!IsBreakTime.Value)
                    return;

                // Rate limiting
                if (Clock.CurrentTime - lastUpdate.Time < 150)
                    return;

                if (isPaused.Value)
                    return;
            }

            lastUpdate = new OffsetUpdate
            {
                Time = Clock.CurrentTime,
                Offset = Current.Value + amount,
                Shortcut = action
            };

            Current.Value += amount;
        }

        private void resetOffset()
        {
            lastUpdate = new OffsetUpdate
            {
                Time = Clock.CurrentTime,
                Offset = 0,
                Shortcut = GlobalAction.ResetBeatmapOffset
            };

            Current.Value = 0;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        protected override void OnOffsetUpdated(ValueChangedEvent<double> offset)
        {
            display?.Display(new BeatmapOffsetChangeToast(lastUpdate));
        }

        private partial class BeatmapOffsetChangeToast : Toast
        {
            public BeatmapOffsetChangeToast(OffsetUpdate update)
                : base(BeatmapOffsetControlStrings.BeatmapOffset, update.GetValueText(), update.Shortcut?.GetLocalisableDescription() ?? ToastStrings.NoKeyBound.ToUpper())
            {
            }
        }

        private struct OffsetUpdate
        {
            public double Time;
            public GlobalAction? Shortcut;
            public double Offset;

            public LocalisableString GetValueText()
                => Offset == 0
                    ? LocalisableString.Interpolate($@"{Offset:F0}ms")
                    : LocalisableString.Interpolate($@"{Offset:F0}ms {getEarlyLateText(Offset)}");

            private LocalisableString getEarlyLateText(double value)
            {
                Debug.Assert(value != 0);

                return value > 0
                    ? BeatmapOffsetControlStrings.HitObjectsAppearEarlierShort
                    : BeatmapOffsetControlStrings.HitObjectsAppearLaterShort;
            }
        }
    }
}
