﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Avalonia.Data;
using Avalonia.Utilities;

#nullable enable

namespace Avalonia.PropertyStore
{
    internal class BindingEntry : IValueEntry,
        IValueFrame,
        IObserver<object?>,
        IList<IValueEntry>,
        IDisposable
    {
        private readonly IObservable<object?> _source;
        private IDisposable? _bindingSubscription;
        private ValueStore? _owner;
        private object? _value;

        public BindingEntry(
            AvaloniaProperty property,
            IObservable<object?> source,
            BindingPriority priority)
        {
            _source = source;
            Property = property;
            Priority = priority;
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
        public BindingPriority Priority { get; }
        public AvaloniaProperty Property { get; }
        public IList<IValueEntry> Values => this;
        int ICollection<IValueEntry>.Count => 1;
        bool ICollection<IValueEntry>.IsReadOnly => true;
        
        IValueEntry IList<IValueEntry>.this[int index] 
        { 
            get => this;
            set => throw new NotImplementedException(); 
        }

        public void Dispose()
        {
            _bindingSubscription?.Dispose();
            BindingCompleted();
        }

        public void SetOwner(ValueStore? owner) => _owner = owner;

        public bool TryGetValue(out object? value)
        {
            StartIfNecessary();
            value = _value;
            return HasValue;
        }

        public void OnCompleted() => BindingCompleted();
        public void OnError(Exception error) => BindingCompleted();
        void IObserver<object?>.OnNext(object? value) => SetValue(value);

        int IList<IValueEntry>.IndexOf(IValueEntry item) => throw new NotImplementedException();
        void IList<IValueEntry>.Insert(int index, IValueEntry item) => throw new NotImplementedException();
        void IList<IValueEntry>.RemoveAt(int index) => throw new NotImplementedException();
        void ICollection<IValueEntry>.Add(IValueEntry item) => throw new NotImplementedException();
        void ICollection<IValueEntry>.Clear() => throw new NotImplementedException();
        bool ICollection<IValueEntry>.Contains(IValueEntry item) => throw new NotImplementedException();
        void ICollection<IValueEntry>.CopyTo(IValueEntry[] array, int arrayIndex) => throw new NotImplementedException();
        bool ICollection<IValueEntry>.Remove(IValueEntry item) => throw new NotImplementedException();
        IEnumerator<IValueEntry> IEnumerable<IValueEntry>.GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        private void SetValue(object? value)
        {
            _ = _owner ?? throw new AvaloniaInternalException("BindingEntry has no owner.");

            if (value == BindingOperations.DoNothing)
                return;

            var accessor = (IStyledPropertyAccessor)Property;

            if (!accessor.ValidateValue(value))
                value = accessor.GetDefaultValue(_owner.Owner.GetType());

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
