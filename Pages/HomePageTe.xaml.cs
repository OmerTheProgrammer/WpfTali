using Model;
using Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Win32;
using WpfTali.Pages;
using WpfTali.UserControls;

namespace WpfTali
{
    public partial class HomePageTe : Page
    {
        public static Trainee currentTrainee = null;
        private Apiservice apiService = new Apiservice();

        public HomePageTe()
        {
            InitializeComponent();
            this.Loaded += HomePageTe_Loaded;
        }
        public HomePageTe(Trainee trainee)
        {
            InitializeComponent();
            currentTrainee = trainee;
            this.Loaded += HomePageTe_Loaded;
        }

        private async void HomePageTe_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();        
            LoadTraineeWorkouts();  
        }

        private async Task LoadData()
        {
            if (currentTrainee == null)
                return;

            lblWelcome.Text = $"Welcome Back dear {currentTrainee.User_name}!";
            lblJoiningDate.Text = currentTrainee.Joining_date.ToShortDateString();
            try
            {
                string base64Photo = await apiService.GetPersonPhotoByte64(currentTrainee.Id);

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
                ContactfirstName = currentTrainee.First_name,
                ContactlastName = currentTrainee.Last_name,
                Contactemail = currentTrainee.Email,
                Contactphone = currentTrainee.Telephone,
                Contactborn = currentTrainee.Born_date,
                Contactgender = currentTrainee.Id_gender?.Gender_name ?? "Not Specified",
                Contactpassword = currentTrainee.Pass,
                ContactidCard = currentTrainee.Num_id
            };

            UserControlinformation userControl = new UserControlinformation(contact);
            userControl.MyEvent += UserControlinformation_EventHandler;
            spContacts.Children.Add(userControl);
        }
        private async void btnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            if (currentTrainee == null) return;

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
                    currentTrainee.Photo = base64String;
                    int result = await apiService.UpdateATrainee(currentTrainee);

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
        private async void LoadTraineeWorkouts()
        {
            if (currentTrainee == null || traineeWorkoutsPanel == null)
                return;

            traineeWorkoutsPanel.Children.Clear();
            try
            {
                var allRegistrations = (await apiService.GetAllTraining_registration())?.Cast<Training_registration>().ToList() ?? new List<Training_registration>();
                var allExcWorkouts = (await apiService.GetAllList_of_Exc_workouts())?.Cast<List_of_Exc_workouts>().ToList() ?? new List<List_of_Exc_workouts>();
                var allKinds = (await apiService.GetAllKinds_of_workouts())?.Cast<Kinds_of_workouts>().ToList() ?? new List<Kinds_of_workouts>();
                var allPeople = (await apiService.GetAllPerson())?.Cast<Person>().ToList() ?? new List<Person>();
                var myRegs = allRegistrations
                    .Where(r => r.Id_trainee != null && r.Id_trainee.Id == currentTrainee.Id)
                    .ToList();
                List<WorkoutDisplayItem> finalWorkoutsList = allExcWorkouts
                    .Where(w => myRegs.Any(r => r.Id_excWorkouts != null && r.Id_excWorkouts.Id == w.Id))
                    .Where(w => w.Workout_date.Date >= DateTime.Today)
                    .Select(w => {
                        var kind = allKinds.FirstOrDefault(k => k.Id == w.Id_kindOf_workouts?.Id);
                        var trainerPerson = allPeople.FirstOrDefault(p => p.Id == w.Id_trainer?.Id);
                        return new WorkoutDisplayItem
                        {
                            RawWorkout = w,
                            WorkoutKind = kind,
                            TrainerPerson = trainerPerson,
                            IsUserRegistered = true
                        };
                    })
                    .OrderBy(w => w.RawWorkout.Workout_date)
                    .ToList();
                UserControlTraineeW workoutUC = new UserControlTraineeW(finalWorkoutsList);
                traineeWorkoutsPanel.Children.Add(workoutUC);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בקבלת נתוני אימוני המתאמן מהשרת: {ex.Message}");
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
        private void Subscription(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SubscriptionPage(currentTrainee));
        }
        private void Workouts(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new WorkoutsPage(currentTrainee));
        }
        private void AboutUsButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new AboutUsPage());
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new LoginPage());
        }
    }
}