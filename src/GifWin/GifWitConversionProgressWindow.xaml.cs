using System;
using System.Windows;
using GifWin.ViewModels;

namespace GifWin
{
    /// <summary>
    /// Interaction logic for GifWitConversionProgressWindow.xaml
    /// </summary>
    public partial class GifWitConversionProgressWindow : Window
    {
        public GifWitConversionProgressWindow()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            ((GifWitConversionProgressViewModel)DataContext).ConvertLibrary();
            base.OnContentRendered(e);
        }
    }
}
