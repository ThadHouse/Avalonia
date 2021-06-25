using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Styling
{
    internal class DirectPropertySetterInstance : ISetterInstance
    {
        private Setter _setter;

        public DirectPropertySetterInstance(Setter setter) => _setter = setter;

        public void Activate(IStyleable target)
        {
            target.SetValue(_setter.Property, _setter.Value);
        }

        public void Dectivate(IStyleable target)
        {
            target.ClearValue(_setter.Property);
        }
    }
}
