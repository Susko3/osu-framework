// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Platform.SDL3.Native;
using osu.Framework.Threading;

namespace osu.Framework.Platform.SDL3
{
    internal abstract class NotQuiteBaseSDL3Window : BaseSDL3Window
    {
        protected readonly Scheduler EventScheduler = new Scheduler();
        protected readonly Scheduler CommandScheduler = new Scheduler();
        private readonly List<UpdateDerivedStateDelegate> scheduledDerivedUpdates = [];
        private readonly List<UpdateNativeStateDelegate> scheduledNativeUpdates = [];
        protected bool UpdatingDerivedState { get; private set; }

        protected void SetupNativeDependencies()
        {
            SetupNativeDependencies(UnsafeGetStateStorage());
        }

        protected abstract void SetupNativeDependencies(NativeStateStorage nativeState);

        /// <remarks>Should only be called inside <see cref="SetupNativeDependencies(NativeStateStorage)"/></remarks>
        public void DependsOnNative<T>(UpdateDerivedStateDelegate updateMethod, IBindable<T> stateStorageBindable)
        {
            stateStorageBindable.BindValueChanged(_ =>
            {
                lock (scheduledDerivedUpdates)
                {
                    if (!scheduledDerivedUpdates.Contains(updateMethod))
                        scheduledDerivedUpdates.Add(updateMethod);
                }
            });
        }

        protected void UpdateDerivedState()
        {
            lock (scheduledDerivedUpdates)
            {
                UpdatingDerivedState = true;

                foreach (var task in scheduledDerivedUpdates)
                    task(UnsafeGetStateStorage());

                scheduledDerivedUpdates.Clear();

                UpdatingDerivedState = false;
            }
        }

        protected void UpdateNativeState()
        {
            lock (scheduledNativeUpdates)
            {
                foreach (var task in scheduledNativeUpdates)
                    task(UnsafeGetWriteableState());

                scheduledNativeUpdates.Clear();
            }
        }

        protected void ScheduleNativeStateUpdate(UpdateNativeStateDelegate update)
        {
            lock (scheduledNativeUpdates)
            {
                if (!scheduledNativeUpdates.Contains(update))
                    scheduledNativeUpdates.Add(update);
            }
        }

        protected void DependsOnDerived<T>(UpdateNativeStateDelegate updateMethod, IBindable<T> windowBindable)
        {
            windowBindable.BindValueChanged(_ =>
            {
                if (UpdatingDerivedState)
                    return;

                ScheduleNativeStateUpdate(updateMethod);
            }, true);
        }
    }

    public delegate void UpdateDerivedStateDelegate(IReadOnlyNativeState state);

    public delegate void UpdateNativeStateDelegate(IWriteOnlyNativeState newState);
}
