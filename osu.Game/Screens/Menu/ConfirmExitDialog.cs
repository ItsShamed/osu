// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Menu
{
    public partial class ConfirmExitDialog : PopupDialog
    {
        private readonly Action onConfirm;
        private readonly Action? onCancel;

        /// <summary>
        /// Construct a new exit confirmation dialog.
        /// </summary>
        /// <param name="onConfirm">An action to perform on confirmation.</param>
        /// <param name="onCancel">An optional action to perform on cancel.</param>
        public ConfirmExitDialog(Action onConfirm, Action? onCancel = null)
        {
            this.onConfirm = onConfirm;
            this.onCancel = onCancel;
        }

        [BackgroundDependencyLoader]
        private void load(INotificationOverlay notifications, LocalisationManager localisation)
        {
            HeaderText = ConfirmExitDialogStrings.DialogTitle;

            Icon = FontAwesome.Solid.ExclamationTriangle;

            if (notifications.HasOngoingOperations)
            {
                string text = $"{localisation.GetLocalisedString(ConfirmExitDialogStrings.OperationsWillBeAborted)}\n\n";

                var ongoingOperations = notifications.OngoingOperations.ToArray();

                foreach (var n in ongoingOperations.Take(10))
                    text += $"{localisation.GetLocalisedString(n.Text)} ({n.Progress:0%})\n";

                if (ongoingOperations.Length > 10)
                    text += $"\n{localisation.GetLocalisedString(ConfirmExitDialogStrings.RemainingOperations(ongoingOperations.Length - 10))}\n";

                text += $"\n{localisation.GetLocalisedString(ConfirmExitDialogStrings.LastChance)}";

                BodyText = text;

                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogDangerousButton
                    {
                        Text = ConfirmExitDialogStrings.ConfirmExit,
                        Action = onConfirm
                    },
                    new PopupDialogCancelButton
                    {
                        Text = CommonStrings.Back,
                        Action = onCancel
                    },
                };
            }
            else
            {
                BodyText = ConfirmExitDialogStrings.LastChance;

                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogOkButton
                    {
                        Text = ConfirmExitDialogStrings.ConfirmExit,
                        Action = onConfirm
                    },
                    new PopupDialogCancelButton
                    {
                        Text = ConfirmExitDialogStrings.CancelExit,
                        Action = onCancel
                    },
                };
            }
        }
    }
}
