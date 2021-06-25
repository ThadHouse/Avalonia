using System;
using System.Collections.Generic;

#nullable enable

namespace Avalonia.Styling
{
    public class Styler : IStyler
    {
        public void ApplyStyles(IStyleable target)
        {
            target = target ?? throw new ArgumentNullException(nameof(target));

            if (target is IStyleHost styleHost)
            {
                ApplyStyles(target, styleHost);
            }
        }

        private void ApplyStyles(IStyleable target, IStyleHost host)
        {
            var parent = host.StylingParent;

            if (parent != null)
            {
                ApplyStyles(target, parent);
            }

            if (host.IsStylesInitialized)
            {
                foreach (var style in host.Styles)
                    ApplyStyle(target, style, host);
            }
        }

        private void ApplyStyle(IStyleable target, IStyle style, IStyleHost host)
        {
            if (style is Style s)
            {
                target.ApplyStyle(s, host);
            }
            else if (style is IEnumerable<IStyle> styles)
            {
                foreach (var child in styles)
                {
                    ApplyStyle(target, child, host);
                }
            }
        }
    }
}
