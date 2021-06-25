using Avalonia.Data;
using Avalonia.PropertyStore;
using Avalonia.Styling.Activators;

#nullable enable

namespace Avalonia.Styling
{
    /// <summary>
    /// Stores state for a <see cref="Style"/> that has been instanced on a control.
    /// </summary>
    /// <remarks>
    /// <see cref="StyleInstance"/> implements the <see cref="IValueFrame"/> interface meaning that
    /// it is injected directly into the value store of an <see cref="AvaloniaObject"/>. Depending
    /// on the setters present on the style, it may be possible to share a single style instance
    /// among all controls that the style is applied to; meaning that a single style instance can
    /// apply to multiple controls.
    /// </remarks>
    internal class StyleInstance : ValueFrameBase, IStyleInstance, IStyleActivatorSink
    {
        private readonly IStyleActivator? _activator;
        private bool _isActivatorSubscribed;
        private bool _isActive;

        public StyleInstance(IStyle style, IStyleActivator? activator)
        {
            _activator = activator;
            _isActive = activator is null;
            Priority = activator is object ? BindingPriority.StyleTrigger : BindingPriority.Style;
            Source = style;
        }

        public override bool IsActive
        {
            get
            {
                if (_activator is object && !_isActivatorSubscribed)
                {
                    _isActivatorSubscribed = true;
                    _activator.Subscribe(this);
                }

                return _isActive;
            }
        }

        public override BindingPriority Priority { get; }
        public IStyle Source { get; }
        public ValueStore? ValueStore { get; private set; }

        public new void Add(IValueEntry value) => base.Add(value);
        public override void SetOwner(ValueStore? owner) => ValueStore = owner;

        void IStyleActivatorSink.OnNext(bool value, int tag)
        {
            if (_isActive != value)
            {
                _isActive = value;
                ValueStore?.FrameActivationChanged(this);
            }
        }
    }
}
