using System;

#nullable enable

namespace Avalonia.Styling
{
    /// <summary>
    /// Represents an <see cref="ISetter"/> that has been instanced on a control.
    /// </summary>
    public interface ISetterInstance
    {
        /// <summary>
        /// Called by the owner <see cref="IStyleInstance"/> when the style is activated on a control.
        /// </summary>
        /// <param name="target">The target control.</param>
        void Activate(IStyleable target);

        /// <summary>
        /// Called by the owner <see cref="IStyleInstance"/> when the style is deactivated on a control.
        /// </summary>
        /// <param name="target">The target control.</param>
        void Dectivate(IStyleable target);
    }
}
