﻿using System;

#nullable enable

namespace Avalonia.Styling
{
    /// <summary>
    /// Represents a <see cref="Style"/> that has been instanced on a control.
    /// </summary>
    public interface IStyleInstance
    {
        /// <summary>
        /// Gets the source style.
        /// </summary>
        IStyle Source { get; }

        /// <summary>
        /// Gets a value indicating whether this style is active.
        /// </summary>
        bool IsActive { get; }
    }
}
