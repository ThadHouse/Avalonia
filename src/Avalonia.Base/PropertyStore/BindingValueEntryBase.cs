using System;
using System.Reactive.Disposables;
using Avalonia.Data;

#nullable enable

namespace Avalonia.PropertyStore
{
    internal abstract class BindingValueEntryBase : IValueEntry, IObserver<object?>, IDisposable
    {
        private readonly IObservable<object?> _source;
        private IDisposable? _bindingSubscription;
        private object? _value = AvaloniaProperty.UnsetValue;

        public BindingValueEntryBase(
            AvaloniaProperty property,
            IObservable<object?> source)
        {
            _source = source;
            Property = property;
        }

        public bool HasValue
        {
            get
            {
                StartIfNecessary();
                return _value != AvaloniaProperty.UnsetValue;
            }
        }
        
        public bool IsActive => true;
        public AvaloniaProperty Property { get; }
        
        public void Dispose()
        {
            _bindingSubscription?.Dispose();
            BindingCompleted();
        }

        public bool TryGetValue(out object? value)
        {
            StartIfNecessary();
            value = _value;
            return HasValue;
        }

        public void OnCompleted() => BindingCompleted();
        public void OnError(Exception error) => BindingCompleted();
        void IObserver<object?>.OnNext(object? value) => SetValue(value);

        protected abstract Type GetOwnerType();

        private void SetValue(object? value)
        {
            if (value == BindingOperations.DoNothing)
                return;

            var accessor = (IStyledPropertyAccessor)Property;

            if (!accessor.ValidateValue(value))
                value = accessor.GetDefaultValue(GetOwnerType());

            _value = BindingNotification.ExtractValue(value);
        }

        private void StartIfNecessary()
        {
            if (_bindingSubscription is null)
            {
                // Prevent reentrancy by first assigning the subscription to a dummy
                // non-null value.
                _bindingSubscription = Disposable.Empty;
                _bindingSubscription = _source.Subscribe(this);
            }
        }

        private void BindingCompleted()
        {
            _bindingSubscription = null;
            SetValue(AvaloniaProperty.UnsetValue);
        }
    }
}
