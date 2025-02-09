// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ExternalLinkStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ExternalLinkOpener";

        /// <summary>
        /// "Are you sure you want to open the following link in a web browser?
        ///
        /// {0}"
        /// </summary>
        public static LocalisableString OpenFollowingLink(string url) => new TranslatableString(getKey(@"open_following_link"), @"Are you sure you want to open the following link in a web browser?

{0}", url);

        /// <summary>
        /// "Open in browser"
        /// </summary>
        public static LocalisableString OpenInBrowser => new TranslatableString(getKey(@"open_in_browser"), @"Open in browser");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
