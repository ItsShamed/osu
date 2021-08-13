﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge
{
    [Cached]
    public abstract class LoungeSubScreen : OnlinePlaySubScreen
    {
        public override string Title => "Lounge";

        protected override UserActivity InitialActivity => new UserActivity.SearchingForLobby();

        protected Container<OsuButton> Buttons { get; } = new Container<OsuButton>
        {
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
            AutoSizeAxes = Axes.Both
        };

        private readonly IBindable<bool> initialRoomsReceived = new Bindable<bool>();
        private readonly IBindable<bool> operationInProgress = new Bindable<bool>();

        private FilterControl filter;
        private LoadingLayer loadingLayer;

        [Resolved]
        private Bindable<Room> selectedRoom { get; set; }

        [Resolved]
        private MusicController music { get; set; }

        [Resolved(CanBeNull = true)]
        private OngoingOperationTracker ongoingOperationTracker { get; set; }

        [CanBeNull]
        private IDisposable joiningRoomOperation { get; set; }

        private RoomsContainer roomsContainer;

        [CanBeNull]
        private LeasedBindable<Room> selectionLease;

        [BackgroundDependencyLoader]
        private void load()
        {
            OsuScrollContainer scrollContainer;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 100,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Top = 20,
                        Left = WaveOverlayContainer.WIDTH_PADDING,
                        Right = WaveOverlayContainer.WIDTH_PADDING,
                    },
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.Absolute, 20)
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 70,
                                    Depth = -1,
                                    Children = new Drawable[]
                                    {
                                        filter = CreateFilterControl(),
                                        Buttons.WithChild(CreateNewRoomButton().With(d =>
                                        {
                                            d.Size = new Vector2(150, 25);
                                            d.Action = () => Open();
                                        }))
                                    }
                                }
                            },
                            null,
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        scrollContainer = new OsuScrollContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            ScrollbarOverlapsContent = false,
                                            Child = roomsContainer = new RoomsContainer()
                                        },
                                        loadingLayer = new LoadingLayer(true),
                                    }
                                },
                            }
                        }
                    },
                }
            };

            // scroll selected room into view on selection.
            selectedRoom.BindValueChanged(val =>
            {
                var drawable = roomsContainer.Rooms.FirstOrDefault(r => r.Room == val.NewValue);
                if (drawable != null)
                    scrollContainer.ScrollIntoView(drawable);
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            initialRoomsReceived.BindTo(RoomManager.InitialRoomsReceived);
            initialRoomsReceived.BindValueChanged(_ => updateLoadingLayer());

            if (ongoingOperationTracker != null)
            {
                operationInProgress.BindTo(ongoingOperationTracker.InProgress);
                operationInProgress.BindValueChanged(_ => updateLoadingLayer(), true);
            }
        }

        protected override void OnFocus(FocusEvent e)
        {
            filter.TakeFocus();
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            onReturning();
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            Debug.Assert(selectionLease != null);

            selectionLease.Return();
            selectionLease = null;

            if (selectedRoom.Value?.RoomID.Value == null)
                selectedRoom.Value = new Room();

            music?.EnsurePlayingSomething();

            onReturning();
        }

        public override bool OnExiting(IScreen next)
        {
            onLeaving();
            return base.OnExiting(next);
        }

        public override void OnSuspending(IScreen next)
        {
            onLeaving();
            base.OnSuspending(next);
        }

        private void onReturning()
        {
            filter.HoldFocus = true;
        }

        private void onLeaving()
        {
            filter.HoldFocus = false;

            // ensure any password prompt is dismissed.
            this.HidePopover();
        }

        public void Join(Room room, string password) => Schedule(() =>
        {
            if (joiningRoomOperation != null)
                return;

            joiningRoomOperation = ongoingOperationTracker?.BeginOperation();

            RoomManager?.JoinRoom(room, password, r =>
            {
                Open(room);
                joiningRoomOperation?.Dispose();
                joiningRoomOperation = null;
            }, _ =>
            {
                joiningRoomOperation?.Dispose();
                joiningRoomOperation = null;
            });
        });

        /// <summary>
        /// Push a room as a new subscreen.
        /// </summary>
        /// <param name="room">An optional template to use when creating the room.</param>
        public void Open(Room room = null) => Schedule(() =>
        {
            // Handles the case where a room is clicked 3 times in quick succession
            if (!this.IsCurrentScreen())
                return;

            OpenNewRoom(room ?? CreateNewRoom());
        });

        protected virtual void OpenNewRoom(Room room)
        {
            selectionLease = selectedRoom.BeginLease(false);
            Debug.Assert(selectionLease != null);
            selectionLease.Value = room;

            this.Push(CreateRoomSubScreen(room));
        }

        protected abstract FilterControl CreateFilterControl();

        protected abstract OsuButton CreateNewRoomButton();

        /// <summary>
        /// Creates a new room.
        /// </summary>
        /// <returns>The created <see cref="Room"/>.</returns>
        protected abstract Room CreateNewRoom();

        protected abstract RoomSubScreen CreateRoomSubScreen(Room room);

        private void updateLoadingLayer()
        {
            if (operationInProgress.Value || !initialRoomsReceived.Value)
                loadingLayer.Show();
            else
                loadingLayer.Hide();
        }
    }
}
