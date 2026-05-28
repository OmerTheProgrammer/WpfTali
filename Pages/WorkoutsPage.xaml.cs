using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Model;
using Service;

namespace WpfTali.Pages
{
    public partial class WorkoutsPage : Page
    {
        private readonly Apiservice _apiService = new Apiservice();

        private List<Person> _allPeople = new List<Person>();
        private List<Trainee> _allTrainees = new List<Trainee>();
        private List<Kinds_of_workouts> _allKinds = new List<Kinds_of_workouts>();
        private List<List_of_Exc_workouts> _allExcWorkouts = new List<List_of_Exc_workouts>();
        private List<Training_registration> _allRegistrations = new List<Training_registration>();
        private List<Subscription> _allSubscriptions = new List<Subscription>();

        private int currentTraineeId = 1;
        private DateTime _currentMonthDateTime = DateTime.Today;
        public WorkoutsPage()
        {
            InitializeComponent();
            Loaded += WorkoutsPage_Loaded;
        }
        private async void WorkoutsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _allPeople = (await _apiService.GetAllPerson())?.Cast<Person>().ToList() ?? new List<Person>();
                _allTrainees = (await _apiService.GetAllTrainee())?.Cast<Trainee>().ToList() ?? new List<Trainee>();
                _allKinds = (await _apiService.GetAllKinds_of_workouts())?.Cast<Kinds_of_workouts>().ToList() ?? new List<Kinds_of_workouts>();
                _allExcWorkouts = (await _apiService.GetAllList_of_Exc_workouts())?.Cast<List_of_Exc_workouts>().ToList() ?? new List<List_of_Exc_workouts>();
                _allRegistrations = (await _apiService.GetAllTraining_registration())?.Cast<Training_registration>().ToList() ?? new List<Training_registration>();
                _allSubscriptions = (await _apiService.GetAllSubscription())?.Cast<Subscription>().ToList() ?? new List<Subscription>();

                if (_allTrainees != null && _allTrainees.Count > 0)
                {
                    currentTraineeId = _allTrainees.First().Id;
                }
                else
                {
                    MessageBox.Show("שים לב: טבלת המתאמנים ריקה! יש להוסיף מתאמן תחילה.");
                }

                DisplayMonth(_currentMonthDateTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת הנתונים מהשרת: {ex.Message}");
            }
        }
        private void DisplayMonth(DateTime monthDate)
        {
            string hebrewMonth = monthDate.ToString("MMMM yyyy", new System.Globalization.CultureInfo("he-IL"));
            MonthYearTextBlock.Text = hebrewMonth;

            int daysInMonth = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);

            var days = Enumerable.Range(1, daysInMonth).Select(day => {
                var date = new DateTime(monthDate.Year, monthDate.Month, day);
                return new
                {
                    Date = date,
                    DayNumber = day.ToString(),
                    DayOfWeekHebrew = date.ToString("ddd", new System.Globalization.CultureInfo("he-IL"))
                };
            }).ToList();

            DaysListBox.ItemsSource = days;

            if (monthDate.Year == DateTime.Today.Year && monthDate.Month == DateTime.Today.Month)
            {
                DaysListBox.SelectedIndex = DateTime.Today.Day - 1;
            }
            else
            {
                DaysListBox.SelectedIndex = 0;
            }

            if (DaysListBox.SelectedItem != null)
            {
                DaysListBox.ScrollIntoView(DaysListBox.SelectedItem);
            }
        }
        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            _currentMonthDateTime = _currentMonthDateTime.AddMonths(1);
            DisplayMonth(_currentMonthDateTime);
            DetailsPanel.Visibility = Visibility.Collapsed;
        }
        private void PrevMonthButton_Click(object sender, RoutedEventArgs e)
        {
            _currentMonthDateTime = _currentMonthDateTime.AddMonths(-1);
            DisplayMonth(_currentMonthDateTime);
            DetailsPanel.Visibility = Visibility.Collapsed;
        }
        private void DaysListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DaysListBox.SelectedItem == null) return;

            dynamic selectedDay = DaysListBox.SelectedItem;
            DateTime targetDate = selectedDay.Date;

            LoadWorkoutsForDate(targetDate);
            DetailsPanel.Visibility = Visibility.Collapsed;
        }
        private void LoadWorkoutsForDate(DateTime date)
        {
            var dailyWorkouts = _allExcWorkouts
                .Where(w => w.Workout_date.Date == date.Date)
                .Select(w => {
                    var kind = _allKinds.FirstOrDefault(k => k.Id == w.Id_kindOf_workouts.Id);
                    var trainerData = _allPeople.FirstOrDefault(p => p.Id == w.Id_trainer.Id);

                    int bookedCount = _allRegistrations.Count(r => r.Id_excWorkouts != null && r.Id_excWorkouts.Id == w.Id);
                    bool isRegistered = _allRegistrations.Any(r => r.Id_excWorkouts != null && r.Id_trainee != null && r.Id_excWorkouts.Id == w.Id && r.Id_trainee.Id == currentTraineeId);
                    return new WorkoutDisplayItem
                    {
                        RawWorkout = w,
                        WorkoutKind = kind,
                        TrainerPerson = trainerData,
                        CurrentBooked = bookedCount,
                        IsUserRegistered = isRegistered
                    };
                }).OrderBy(w => w.TimeStr).ToList();
            WorkoutsListView.ItemsSource = new ObservableCollection<WorkoutDisplayItem>(dailyWorkouts);
        }
        private void WorkoutsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = WorkoutsListView.SelectedItem as WorkoutDisplayItem;
            if (selectedItem == null) return;

            RefreshDetailsPanel(selectedItem);
            DetailsPanel.Visibility = Visibility.Visible;
        }
        private void RefreshDetailsPanel(WorkoutDisplayItem item)
        {
            if (item == null) return;

            DetailWorkoutName.Text = item.WorkoutName;
            DetailTrainer.Text = $"מאמן/ת: {item.TrainerName}";
            DetailDate.Text = $"תאריך: {item.RawWorkout.Workout_date:dd/MM/yyyy}";
            DetailTime.Text = $"שעה: {item.TimeStr}";
            DetailStatus.Text = $"סטטוס: {item.StatusText}";

            UpdateRegisteredTraineesList(item.RawWorkout.Id);
            UpdateActionButtonState(item);
        }
        private void UpdateRegisteredTraineesList(int workoutId)
        {
            var traineeIds = _allRegistrations
                .Where(r => r.Id_excWorkouts != null && r.Id_excWorkouts.Id == workoutId && r.Id_trainee != null)
                .Select(r => r.Id_trainee.Id)
                .ToList();

            var registeredPeople = _allPeople
                .Where(p => traineeIds.Contains(p.Id))
                .ToList();

            RegisteredTraineesListView.ItemsSource = registeredPeople;
        }
        private void UpdateActionButtonState(WorkoutDisplayItem item)
        {
            // חסימה ויזואלית לאימוני עבר
            if (item.RawWorkout.Workout_date < DateTime.Now)
            {
                ActionButton.Content = "אימון זה כבר עבר";
                ActionButton.Background = Brushes.DarkGray;
                return;
            }

            if (item.IsUserRegistered)
            {
                ActionButton.Content = "ביטול רישום";
                ActionButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
            }
            else if (item.IsFull)
            {
                ActionButton.Content = "כניסה לרשימת המתנה";
                ActionButton.Background = Brushes.Orange;
            }
            else
            {
                ActionButton.Content = "הרשמה לאימון";
                ActionButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1493"));
            }
        }
        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var itemToUpdate = WorkoutsListView.SelectedItem as WorkoutDisplayItem;
            if (itemToUpdate == null) return;
            if (itemToUpdate.RawWorkout.Workout_date < DateTime.Now)
            {
                MessageBox.Show("לא ניתן לבצע פעולות ברישום עבור אימון שכבר עבר.", "האירוע עבר", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var workoutId = itemToUpdate.RawWorkout.Id;
            if (itemToUpdate.IsUserRegistered)
            {
                var registration = _allRegistrations.FirstOrDefault(r => r.Id_excWorkouts != null && r.Id_trainee != null && r.Id_excWorkouts.Id == workoutId && r.Id_trainee.Id == currentTraineeId);
                if (registration != null)
                {
                    int res = await _apiService.DeleteATraining_registration(registration.Id);
                    if (res > 0)
                    {
                        _allRegistrations.Remove(registration);
                        itemToUpdate.CurrentBooked--;
                        itemToUpdate.IsUserRegistered = false;
                        MessageBox.Show("ההרשמה בוטלה בהצלחה.");
                    }
                    else
                    {
                        MessageBox.Show("פעולת הביטול נכשלה בשרת. ודא שהשרת פועל כראוי.");
                    }
                }
            }
            else
            {
                if (!CheckSubscriptionLimit())
                {
                    MessageBox.Show("אזהרה: עברת את מגבלת האימונים השבועית המותרת לפי סוג המנוי שלך!", "מגבלת מנוי", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var currentTraineeObj = _allTrainees.FirstOrDefault(t => t.Id == currentTraineeId);
                var currentWorkoutObj = _allExcWorkouts.FirstOrDefault(w => w.Id == workoutId);
                var newReg = new Training_registration
                {
                    Id_trainee = currentTraineeObj,
                    Id_excWorkouts = currentWorkoutObj
                };
                int res = await _apiService.InsertATraining_registration(newReg);
                if (res > 0)
                {
                    newReg.Id = res;
                    _allRegistrations.Add(newReg);
                    itemToUpdate.CurrentBooked++;
                    itemToUpdate.IsUserRegistered = true;
                    MessageBox.Show(itemToUpdate.IsFull ? "נכנסת לרשימת ההמתנה בהצלחה!" : "נרשמת לאימון בהצלחה!");
                }
                else
                {
                    MessageBox.Show("פעולת ההרשמה נכשלה בשרת. ודא שהשרת פועל כראוי.");
                }
            }
            RefreshDetailsPanel(itemToUpdate);
        }
        private bool CheckSubscriptionLimit()
        {
            var trainee = _allTrainees.FirstOrDefault(t => t.Id == currentTraineeId);
            if (trainee == null || trainee.Id_Sub == null) return true;

            // תיקון: השוואה בין ה-Id של אובייקט ה-Subscription המשויך למתאמן לבין רשימת המנויים הכללית
            var sub = _allSubscriptions.FirstOrDefault(s => s.Id == trainee.Id_Sub.Id);
            if (sub == null) return true;

            int maxAllowedPerWeek = 3;
            if (sub.Name_of_sub.Contains("חופשי חודשי") || sub.Name_of_sub.Contains("ללא הגבלה")) maxAllowedPerWeek = int.MaxValue;
            else if (sub.Name_of_sub.Contains("דו שבועי")) maxAllowedPerWeek = 2;
            else if (sub.Name_of_sub.Contains("חד שבועי")) maxAllowedPerWeek = 1;

            DateTime startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            DateTime endOfWeek = startOfWeek.AddDays(7);

            var bookedThisWeekCount = _allRegistrations.Count(r =>
                r.Id_trainee != null && r.Id_trainee.Id == currentTraineeId &&
                _allExcWorkouts.Any(w => r.Id_excWorkouts != null && w.Id == r.Id_excWorkouts.Id && w.Workout_date >= startOfWeek && w.Workout_date < endOfWeek)
            );

            return bookedThisWeekCount < maxAllowedPerWeek;
        }
    }
}