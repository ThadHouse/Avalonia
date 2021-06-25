using System;
using Avalonia.Data;
using Avalonia.PropertyStore;

#nullable enable

namespace Avalonia.Styling
{
    internal class PropertySetterBindingInstance : BindingValueEntryBase, ISetterInstance
    {
        private readonly StyleInstance _instance;

        public PropertySetterBindingInstance(
            StyleInstance instance,
            AvaloniaProperty property, 
            IObservable<object?> source)
            : base(property, source)
        {
            _instance = instance;
        }

        protected override Type GetOwnerType()
        {
            return _instance.ValueStore!.Owner.GetType();
        }

        protected override void ValueChanged(object? oldValue)
        {
            _instance.ValueStore!.ValueChanged(_instance, this, oldValue);
        }
    }
}
