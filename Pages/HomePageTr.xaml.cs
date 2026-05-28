
using Microsoft.Win32;
using Model;
using Service; 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfTali.Pages;
using WpfTali.UserControls;

namespace WpfTali
{
    public partial class HomePageTr : Page
    {
        public static Trainer currentTrainer = null;
        private Apiservice apiService = new Apiservice();
        public HomePageTr()
        {
            InitializeComponent();
            this.Loaded += HomePageTr_Loaded; 
        }
        public HomePageTr(Trainer trainer)
        {
            InitializeComponent();
            currentTrainer = trainer;
            this.Loaded += HomePageTr_Loaded; 
        }

        private async void HomePageTr_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();        
            LoadTrainerWorkouts();  
        }
        private async Task LoadData()
        {
            if (currentTrainer == null)
                return;

            lblWelcome.Text = $"Welcome Back dear {currentTrainer.User_name}!";
            try
            {
                string base64Photo = await apiService.GetPersonPhotoByte64(currentTrainer.Id);

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
            ContantDetailsInform contact =
                           new ContantDetailsInform
                           {
                               ContactfirstName = currentTrainer.First_name,
                               ContactlastName = currentTrainer.Last_name,
                               Contactemail = currentTrainer.Email,
                               Contactphone = currentTrainer.Telephone,
                               Contactborn = currentTrainer.Born_date,
                               Contactgender = currentTrainer.Id_gender?.Gender_name ?? "Not Specified",
                               Contactpassword = currentTrainer.Pass,
                               ContactidCard = currentTrainer.Num_id
                           };

            UserControlinformation userControl =
                new UserControlinformation(contact);
            userControl.MyEvent += UserControlinformation_EventHandler;
            spContacts.Children.Add(userControl);
        }
        private async void btnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            if (currentTrainer == null) return;
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
                    currentTrainer.Photo = base64String;

                    int result = await apiService.UpdateATrainer(currentTrainer);

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

        private async void LoadTrainerWorkouts()
        {
            if (currentTrainer == null || workContacts == null)
                return;

            workContacts.Children.Clear();
            try
            {
                Workouts_of_trainersList allWorkoutsRelations = await apiService.GetAllWorkouts_of_trainers();

                if (allWorkoutsRelations == null)
                    return;

                var trainerWorkouts = allWorkoutsRelations
                    .Where(w => w.Id_trainer != null && w.Id_trainer.Id == currentTrainer.Id)
                    .ToList();

                List<WorkoutRowItem> finalWorkoutsList = new List<WorkoutRowItem>();

                foreach (var workoutRelation in trainerWorkouts)
                {
                    if (workoutRelation.Id_kind_of_workouts != null)
                    {
                        WorkoutRowItem row = new WorkoutRowItem
                        {
                            RelationId = workoutRelation.Id, 
                            KindId = workoutRelation.Id_kind_of_workouts.Id, 
                            WorkoutName = workoutRelation.Id_kind_of_workouts.Name_of_workout,
                            MaxPeople = workoutRelation.Id_kind_of_workouts.Max_amount_of_people
                        };

                        finalWorkoutsList.Add(row);
                    }
                }
                UserControlTrainerW workoutUC = new UserControlTrainerW(currentTrainer, finalWorkoutsList);
                workContacts.Children.Add(workoutUC);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בקבלת נתוני אימונים מהשרת: {ex.Message}");
            }
        }
        public void RefreshWorkoutsPage()
        {
            LoadTrainerWorkouts();
        }

        private void UserControlinformation_EventHandler(object sender, EventArgs e)
        {
            if (sender is UserControlinformation spcificUserControl)
            {
                ContantDetailsInform data = spcificUserControl.ContactData;
                MessageBox.Show($"Profile of: {data.ContactfirstName} {data.ContactlastName}");
            }
        }
        private void Workouts(object sender, RoutedEventArgs e) => NavigationService.Navigate(new WorkoutsPage());
        private void MoreInfo(object sender, RoutedEventArgs e) => NavigationService.Navigate(new PageMoreInformation(currentTrainer));
    }
}



       

        
