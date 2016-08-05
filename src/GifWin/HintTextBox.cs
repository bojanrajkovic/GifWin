using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GifWin
{
    public class HintTextBox
        : TextBox
    {
        static HintTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata (
                typeof(HintTextBox),
                new FrameworkPropertyMetadata (typeof(HintTextBox)));
        }

        public static readonly DependencyProperty HintProperty = DependencyProperty.Register (
            "Hint", typeof (string), typeof (HintTextBox), new PropertyMetadata (default (string)));

        public string Hint
        {
            get { return (string)GetValue (HintProperty); }
            set { SetValue (HintProperty, value); }
        }

        public static readonly DependencyProperty HintForegroundProperty = DependencyProperty.Register (
            "HintForeground", typeof (Brush), typeof (HintTextBox), new PropertyMetadata (default (Brush)));

        public Brush HintForeground
        {
            get { return (Brush)GetValue (HintForegroundProperty); }
            set { SetValue (HintForegroundProperty, value); }
        }

        protected override void OnGotFocus (RoutedEventArgs e)
        {
            TextBox actual = GetTemplateChild ("actual") as TextBox;
            if (actual == null)
                return;

            actual.Focus ();
        }
    }
}