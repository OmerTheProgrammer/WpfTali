using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Model;
using Service;

namespace WpfTali.Pages
{
    public partial class ManageWorkoutsPage : Page
    {
        private readonly Apiservice _apiService = new Apiservice();
        private List_of_Exc_workouts _selectedWorkout;

        public ManageWorkoutsPage()
        {
            InitializeComponent();
            LoadWorkouts();
        }

        private async void LoadWorkouts()
        {
            try
            {
                var list = await _apiService.GetAllList_of_Exc_workouts();
                WorkoutsDataGrid.ItemsSource = list?.Cast<List_of_Exc_workouts>().ToList();
            }
            catch (Exception ex) { MessageBox.Show($"שגיאה בטעינת אימונים: {ex.Message}"); }
        }

        private void WorkoutsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkoutsDataGrid.SelectedItem is List_of_Exc_workouts workout)
            {
                _selectedWorkout = workout;
                DpDate.SelectedDate = workout.Workout_date;
                TxtTime.Text = workout.Workout_date.ToString("HH:mm");
                TxtKindId.Text = workout.Id_kindOf_workouts?.Id.ToString() ?? "";
                TxtTrainerId.Text = workout.Id_trainer?.Id.ToString() ?? "";
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.GoBack();
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DpDate.SelectedDate == null || !int.TryParse(TxtKindId.Text, out int kindId) || !int.TryParse(TxtTrainerId.Text, out int trainerId))
                {
                    MessageBox.Show("נא למלא תאריך, קוד סוג אימון וקוד מאמן תקינים.");
                    return;
                }
                DateTime baseDate = DpDate.SelectedDate.Value;
                if (!TimeSpan.TryParse(TxtTime.Text, out TimeSpan timeParsed))
                {
                    MessageBox.Show("נא להזין שעה תקינה בפורמט HH:mm (לדוגמה: 15:30)");
                    return;
                }
                DateTime fullDateTime = baseDate.Date + timeParsed;

                var allKinds = await _apiService.GetAllKinds_of_workouts();
                var allTrainers = await _apiService.GetAllTrainer();

                var matchedKind = allKinds?.Cast<Kinds_of_workouts>().FirstOrDefault(k => k.Id == kindId);
                var matchedTrainer = allTrainers?.Cast<Trainer>().FirstOrDefault(t => t.Id == trainerId);

                if (matchedKind == null || matchedTrainer == null)
                {
                    MessageBox.Show("קוד מאמן או קוד סוג אימון לא קיימים במסד הנתונים!");
                    return;
                }

                var newWorkout = new List_of_Exc_workouts
                {
                    Workout_date = fullDateTime, 
                    Id_kindOf_workouts = matchedKind,
                    Id_trainer = matchedTrainer
                };

                if (await _apiService.InsertAList_of_Exc_workouts(newWorkout) > 0)
                {
                    MessageBox.Show("האימון נוסף בהצלחה!");
                    LoadWorkouts();
                    ClearFields();
                }
            }
            catch (Exception ex) { MessageBox.Show($"שגיאה בהוספה: {ex.Message}"); }
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedWorkout == null || DpDate.SelectedDate == null) return;
                if (!int.TryParse(TxtKindId.Text, out int kindId) || !int.TryParse(TxtTrainerId.Text, out int trainerId)) return;

                DateTime baseDate = DpDate.SelectedDate.Value;
                if (!TimeSpan.TryParse(TxtTime.Text, out TimeSpan timeParsed))
                {
                    MessageBox.Show("נא להזין שעה תקינה בפורמט HH:mm");
                    return;
                }
                DateTime fullDateTime = baseDate.Date + timeParsed;

                var allKinds = await _apiService.GetAllKinds_of_workouts();
                var allTrainers = await _apiService.GetAllTrainer();

                var matchedKind = allKinds?.Cast<Kinds_of_workouts>().FirstOrDefault(k => k.Id == kindId);
                var matchedTrainer = allTrainers?.Cast<Trainer>().FirstOrDefault(t => t.Id == trainerId);

                if (matchedKind == null || matchedTrainer == null) return;

                _selectedWorkout.Workout_date = fullDateTime;
                _selectedWorkout.Id_kindOf_workouts = matchedKind;
                _selectedWorkout.Id_trainer = matchedTrainer;

                if (await _apiService.UpdateAList_of_Exc_workouts(_selectedWorkout) > 0)
                {
                    MessageBox.Show("האימון עודכן בהצלחה!");
                    LoadWorkouts();
                    ClearFields();
                }
            }
            catch (Exception ex) { MessageBox.Show($"שגיאה בעדכון: {ex.Message}"); }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWorkout == null) return;
            var res = MessageBox.Show("האם למחוק אימון זה?", "אישור", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes && await _apiService.DeleteAList_of_Exc_workouts(_selectedWorkout.Id) > 0)
            {
                MessageBox.Show("נמחק בהצלחה!");
                LoadWorkouts();
                ClearFields();
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) => ClearFields();
        private void ClearFields()
        {
            _selectedWorkout = null;
            DpDate.SelectedDate = null;
            TxtTime.Clear();
            TxtKindId.Clear();
            TxtTrainerId.Clear();
            WorkoutsDataGrid.SelectedItem = null;
        }
    }
}