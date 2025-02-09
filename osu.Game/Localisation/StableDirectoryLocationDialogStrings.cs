// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class StableDirectoryLocationDialogStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.StableDirectoryLocationDialog";

        /// <summary>
        /// "Failed to automatically locate an osu!stable installation."
        /// </summary>
        public static LocalisableString FailedToLocateStable => new TranslatableString(getKey(@"failed_to_locate_stable"), @"Failed to automatically locate an osu!stable installation.");

        /// <summary>
        /// "An existing install could not be located. If you know where it is, you can help locate it."
        /// </summary>
        public static LocalisableString FailedToLocateStableDescription => new TranslatableString(getKey(@"failed_to_locate_stable_description"), @"An existing install could not be located. If you know where it is, you can help locate it.");

        /// <summary>
        /// "Sure! I know where it is located!"
        /// </summary>
        public static LocalisableString OkLocate => new TranslatableString(getKey(@"ok_locate"), @"Sure! I know where it is located!");

        /// <summary>
        /// "Actually I don&#39;t have osu!stable installed."
        /// </summary>
        public static LocalisableString CancelNotInstalled => new TranslatableString(getKey(@"cancel_not_installed"), @"Actually I don't have osu!stable installed.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
