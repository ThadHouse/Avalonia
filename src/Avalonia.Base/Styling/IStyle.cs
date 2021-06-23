using System;
using System.Collections.Generic;

#nullable enable

namespace Avalonia.Styling
{
    /// <summary>
    /// Represents a style that can be applied to a control.
    /// </summary>
    public interface IStyle
    {
        /// <summary>
        /// Gets a collection of child styles.
        /// </summary>
        IReadOnlyList<IStyle> Children { get; }
    }
}
