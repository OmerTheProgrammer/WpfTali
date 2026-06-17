using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfTali.Pages
{
    /// <summary>
    /// Interaction logic for AboutUsPage.xaml
    /// </summary>
    public partial class AboutUsPage : Page
    {
        public AboutUsPage()
        {
            InitializeComponent();
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            // ניווט חכם חזרה לעמוד האחרון שממנו המשתמש הגיע
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
        }
    }
}