
using Microsoft.Win32;
using Model;
using Service;
using System;
using System.Collections.Generic;
using System.IO;
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
using ViewModel;
using WpfTali;
using WpfTali.UserControls;

namespace WpfTali
{
    /// <summary>
    /// Interaction logic for HomePageMa.xaml
    /// </summary>
    public partial class HomePageMa : Page
    {
        public static Manager currentManager = null;
        private Apiservice apiService = new Apiservice();
        public HomePageMa()
        {
            InitializeComponent();
            this.Loaded += HomePageMa_Loaded;
        }
        public HomePageMa(Manager manager)
        {
            InitializeComponent();
            currentManager = manager;
            this.Loaded += HomePageMa_Loaded;
        }

        private async void HomePageMa_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();       
        }

        private async Task LoadData()
        {
            if (currentManager  == null)
                return;

            lblWelcome.Text = $"Welcome Back dear {currentManager.User_name}!";

            try
            {
                string base64Photo = await apiService.GetPersonPhotoByte64(currentManager.Id);

                if (!string.IsNullOrEmpty(base64Photo) && !base64Photo.StartsWith("Missing"))
                {
                    base64Photo = base64Photo.Trim().Replace("\"", "").Replace("\\", "");
                    byte[] imageBytes = Convert.FromBase64String(base64Photo);

                    var profileImage = ByteImageConverter.ByteToImage(imageBytes) as System.Windows.Media.Imaging.BitmapSource;

                    if (profileImage != null)
                    {
                        profileImage.Freeze();

                        ImageBrush loadedBrush = new ImageBrush();
                        loadedBrush.ImageSource = profileImage;
                        loadedBrush.Stretch = Stretch.UniformToFill;

                        profileEllipse.Fill = loadedBrush;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WPF Image Load Error]: {ex.Message}");
            }

            spContacts.Children.Clear();

            ContantDetailsInform contact = new ContantDetailsInform
            {
                ContactfirstName = currentManager.First_name,
                ContactlastName = currentManager.Last_name,
                Contactemail = currentManager.Email,
                Contactphone = currentManager.Telephone,
                Contactborn = currentManager.Born_date,
                Contactgender = currentManager.Id_gender?.Gender_name ?? "Not Specified",
                Contactpassword = currentManager.Pass,
                ContactidCard = currentManager.Num_id
            };

            UserControlinformation userControl = new UserControlinformation(contact);
            userControl.MyEvent += UserControlinformation_EventHandler;
            spContacts.Children.Add(userControl);
        }

        private async void btnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            if (currentManager == null) return;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string selectedFilePath = openFileDialog.FileName;
                    byte[] imageBytes = File.ReadAllBytes(selectedFilePath);

                    byte[] resizedBytes = ResizeImage(imageBytes, 300, 300);


                    string base64String = Convert.ToBase64String(resizedBytes);

                    var newImage = ByteImageConverter.ByteToImage(resizedBytes) as System.Windows.Media.Imaging.BitmapSource;
                    if (newImage != null)
                    {

                        newImage.Freeze();

                        ImageBrush newBrush = new ImageBrush();
                        newBrush.ImageSource = newImage;
                        newBrush.Stretch = Stretch.UniformToFill;

                        profileEllipse.Fill = newBrush;
                    }

                    currentManager.Photo = base64String;

                    int result = await apiService.UpdateAManager(currentManager);

                    if (result == 1)
                    {
                        MessageBox.Show("Profile picture updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to save image to server database. check server logs.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error uploading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private byte[] ResizeImage(byte[] imageBytes, int maxWidth, int maxHeight)
        {
            try
            {
                using (var ms = new MemoryStream(imageBytes))
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = maxWidth;
                    bitmap.EndInit();

                    var encoder = new System.Windows.Media.Imaging.JpegBitmapEncoder();
                    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));

                    using (var outMs = new MemoryStream())
                    {
                        encoder.Save(outMs);
                        return outMs.ToArray();
                    }
                }
            }
            catch
            {
                return imageBytes;
            }
        }
        private void UserControlinformation_EventHandler(object sender, EventArgs e)
        {
            if (sender is UserControlinformation specificUserControl)
            {
                ContantDetailsInform data = specificUserControl.ContactData;
                MessageBox.Show($"Profile of: {data.ContactfirstName} {data.ContactlastName}");
            }
        }


        private void BtnWorkouts_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new Pages.ManageWorkoutsPage());
        }
       
        private void BtnSubscription_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new Pages.ManageSubscriptionsPage());
        }

        private void BtnPersonalData_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new Pages.ManagePeoplePage());
        }
      private void AboutUsButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new Pages.AboutUsPage());
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new LoginPage());
        }
    }
}















   

    

