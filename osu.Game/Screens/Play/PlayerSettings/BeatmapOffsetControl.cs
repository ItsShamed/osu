// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Database;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public abstract partial class BeatmapOffsetControl : CompositeDrawable
    {
        public BindableDouble Current { get; } = new BindableDouble
        {
            MinValue = -50,
            MaxValue = 50,
            Precision = 0.1,
        };

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        protected IBindable<WorkingBeatmap> Beatmap { get; set; } = null!;

        private IDisposable? beatmapOffsetSubscription;

        private Task? realmWriteTask;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapOffsetSubscription = realm.SubscribeToPropertyChanged(
                r => r.Find<BeatmapInfo>(Beatmap.Value.BeatmapInfo.ID)?.UserSettings,
                settings => settings.Offset,
                val =>
                {
                    // At the point we reach here, it's not guaranteed that all realm writes have taken place (there may be some in-flight).
                    // We are only aware of writes that originated from our own flow, so if we do see one that's active we can avoid handling the feedback value arriving.
                    if (realmWriteTask == null)
                        Current.Value = val;

                    if (realmWriteTask?.IsCompleted == true)
                    {
                        // we can also mark any in-flight write that is managed locally as "seen" and start handling any incoming changes again.
                        realmWriteTask = null;
                    }
                });

            Current.BindValueChanged(currentChanged);
        }

        protected virtual void OnOffsetUpdated(ValueChangedEvent<double> offset)
        {
        }

        private void currentChanged(ValueChangedEvent<double> offset)
        {
            Scheduler.AddOnce(updateOffset);

            void updateOffset()
            {
                // ensure the previous write has completed. ignoring performance concerns, if we don't do this, the async writes could be out of sequence.
                if (realmWriteTask?.IsCompleted == false)
                {
                    Scheduler.AddOnce(updateOffset);
                    return;
                }

                OnOffsetUpdated(offset);

                realmWriteTask = realm.WriteAsync(r =>
                {
                    var settings = r.Find<BeatmapInfo>(Beatmap.Value.BeatmapInfo.ID)?.UserSettings;

                    if (settings == null) // only the case for tests.
                        return;

                    double val = Current.Value;

                    if (settings.Offset == val)
                        return;

                    settings.Offset = val;
                });
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            beatmapOffsetSubscription?.Dispose();
        }
    }
}
