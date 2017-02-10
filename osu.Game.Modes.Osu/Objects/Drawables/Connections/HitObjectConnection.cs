﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects.Drawables;
using System.Collections.Generic;

namespace osu.Game.Modes.Osu.Objects.Drawables.Connections
{
    public abstract class HitObjectConnection : Container
    {
        /// <summary>
        /// Create drawables inside this container, connecting hitobjects visually, for example with follow points.
        /// </summary>
        /// <param name="drawableHitObjects">The drawables hit objects to create connections for</param>
        /// <param name="startIndex">Start index into the drawableHitObjects enumeration.</param>
        /// <param name="endIndex">End index into the drawableHitObjects enumeration. Use -1 to draw connections until the end.</param>
        public abstract void AddConnections(IEnumerable<DrawableHitObject> drawableHitObjects, int startIndex = 0, int endIndex = -1);
    }
}
