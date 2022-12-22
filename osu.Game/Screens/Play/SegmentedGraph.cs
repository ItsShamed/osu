// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osuTK;

namespace osu.Game.Screens.Play
{
    public abstract partial class SegmentedGraph : Container
    {
        private BufferedContainer? rectSegments;
        private float previousDrawWidth;
        private bool graphNeedsUpdate;
        private int[]? values;

        public int[] Values
        {
            get => values ?? Array.Empty<int>();
            set
            {
                if (value == values) return;

                values = value;
                graphNeedsUpdate = true;
            }
        }

        public Colour4 LowestSegmentColour { get; set; }
        public Colour4 LowSegmentColour { get; set; }
        public Colour4 MidSegmentColour { get; set; }
        public Colour4 HighSegmentColour { get; set; }
        public Colour4 HighestSegmentColour { get; set; }

        private (Tier tier, float start, float end)[] normalizedSegments = Array.Empty<(Tier, float, float)>();

        private CancellationTokenSource? cts;
        private ScheduledDelegate? scheduledCreate;

        protected override void Update()
        {
            base.Update();

            if (graphNeedsUpdate || (values != null && DrawWidth != previousDrawWidth))
            {
                rectSegments?.FadeOut(500, Easing.OutQuint).Expire();

                scheduledCreate?.Cancel();
                scheduledCreate = Scheduler.AddDelayed(RecreateGraph, 500);

                previousDrawWidth = DrawWidth;
                graphNeedsUpdate = false;
            }
        }

        protected virtual void RecreateGraph()
        {
            var newSegments = new BufferedContainer(cachedFrameBuffer: true)
            {
                RedrawOnScale = false,
                RelativeSizeAxes = Axes.Both
            };

            cts?.Cancel();
            recalculateSegments();
            redrawSegments(newSegments);

            LoadComponentAsync(newSegments, s =>
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        Name = "Background",
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        Colour = LowestSegmentColour
                    },
                    rectSegments = s
                };

                s.FadeInFromZero(500);
            }, (cts = new CancellationTokenSource()).Token);
        }

        private void recalculateSegments()
        {
            if (values == null)
            {
                normalizedSegments = new (Tier tier, float start, float end)[] { (0, 0f, 1f) };
                return;
            }

            var newValues = new List<(Tier tier, float start, float end)>();

            int length = values.Length;
            int max = values.Max();

            Tier lastTier = Tier.None;
            (Tier tier, float start, float end) lowSegment = (Tier.None, 0, 0);
            (Tier tier, float start, float end) midSegment = (Tier.None, 0, 0);
            (Tier tier, float start, float end) highSegment = (Tier.None, 0, 0);
            (Tier tier, float start, float end) highestSegment = (Tier.None, 0, 0);

            for (int i = 0; i < length; i++)
            {
                float normalizedValue = values[i] * 1f / max;

                Tier currentTier = normalizedValue switch
                {
                    < 1f / 5 => Tier.Lowest,
                    < 2f / 5 => Tier.Low,
                    < 3f / 5 => Tier.Mid,
                    < 4f / 5 => Tier.High,
                    <= 1f => Tier.Highest,
                    _ => Tier.None
                };

                if (lastTier != currentTier)
                {
                    if (currentTier == Tier.Highest)
                        highestSegment = (currentTier, i / (length - 1f), 0);

                    if (currentTier < Tier.Highest)
                    {
                        if (highestSegment.tier != Tier.None)
                        {
                            highestSegment.end = i / (length - 1f);
                            newValues.Add(highestSegment);
                            highestSegment = (Tier.None, 0, 0);
                        }
                    }

                    if (currentTier < Tier.High)
                    {
                        if (highSegment.tier != Tier.None)
                        {
                            highSegment.end = i / (length - 1f);
                            newValues.Add(highSegment);
                            highSegment = (Tier.None, 0, 0);
                        }
                    }

                    if (currentTier < Tier.Mid)
                    {
                        if (midSegment.tier != Tier.None)
                        {
                            midSegment.end = i / (length - 1f);
                            newValues.Add(midSegment);
                            midSegment = (Tier.None, 0, 0);
                        }
                    }

                    if (currentTier < Tier.Low)
                    {
                        if (lowSegment.tier != Tier.None)
                        {
                            lowSegment.end = i / (length - 1f);
                            newValues.Add(midSegment);
                            lowSegment = (Tier.None, 0, 0);
                        }
                    }

                    switch (currentTier)
                    {
                        case Tier.Low:
                            lowSegment = (currentTier, i / (length - 1f), 0);
                            break;

                        case Tier.Mid:
                            midSegment = (currentTier, i / (length - 1f), 0);
                            break;

                        case Tier.High:
                            highestSegment = (currentTier, i / (length - 1f), 0);
                            break;

                        case Tier.Highest:
                            highestSegment = (currentTier, i / (length - 1f), 0);
                            break;
                    }
                }

                lastTier = currentTier;

                normalizedSegments = newValues.ToArray();
            }
        }

        private void redrawSegments(BufferedContainer container)
        {
            float x = 0;

            if (normalizedSegments.Length == 0)
                return;

            foreach ((Tier tier, float start, float end) segment in normalizedSegments)
            {
                if (segment.tier is Tier.Low or Tier.None) continue;
                if (x > DrawWidth) continue;

                float width = (segment.end - segment.start) * DrawWidth;

                if (x + width >= DrawWidth)
                    width = DrawWidth - x;

                Colour4 segmentColour = segment.tier switch
                {
                    Tier.Lowest => LowestSegmentColour,
                    Tier.Low => LowSegmentColour,
                    Tier.Mid => MidSegmentColour,
                    Tier.High => HighSegmentColour,
                    Tier.Highest => HighestSegmentColour,
                    _ => throw new ArgumentOutOfRangeException(nameof(segment.tier))
                };

                container.Add(new Box
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Y,
                    Position = new Vector2(x + segment.start * DrawWidth, 0),
                    Width = width,
                    Colour = segmentColour
                });

                x += width;
            }
        }

        private enum Tier
        {
            Lowest = 0,
            Low = 1,
            Mid = 2,
            High = 3,
            Highest = 4,
            None = -1
        }
    }
}
