using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace GifWin
{
    internal abstract class ValueTriggerBase<T>
        : StateTriggerBase
    {
        public static readonly DependencyProperty Value =
            DependencyProperty.Register("Value", typeof(T), typeof(ValueTriggerBase<>), new PropertyMetadata(default(T)));
    }

    internal class BoolStateTrigger
        : StateTriggerBase
    {
        
    }
}
