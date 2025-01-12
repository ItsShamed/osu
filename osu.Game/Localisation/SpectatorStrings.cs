// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class SpectatorStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Spectator";

        /// <summary>
        /// "Spectators ({0})"
        /// </summary>
        public static LocalisableString SpectatorListTitle(int count = 0) => new TranslatableString(getKey(@"spectator_list_title"), @"Spectators ({0})", count);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
