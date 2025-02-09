// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ConfirmExitDialogStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ConfirmExitDialog";

        /// <summary>
        /// "Are you sure you want to exit osu!?"
        /// </summary>
        public static LocalisableString DialogTitle => new TranslatableString(getKey(@"dialog_title"), @"Are you sure you want to exit osu!?");

        /// <summary>
        /// "There are currently some background operations which will be aborted if you continue:"
        /// </summary>
        public static LocalisableString OperationsWillBeAborted => new TranslatableString(getKey(@"operations_will_be_aborted"), @"There are currently some background operations which will be aborted if you continue:");

        /// <summary>
        /// "and {0} other operation(s)."
        /// </summary>
        public static LocalisableString RemainingOperations(int count) => new TranslatableString(getKey(@"remaining_operations"), @"and {0} other operation(s).", count);

        /// <summary>
        /// "Last chance to turn back"
        /// </summary>
        public static LocalisableString LastChance => new TranslatableString(getKey(@"last_chance"), @"Last chance to turn back");

        /// <summary>
        /// "Let me out!"
        /// </summary>
        public static LocalisableString ConfirmExit => new TranslatableString(getKey(@"confirm_exit"), @"Let me out!");

        /// <summary>
        /// "Just a little more..."
        /// </summary>
        public static LocalisableString CancelExit => new TranslatableString(getKey(@"cancel_exit"), "Just a little more...");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
