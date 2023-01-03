// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonSongProgressGraph : SegmentedGraph
    {
        private IEnumerable<HitObject>? objects;

        public IEnumerable<HitObject> Objects
        {
            set
            {
                objects = value;

                const int granularity = 300;
                int[] values = new int[granularity];

                if (!objects.Any())
                    return;

                double firstHit = objects.First().StartTime;
                double lastHit = objects.Max(o => o.GetEndTime());

                if (lastHit == 0)
                    lastHit = objects.Last().StartTime;

                double interval = (lastHit - firstHit + 1) / granularity;

                foreach (var h in objects)
                {
                    double endTime = h.GetEndTime();

                    Debug.Assert(endTime >= h.StartTime);

                    int startRange = (int)((h.StartTime - firstHit) / interval);
                    int endRange = (int)((endTime - firstHit) / interval);
                    for (int i = startRange; i <= endRange; i++)
                        values[i]++;
                }

                Values = values;
            }
        }

        public ArgonSongProgressGraph()
            : base(5)
        {
            for (int i = 0; i < 5; i++)
            {
                TierColours[i] = Colour4.White.Opacity(1 / 5f * 0.85f);
            }
        }
    }
}
