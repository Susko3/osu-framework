// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Threading;

namespace osu.Framework.Platform.SDL3.Native
{
    internal interface IHasDependsOnNative
    {
        void DependsOnNative<T>(IBindable<T> stateStorageBindable);
    }

    internal delegate void SetupNativeDependenciesDelegate(IHasDependsOnNative @this, NativeStateStorage state);

    internal delegate T GetValueDelegate<T>(IReadOnlyNativeState state);

    internal class Magic<T>(Bindable<T>? newBindable = null) : IHasDependsOnNative
    {
        public required SetupNativeDependenciesDelegate SetupDependencies { private get; set; }

        public required GetValueDelegate<T> GetValue { private get; init; }

        private readonly Bindable<T> bindable = newBindable ?? new Bindable<T>();

        protected Bindable<T> GetBindableUnsafe() => bindable;

        public IBindable<T> Bindable => bindable;

        private BaseSDL3Window? window;

        void IHasDependsOnNative.DependsOnNative<U>(IBindable<U> nativeStateBindable) => window!.DependsOnNative(UpdateValue, nativeStateBindable);

        public void SetupNativeDependencies(BaseSDL3Window window, NativeStateStorage nativeState)
        {
            Debug.Assert(SetupDependencies != null);

            this.window = window;
            SetupDependencies(this, nativeState);
            this.window = null;

            SetupDependencies = null!;
        }

        public virtual void UpdateValue(IReadOnlyNativeState state)
        {
            ThreadSafety.EnsureInputThread();
            bindable.Value = GetValue(state);
        }
    }

    internal class SettableMagic<T>(Bindable<T>? newBindable = null) : Magic<T>(newBindable)
    {
        public T? Pending { get; private set; }

        public T CurrentOrPending => Pending ?? Bindable.Value;

        public event Action? OnUpdate;

        public void SetPending(T value)
        {
            ThreadSafety.EnsureInputThread();
            Pending = value;
            OnUpdate?.Invoke(); // this will schedule perform native updates in NewSDL3Window
        }

        public void ClearPending()
        {
            ThreadSafety.EnsureInputThread();
            Pending = default;
        }
    }

    internal class SettableTroughBindableMagic<T>(Bindable<T>? newBindable = null) : SettableMagic<T>(newBindable)
    {
        private bool updatingValue;

        public void SetupScheduling(Scheduler commandScheduler)
        {
            Bindable.BindValueChanged(e =>
            {
                if (updatingValue)
                    return;

                commandScheduler.AddOnce(SetPending, e.NewValue);
                Bindable.Value = e.OldValue; // TODO: maybe don't set back old value
            });
        }

        public new Bindable<T> Bindable => GetBindableUnsafe();

        public override void UpdateValue(IReadOnlyNativeState state)
        {
            updatingValue = true;
            base.UpdateValue(state);
            updatingValue = false;
        }
    }

    internal class WriteOnlyBindableMagic<T>(Bindable<T>? newBindable = null)
    {
        public readonly Bindable<T> Bindable = newBindable ?? new Bindable<T>();

        public T? Pending { get; private set; }

        public T CurrentOrPending => Pending ?? Bindable.Value;

        public event Action? OnUpdate;

        public void SetPending(T value)
        {
            ThreadSafety.EnsureInputThread();
            Pending = value;
            OnUpdate?.Invoke(); // this will schedule perform native updates in NewSDL3Window
        }

        public void ClearPending()
        {
            ThreadSafety.EnsureInputThread();
            Pending = default;
        }

        public void SetupScheduling(Scheduler commandScheduler)
        {
            Bindable.BindValueChanged(e =>
            {
                commandScheduler.AddOnce(SetPending, e.NewValue);
            });
        }
    }
}
