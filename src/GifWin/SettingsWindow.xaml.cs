using System.Windows;
using GalaSoft.MvvmLight.Messaging;

namespace GifWin
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow ()
        {
            InitializeComponent ();
            Messenger.Default.Register<SettingsSaved> (this, s => Close());
        }
    }
}
