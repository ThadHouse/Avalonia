using System;
using Avalonia.Data;
using Avalonia.PropertyStore;

#nullable enable

namespace Avalonia.Styling
{
    internal class PropertySetterBindingInstance : BindingValueEntryBase, ISetterInstance
    {
        private readonly AvaloniaObject _target;

        public PropertySetterBindingInstance(
            AvaloniaObject target,
            AvaloniaProperty property, 
            IObservable<object?> source)
            : base(property, source)
        {
            _target = target;
        }

        protected override Type GetOwnerType() => _target.GetType();
    }
}
