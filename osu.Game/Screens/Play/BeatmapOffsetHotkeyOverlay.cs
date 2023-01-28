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
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Screens.Play
{
    public partial class BeatmapOffsetHotkeyOverlay : BeatmapOffsetControl, IKeyBindingHandler<GlobalAction>
    {
        private const double step = 5;
        private const double precision_step = 1;

        private OffsetUpdate lastUpdate;
        private bool affine;

        [Resolved]
        private OnScreenDisplay display { get; set; } = null!;

        [Resolved]
        private IFrameStableClock frameStableClock { get; set; } = null!;

        public BeatmapOffsetHotkeyOverlay()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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
            if (e.Action != GlobalAction.IncreaseBeatmapOffset && e.Action != GlobalAction.DecreaseBeatmapOffset && e.Action != GlobalAction.AffineBeatmapOffset)
                return false;

            if (e.Repeat)
                return false;

            if (Clock.CurrentTime - lastUpdate.Time < 150)
                return false;

            switch (e.Action)
            {
                case GlobalAction.IncreaseBeatmapOffset:
                    Current.Value += affine ? precision_step : step;
                    break;

                case GlobalAction.DecreaseBeatmapOffset:
                    Current.Value -= affine ? precision_step : step;
                    break;

                case GlobalAction.AffineBeatmapOffset:
                    affine = true;
                    break;
            }

            lastUpdate = new OffsetUpdate
            {
                Time = Clock.CurrentTime,
                Offset = Current.Value,
                Shortcut = e.Action
            };

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.AffineBeatmapOffset)
                affine = false;
        }

        protected override void OnOffsetUpdated(ValueChangedEvent<double> offset)
        {
            display.Display(new BeatmapOffsetChangeToast(lastUpdate));
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
                    ? LocalisableString.Interpolate($@"{Offset}ms")
                    : LocalisableString.Interpolate($@"{Offset}ms {getEarlyLateText(Offset)}");

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
