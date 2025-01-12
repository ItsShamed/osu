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

        /// <summary>
        /// "Another player is also watching"
        /// </summary>
        public static LocalisableString OtherSpectatorSingle => new TranslatableString(getKey(@"other_spectator_single"), @"Another player is also watching");

        /// <summary>
        /// "{0} other players are also watching"
        /// </summary>
        public static LocalisableString OtherSpectatorsMultiple(int count) => new TranslatableString(getKey(@"other_spectators_multiple"), @"{0} other players are also watching", count);

        /// <summary>
        /// "Spectator Mode"
        /// </summary>
        public static LocalisableString SpectatorMode => new TranslatableString(getKey(@"spectator_mode"), @"Spectator Mode");

        /// <summary>
        /// "Start Watching"
        /// </summary>
        public static LocalisableString StartWatching => new TranslatableString(getKey(@"start_watching"), @"Start Watching");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
