using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GifWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GifWitLibrary lib;
        public MainWindow ()
        {
            InitializeComponent ();
            lib = GifWitLibrary.LoadFromFile ("library.gifwit");
            listBox.DataContext = lib;
        }

        private void textBox_TextChanged (object sender, TextChangedEventArgs e)
        {

        }
    }
}
