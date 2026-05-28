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
using Service;
using Model;

namespace WpfTali
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        public async Task CheckUser()
        {
            Apiservice api = new Apiservice();
            PersonList pl;
            TraineeList tl;
            TrainerList trl;
            ManagerList managers;
            AppComment.Visibility = Visibility.Visible;
            AppComment.Text = "Thinking about this...";
            AppComment.Foreground = Brushes.Green;
            try
            {
                pl = await api.GetAllPerson();
                tl = await api.GetAllTrainee();
                trl = await api.GetAllTrainer();
                managers = await api.GetAllManager();
            }
            catch (Exception ex)
            {
                RunError("Could not connect to the server!");
                return;
            }

            Person person = pl.FirstOrDefault(p =>
                p.Email == emailText.Text &&
                p.Pass == passwordText.Password);
            AppComment.Visibility = Visibility.Collapsed;
            if (person == null)
            {
                RunError("User doesn't exist!");
                return;
            }
            Trainee trainee = tl.FirstOrDefault(t => t.Id == person?.Id);
            Trainer trainer = trl.FirstOrDefault(tr => tr.Id == person?.Id);
            Manager manager = managers.FirstOrDefault(manager => manager.Id == person?.Id);
            if (trainee != null)
            {
                NavigationService.Navigate(new HomePageTe(trainee));
            }
            else if (trainer != null)
            {
                NavigationService.Navigate(new HomePageTr(trainer));
            }
            else if (manager != null)
            {
                NavigationService.Navigate(new HomePageMa(manager));
            }
            else
            {
                RunError("User is neither Trainee nor Trainer nor manager!");
                return;
            }
        }
        public void RunError(string errorMsg)
        {
            AppComment.Visibility = Visibility.Visible;
            AppComment.Text = errorMsg;
            AppComment.Foreground = Brushes.Red;
        }
        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            await CheckUser();
            if (AppComment.Visibility == Visibility.Visible)
                return;
        }
        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegistrationPage());
        }

        //private void Skip(object sender, RoutedEventArgs e)
        //{
        //    emailText.Text = "Omer.Sher2@gmail.com";
        //    passwordText.Password = "Fals637Om";
        //}
    }
}
