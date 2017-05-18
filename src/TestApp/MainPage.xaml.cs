using System;
using System.Linq;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            SQLitePCL.Batteries_V2.Init();

            var db = new GifWin.Core.Data.GifwinDatabase("GifWin.sqlite");
            db.GetGifsByTagAsync("nope").ContinueWith(t => {
                Console.WriteLine(t.Result.Count());
            });
        }
    }
}
