using System;
using Avalonia.Data;
using Avalonia.Logging;
using Avalonia.PropertyStore;

#nullable enable

namespace Avalonia.Styling
{
    internal class PropertySetterBindingInstance : IValueEntry, ISetterInstance, IObserver<object?>
    {
        private static readonly object s_finished = new object();
        private readonly StyleInstance _owner;
        private readonly IBinding _binding;
        private InstancedBinding? _instancedBinding;
        private object? _value = AvaloniaProperty.UnsetValue;

        public PropertySetterBindingInstance(StyleInstance owner, AvaloniaProperty property, IBinding binding)
        {
            _owner = owner;
            _binding = binding;
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

        public AvaloniaProperty Property { get; }

        public bool TryGetValue(out object? value)
        {
            if (_owner.ValueStore is null)
                throw new InvalidOperationException("Cannot get value from unowned BindingValue.");

            if (_value == s_finished)
            {
                value = default;
                return false;
            }

            StartIfNecessary();
            value = _value;
            return value != AvaloniaProperty.UnsetValue;
        }

        void IObserver<object?>.OnCompleted() => _value = s_finished;

        void IObserver<object?>.OnError(Exception error)
        {
            Logger.TryGet(LogEventLevel.Error, LogArea.Property)?.Log(this, "Binding error {Error}", error);
            _value = s_finished;
        }

        void IObserver<object?>.OnNext(object? value)
        {
            var oldValue = _value;
            _value = value;
            _owner.ValueStore?.ValueChanged(_owner, this, oldValue);
        }

        private void StartIfNecessary()
        {
            if (_instancedBinding is null)
            {
                _instancedBinding = _binding.Initiate(_owner.ValueStore!.Owner, Property);
                _instancedBinding.Observable.Subscribe(this);
            }
        }
    }
}
