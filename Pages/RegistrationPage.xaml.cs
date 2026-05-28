using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WpfTali.Pages;

namespace WpfTali
{
    public partial class RegistrationPage : Page
    {
        List<Gender> gList = new List<Gender>();
        Apiservice apiservice = new Apiservice();

        public RegistrationPage()
        {
            InitializeComponent();
            LoadGenderData();
        }

        public async void LoadGenderData()
        {
            try
            {
                gList = await apiservice.GetAllGender();
                if (gList != null)
                {
                    List<string> genderNames = gList.Select(x => x.Gender_name).ToList();
                    GenderComboBox.ItemsSource = genderNames;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading genders: " + ex.Message);
            }
        }

        private async void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            bool isin = true;
            if (!Regex.IsMatch(Email.Text, @"^[a-zA-Z0-9]+@[a-zA-Z0-9]+\.[a-zA-Z]+[a-zA-Z]+$"))
            {
                MessageBox.Show("Invalid email format. Please enter a valid email address.");
                isin = false;
            }
            if (!Regex.IsMatch(telephone.Text, @"^05\d{8}$"))
            {
                    MessageBox.Show("Invalid phone number format. It should start with '05' and be followed by 8 digits.");
                    isin = false;
            }
            if (!Regex.IsMatch(idNum.Text, @"^\d{5,9}$"))
            {
                MessageBox.Show("Invalid Id number format. It should be between 5 and 9 digits.");
                isin = false;
            }

            if (GenderComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select your gender");
                isin = false;
                return;
            }

            var selectedUserTypeItem = UserTypeComboBox.SelectedItem as ComboBoxItem;
            if (selectedUserTypeItem == null)
            {
                MessageBox.Show("Please select kind of user");
                isin = false;
                return;
            }
            if (isin)
            {
                string selectedUserType = selectedUserTypeItem.Content.ToString();
                // 2. איסוף נתונים מהשדות
                string firstName = pName.Text;
                string lastName = lName.Text;
                string email = Email.Text;
                string userName = UserName.Text;
                string phone = telephone.Text;
                string password = Pass.Password;
                string idCard = idNum.Text;
                DateTime? bDate = bornDate.SelectedDate;

                if (!bDate.HasValue)
                {
                    MessageBox.Show("Please select your birth date");
                    return;
                }
                Gender selectedGenderObj = gList[GenderComboBox.SelectedIndex];
                try
                {
                    switch (selectedUserType)
                    {
                        case "Trainee":
                            // הבאת מנוי בסיסי עבור מתאמן חדש
                            List<Subscription> subscriptions = await apiservice.GetAllSubscription();
                            Subscription selectedSub = subscriptions?.Find(s => s.Name_of_sub == "Basic");

                            Trainee trainee = new Trainee()
                            {
                                First_name = firstName,
                                Last_name = lastName,
                                Email = email,
                                Telephone = phone,
                                User_name = userName,
                                Pass = password,
                                Id_gender = selectedGenderObj,
                                Born_date = bDate.Value.Date,
                                Num_id = idCard,
                                Health_Declaration = false,
                                Joining_date = DateTime.Now.Date,
                                Photo = "",
                                Id_Sub = selectedSub
                            };

                            await apiservice.InsertATrainee(trainee);
                            MessageBox.Show("Trainee registration successful!");
                            NavigationService.Navigate(new LoginPage());
                            break;

                        case "Trainer":
                            Trainer trainer = new Trainer()
                            {
                                First_name = firstName,
                                Last_name = lastName,
                                Email = email,
                                Telephone = phone,
                                User_name = userName,
                                Pass = password,
                                Id_gender = selectedGenderObj,
                                Born_date = bDate.Value.Date,
                                Num_id = idCard
                            };

                            await apiservice.InsertATrainer(trainer);
                            MessageBox.Show("Trainer registration successful!");
                            NavigationService.Navigate(new LoginPage());
                            break;

                        case "Manager":
                            Manager manager = new Manager()
                            {
                                First_name = firstName,
                                Last_name = lastName,
                                Email = email,
                                Telephone = phone,
                                User_name = userName,
                                Pass = password,
                                Id_gender = selectedGenderObj,
                                Born_date = bDate.Value.Date,
                                Num_id = idCard
                            };

                            await apiservice.InsertAManager(manager);
                            MessageBox.Show("Manager registration successful!");
                            NavigationService.Navigate(new LoginPage());
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Registration failed: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please fill in all required fields.");
            }
        }
        private void LogInButton_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new LoginPage());
        private void AboutUsButton_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new AboutUsPage());

       
    }
}