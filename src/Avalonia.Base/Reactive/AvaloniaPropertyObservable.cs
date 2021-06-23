using System;

#nullable enable

namespace Avalonia.Reactive
{
    internal class AvaloniaPropertyObservable<T> : LightweightObservableBase<T>, IDescription
    {
        private readonly WeakReference<AvaloniaObject> _target;
        private readonly DirectPropertyBase<T>? _directProperty;
        private readonly StyledPropertyBase<T>? _styledProperty;

        public AvaloniaPropertyObservable(
            AvaloniaObject target,
            StyledPropertyBase<T> property)
        {
            _target = new(target);
            _styledProperty = property;
        }

        public AvaloniaPropertyObservable(
            AvaloniaObject target,
            DirectPropertyBase<T> property)
        {
            _target = new(target);
            _directProperty = property;
        }

        public string Description
        {
            get
            {
                return _styledProperty is object ?
                    $"{_target.GetType().Name}.{_styledProperty.Name}" :
                    $"{_target.GetType().Name}.{_directProperty!.Name}";
            }
        }

        public void OnNext(T value) => PublishNext(value);

        protected override void Subscribed(IObserver<T> observer, bool first)
        {
            if (_target.TryGetTarget(out var target))
            {
                var value = _styledProperty is object ?
                    target.GetValue(_styledProperty) :
                    target.GetValue(_directProperty!);
                observer.OnNext(value!);
            }
        }
    }
}
