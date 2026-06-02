using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WpfTali.UserControls
{
    public partial class UserControlTrainerW : UserControl
    {
        private bool isEditMode = false;
        private Apiservice apiService = new Apiservice();
        private Trainer currentTrainer;
        public ObservableCollection<WorkoutRowItem> WorkoutsRows { get; set; } = new ObservableCollection<WorkoutRowItem>();
        public List<Kinds_of_workouts> AvailableKinds { get; set; } = new List<Kinds_of_workouts>();
        private List<int> relationsToDelete = new List<int>();
        public UserControlTrainerW(Trainer trainer, List<WorkoutRowItem> initialRows)
        {
            InitializeComponent();
            this.currentTrainer = trainer;
            this.DataContext = this; 
            LoadAvailableKinds();
            UpdateData(initialRows);
        }
        private async void LoadAvailableKinds()
        {
            try
            {
                var kinds = await apiService.GetAllKinds_of_workouts();
                if (kinds != null)
                {
                    AvailableKinds = kinds.Cast<Kinds_of_workouts>().ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading available kinds: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void UpdateData(List<WorkoutRowItem> initialRows)
        {
            WorkoutsRows.Clear();
            relationsToDelete.Clear();

            foreach (var row in initialRows)
            {
                row.ReadVisibility = Visibility.Visible;
                row.EditVisibility = Visibility.Collapsed;
                WorkoutsRows.Add(row);
            }
            ItemsWorkoutsList.ItemsSource = null;
            ItemsWorkoutsList.ItemsSource = WorkoutsRows;
            ImgEditIcon.Source = new BitmapImage(new Uri("https://cdn-icons-png.flaticon.com/512/116/116996.png"));
            BtnAddWorkoutRow.Visibility = Visibility.Collapsed;
            isEditMode = false;
        }
        private async void BtnEditMode_Click(object sender, RoutedEventArgs e)
        {
            if (!isEditMode) 
            {
                isEditMode = true;
                ImgEditIcon.Source = new BitmapImage(new Uri("https://cdn-icons-png.flaticon.com/512/10419/10419616.png")); // אייקון וי / שמירה
                BtnAddWorkoutRow.Visibility = Visibility.Visible;
                foreach (var row in WorkoutsRows)
                {
                    row.ReadVisibility = Visibility.Collapsed;
                    row.EditVisibility = Visibility.Visible;
                }
                ItemsWorkoutsList.Refresh(); 
            }
            else
            {
                try
                {
                    foreach (int idToDelete in relationsToDelete)
                    {
                        await apiService.DeleteAWorkouts_of_trainers(idToDelete);
                    }
                    foreach (var row in WorkoutsRows)
                    {
                        var selectedKind = AvailableKinds.FirstOrDefault(k => k.Id == row.KindId);
                        if (selectedKind == null) continue;
                        selectedKind.Max_amount_of_people = row.MaxPeople;
                        await apiService.UpdateAKinds_of_workouts(selectedKind);

                        Workouts_of_trainers relation = new Workouts_of_trainers
                        {
                            Id = row.RelationId,
                            Id_trainer = this.currentTrainer,
                            Id_kind_of_workouts = selectedKind
                        };

                        if (row.RelationId == 0) 
                        {
                            await apiService.InsertAWorkouts_of_trainers(relation);
                        }
                        else 
                        {
                            await apiService.UpdateAWorkouts_of_trainers(relation);
                        }
                    }

                    MessageBox.Show("הנתונים נשמרו בהצלחה במסד הנתונים!");
                    if (Window.GetWindow(this) is MainWindow mainWindow && mainWindow.MainFrame.Content is HomePageTr homePage)
                    {
                        homePage.RefreshWorkoutsPage();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"שגיאה בעת השמירה לשרת: {ex.Message}");
                }
            }
        }
        private void BtnAddWorkoutRow_Click(object sender, RoutedEventArgs e)
        {
            var newRow = new WorkoutRowItem
            {
                RelationId = 0, // חדש
                KindId = AvailableKinds.Count > 0 ? AvailableKinds[0].Id : 0,
                WorkoutName = "",
                MaxPeople = 10,
                ReadVisibility = Visibility.Collapsed,
                EditVisibility = Visibility.Visible
            };
            WorkoutsRows.Add(newRow);
        }
        private void BtnDeleteWorkout_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is WorkoutRowItem rowItem)
            {
                if (rowItem.RelationId != 0)
                {
                    relationsToDelete.Add(rowItem.RelationId);
                }
                WorkoutsRows.Remove(rowItem);
            }
        }
    }
    public static class ExtensionMethods
    {
        public static void Refresh(this ItemsControl itemsControl)
        {
            var provider = itemsControl.ItemsSource;
            itemsControl.ItemsSource = null;
            itemsControl.ItemsSource = provider;
        }
    }
}