using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GifWin
{
    /// <summary>
    /// Interaction logic for SearchTextBox.xaml
    /// </summary>
    public partial class SearchTextBox
    {
        public SearchTextBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HintProperty = DependencyProperty.Register (
            "Hint", typeof (string), typeof (SearchTextBox), new PropertyMetadata (default(string)));

        public string Hint
        {
            get { return (string) GetValue (HintProperty); }
            set { SetValue (HintProperty, value); }
        }

        public static readonly DependencyProperty HintForegroundProperty = DependencyProperty.Register (
            "HintForeground", typeof (Brush), typeof (SearchTextBox), new PropertyMetadata (new SolidColorBrush { Color = Colors.Black, Opacity = 0.5 }));

        public Brush HintForeground
        {
            get { return (Brush) GetValue (HintForegroundProperty); }
            set { SetValue (HintForegroundProperty, value); }
        }

        protected override void OnGotFocus (RoutedEventArgs e)
        {
            TextBox actual = GetTemplateChild ("actual") as TextBox;
            if (actual == null)
                return;

            actual.Focus();
        }
    }
}
