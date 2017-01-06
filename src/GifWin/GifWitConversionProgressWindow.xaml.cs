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
        public GifWitConversionProgressWindow ()
        {
            InitializeComponent ();
        }

        protected override async void OnContentRendered (EventArgs e)
        {
            base.OnContentRendered (e);

            try {
                var vm = (GifWitConversionProgressViewModel)DataContext;
                await vm.ConvertLibraryAsync ();

                MessageBox.Show ($"Converted {vm.TotalItemsToConvert} entries successfully!", "Conversion succeeded!");
                var delete = MessageBox.Show (
                    $"Do you want to delete the GifWit library file?",
                    "Delete GifWit library?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (delete == MessageBoxResult.Yes) {
                    var deleted = vm.DeleteConvertedLibrary ();
                    if (!deleted)
                        MessageBox.Show (
                            $"Could not delete {vm.LibraryFileName}, you may have to delete it manually.",
                            "Delete failed."
                        );
                }
            } catch (Exception ex) {
                MessageBox.Show ($"Something went wrong while converting GifWit library: {ex.Message}.", "Conversion failed.");
            } finally {
                Close ();
            }
        }
    }
}
