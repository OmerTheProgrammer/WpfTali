using System;
using System.Collections.Generic;
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
using WpfTali.Pages;

namespace WpfTali
{
    /// <summary>
    /// Interaction logic for WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            InitializeComponent();
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "videos", "we_are.mp4");
                BackgroundVideo.Source = new Uri(path, UriKind.Absolute);
                BackgroundVideo.Play();
         }

       private void BackgroundVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
           
            BackgroundVideo.Position = TimeSpan.Zero; BackgroundVideo.Play();
            

        }

        //private void login(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new LoginPage());
        //}

        private void EnterPro(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }
    }

 }

