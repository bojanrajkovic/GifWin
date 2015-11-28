using System;
using System.Windows;
using System.Windows.Controls;

namespace GifWin
{
    static class ReplayBehavior
    {
        public static readonly DependencyProperty RepeatsProperty = DependencyProperty.RegisterAttached (
            "Repeats", typeof (bool), typeof (ReplayBehavior), new PropertyMetadata (default(bool), OnRepeatsChanged));

        public static void SetRepeats (MediaElement element, bool value)
        {
            
            element.SetValue (RepeatsProperty, value);
        }

        static void OnMediaEnded (object sender, RoutedEventArgs routedEventArgs)
        {
            MediaElement element = (MediaElement)sender;
            if (GetRepeats (element)) {
                element.Position = new TimeSpan (0, 0, 1);
                element.Play ();
            }
        }

        public static bool GetRepeats (MediaElement element)
        {
            return (bool) element.GetValue (RepeatsProperty);
        }

        static void OnRepeatsChanged (DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            MediaElement element = (MediaElement)dependencyObject;
            element.MediaEnded -= OnMediaEnded;
            element.MediaEnded += OnMediaEnded;
            if ((bool)e.NewValue)
                element.Play ();
        }
    }
}