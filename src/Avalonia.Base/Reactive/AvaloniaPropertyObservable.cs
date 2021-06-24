using System;
using System.Collections.Generic;

#nullable enable

namespace Avalonia.Reactive
{
    internal abstract class AvaloniaPropertyObservable : IObservable<object?>, IDescription
    {
        protected readonly WeakReference<AvaloniaObject> _source;
        protected readonly List<object> _observers = new();

        public AvaloniaPropertyObservable(AvaloniaObject source)
        {
            _source = new(source);
        }

        public abstract string Description { get; }

        public IDisposable Subscribe(IObserver<object?> observer)
        {
            var result = SubscribeCore(observer);
            observer.OnNext(GetValueUntyped());
            return result;
        }

        protected abstract object? GetValueUntyped();

        protected IDisposable SubscribeCore(object observer)
        {
            _observers.Add(observer);
            return new RemoveObserver(this, observer);
        }

        sealed class RemoveObserver : IDisposable
        {
            AvaloniaPropertyObservable _parent;
            object _observer;

            public RemoveObserver(AvaloniaPropertyObservable parent, object observer)
            {
                _parent = parent;
                _observer = observer;
            }

            public void Dispose() => _parent._observers.Remove(_observer);
        }
    }

    internal abstract class AvaloniaPropertyObservable<T> : AvaloniaPropertyObservable, IObservable<T?>
    {
        public AvaloniaPropertyObservable(AvaloniaObject source)
            : base(source)
        {
        }

        public void PublishNext(T value)
        {
            object? boxed = null;
            var hasBoxed = false;

            foreach (var observer in _observers)
            {
                if (observer is IObserver<T> typed)
                    typed.OnNext(value);
                else
                {
                    if (!hasBoxed)
                        boxed = value;
                    ((IObserver<object?>)observer).OnNext(boxed);
                }
            }
        }

        public IDisposable Subscribe(IObserver<T?> observer)
        {
            var result = SubscribeCore(observer);
            observer.OnNext(GetValue());
            return result;
        }

        protected abstract T? GetValue();
        protected override object? GetValueUntyped() => GetValue();
    }

    internal class StyledPropertyObservable<T> : AvaloniaPropertyObservable<T>, IDescription
    {
        private readonly StyledPropertyBase<T> _property;

        public StyledPropertyObservable(
            AvaloniaObject source,
            StyledPropertyBase<T> property)
            : base(source)
        {
            _property = property;
        }

        public override string Description
        {
            get
            {
                if (_source.TryGetTarget(out var source))
                    return $"{source.GetType().Name}.{_property.Name}";
                else
                    return "(dead)";
            }
        }

        public void OnNext(T value) => PublishNext(value);

        protected override T? GetValue()
        {
            if (_source.TryGetTarget(out var source))
                return source.GetValue(_property);
            return default;
        }
    }

    internal class DirectPropertyObservable<T> : AvaloniaPropertyObservable<T>, IDescription
    {
        private readonly DirectPropertyBase<T> _property;

        public DirectPropertyObservable(
            AvaloniaObject source,
            DirectPropertyBase<T> property)
            : base(source)
        {
            _property = property;
        }

        public override string Description
        {
            get
            {
                if (_source.TryGetTarget(out var source))
                    return $"{source.GetType().Name}.{_property.Name}";
                else
                    return "(dead)";
            }
        }

        public void OnNext(T value) => PublishNext(value);

        protected override T? GetValue()
        {
            if (_source.TryGetTarget(out var source))
                return source.GetValue(_property);
            return default;
        }
    }
}
