﻿using System;
using Avalonia.Animation;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.PropertyStore;

#nullable enable

namespace Avalonia.Styling
{
    /// <summary>
    /// A setter for a <see cref="Style"/>.
    /// </summary>
    /// <remarks>
    /// A <see cref="Setter"/> is used to set a <see cref="AvaloniaProperty"/> value on a
    /// <see cref="AvaloniaObject"/> depending on a condition.
    /// </remarks>
    public class Setter : ISetter, IValueEntry, ISetterInstance, IPropertySetter
    {
        private object? _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Setter"/> class.
        /// </summary>
        public Setter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Setter"/> class.
        /// </summary>
        /// <param name="property">The property to set.</param>
        /// <param name="value">The property value.</param>
        public Setter(AvaloniaProperty property, object? value)
        {
            Property = property;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the property to set.
        /// </summary>
        public AvaloniaProperty? Property { get; set; }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Content]
        [AssignBinding]
        [DependsOn(nameof(Property))]
        public object? Value
        {
            get => _value;
            set
            {
                (value as ISetterValue)?.Initialize(this);
                _value = value;
            }
        }

        bool IValueEntry.HasValue => true;
        AvaloniaProperty IValueEntry.Property => EnsureProperty();

        ISetterInstance ISetter.Instance(IStyleInstance instance, IStyleable target)
        {
            _ = Property ?? throw new InvalidOperationException("Setter.Property must be set.");

            if (Value is IBinding binding)
            {
                return new PropertySetterBindingInstance((StyleInstance)instance, Property, binding);
            }
            else if (Value is ITemplate template && !typeof(ITemplate).IsAssignableFrom(Property.PropertyType))
            {
                return new PropertySetterTemplateInstance(Property, template);
            }
            else
            {
                if (!Property.IsValidValue(Value))
                    throw new InvalidCastException($"Setter value '{Value}' is not a valid value for property '{Property}'.");
                return this;
            }
        }

        bool IValueEntry.TryGetValue(out object? value)
        {
            value = Value;
            return true;
        }

        private AvaloniaProperty EnsureProperty()
        {
            return Property ?? throw new InvalidOperationException("Setter.Property must be set.");
        }
    }
}
