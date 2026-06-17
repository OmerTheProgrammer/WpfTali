//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Media;
//using System.Windows.Navigation;
//using Model;
//using Service;

//namespace WpfTali.Pages
//{
//    public partial class WorkoutsPage : Page
//    {
//        private readonly Apiservice _apiService = new Apiservice();

//        private List<Person> _allPeople = new List<Person>();
//        private List<Trainee> _allTrainees = new List<Trainee>();
//        private List<Kinds_of_workouts> _allKinds = new List<Kinds_of_workouts>();
//        private List<List_of_Exc_workouts> _allExcWorkouts = new List<List_of_Exc_workouts>();
//        private List<Training_registration> _allRegistrations = new List<Training_registration>();
//        private List<Subscription> _allSubscriptions = new List<Subscription>();

//        private object _currentUser;
//        private bool _isTrainer = false;
//        private int currentTraineeId = -1;
//        private int currentTrainerId = -1;

//        private DateTime _currentMonthDateTime = new DateTime(2026, 6, 1);

//        public WorkoutsPage(object loggedInUser)
//        {
//            InitializeComponent();
//            _currentUser = loggedInUser;
//            DetermineUserRole();
//            Loaded += WorkoutsPage_Loaded;
//        }

//        private void DetermineUserRole()
//        {
//            if (_currentUser is Trainer trainer)
//            {
//                _isTrainer = true;
//                currentTrainerId = trainer.Id;
//            }
//            else if (_currentUser is Trainee trainee)
//            {
//                _isTrainer = false;
//                currentTraineeId = trainee.Id;
//            }
//        }

//        private async void WorkoutsPage_Loaded(object sender, RoutedEventArgs e)
//        {
//            LoadingOverlay.Visibility = Visibility.Visible;
//            try
//            {
//                await Task.Run(async () =>
//                {
//                    _allPeople = (await _apiService.GetAllPerson())?.Cast<Person>().ToList() ?? new List<Person>();
//                    _allTrainees = (await _apiService.GetAllTrainee())?.Cast<Trainee>().ToList() ?? new List<Trainee>();
//                    _allKinds = (await _apiService.GetAllKinds_of_workouts())?.Cast<Kinds_of_workouts>().ToList() ?? new List<Kinds_of_workouts>();
//                    _allExcWorkouts = (await _apiService.GetAllList_of_Exc_workouts())?.Cast<List_of_Exc_workouts>().ToList() ?? new List<List_of_Exc_workouts>();
//                    _allRegistrations = (await _apiService.GetAllTraining_registration())?.Cast<Training_registration>().ToList() ?? new List<Training_registration>();
//                    _allSubscriptions = (await _apiService.GetAllSubscription())?.Cast<Subscription>().ToList() ?? new List<Subscription>();
//                });

//                if (!_isTrainer && currentTraineeId == -1 && _allTrainees.Count > 0)
//                {
//                    var personUser = _currentUser as Trainee;
//                    if (personUser != null) currentTraineeId = personUser.Id;
//                }

//                ApplyRolePermissions();
//                DisplayMonth(_currentMonthDateTime);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"שגיאה בתקשורת: {ex.Message}");
//            }
//            finally
//            {
//                LoadingOverlay.Visibility = Visibility.Collapsed;
//            }
//        }

//        private void ApplyRolePermissions()
//        {
//            if (_isTrainer)
//            {
//                AddWorkoutButton.Visibility = Visibility.Visible;
//                ActionButton.Visibility = Visibility.Collapsed;
//            }
//            else
//            {
//                AddWorkoutButton.Visibility = Visibility.Collapsed;
//                ActionButton.Visibility = Visibility.Visible;
//                DeleteWorkoutButton.Visibility = Visibility.Collapsed;
//            }
//        }

//        private void BtnBackToHome_Click(object sender, RoutedEventArgs e)
//        {
//            if (NavigationService != null)
//            {
//                if (_isTrainer)
//                {
//                    // המרה של המשתמש הכללי לטיפוס מאמן ומסירתו לעמוד הבית של המאמנים
//                    Trainer currentTrainer = _currentUser as Trainer;
//                    NavigationService.Navigate(new HomePageTr(currentTrainer));
//                }
//                else
//                {
//                    // המרה של המשתמש הכללי לטיפוס מתאמן ומסירתו לעמוד הבית של המתאמנים
//                    Trainee currentTrainee = _currentUser as Trainee;
//                    NavigationService.Navigate(new HomePageTe(currentTrainee));
//                }
//            }
//        }

//        private void DisplayMonth(DateTime monthDate)
//        {
//            string hebrewMonth = monthDate.ToString("MMMM yyyy", new System.Globalization.CultureInfo("he-IL"));
//            MonthYearTextBlock.Text = hebrewMonth;

//            int daysInMonth = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);
//            var days = Enumerable.Range(1, daysInMonth).Select(day => {var date = new DateTime(monthDate.Year, monthDate.Month, day);
//                return new { Date = date, DayNumber = day.ToString(), DayOfWeekHebrew = date.ToString("ddd", new System.Globalization.CultureInfo("he-IL")) };
//            }).ToList();

//            DaysListBox.ItemsSource = days;
//            DaysListBox.SelectedIndex = (monthDate.Year == DateTime.Today.Year && monthDate.Month == DateTime.Today.Month) ? DateTime.Today.Day - 1 : 0;
//        }

//        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
//        {
//            _currentMonthDateTime = _currentMonthDateTime.AddMonths(1);
//            DisplayMonth(_currentMonthDateTime);
//            DetailsPanel.Visibility = Visibility.Collapsed;
//        }

//        private void PrevMonthButton_Click(object sender, RoutedEventArgs e)
//        {
//            _currentMonthDateTime = _currentMonthDateTime.AddMonths(-1);
//            DisplayMonth(_currentMonthDateTime);
//            DetailsPanel.Visibility = Visibility.Collapsed;
//        }

//        private void DaysListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {
//            if (DaysListBox.SelectedItem == null) return;
//            dynamic selectedDay = DaysListBox.SelectedItem;
//            LoadWorkoutsForDate(selectedDay.Date);
//            DetailsPanel.Visibility = Visibility.Collapsed;
//        }

//        private void LoadWorkoutsForDate(DateTime date)
//        {
//            var dailyWorkouts = _allExcWorkouts
//                .Where(w => w.Workout_date.Date == date.Date)
//                .Select(w => {
//                    var kind = _allKinds.FirstOrDefault(k => k.Id == w.Id_kindOf_workouts?.Id);
//                    var trainerData = _allPeople.FirstOrDefault(p => p.Id == w.Id_trainer?.Id);
//                    int bookedCount = _allRegistrations.Count(r => r.Id_excWorkouts != null && r.Id_excWorkouts.Id == w.Id);
//                    bool isRegistered = _allRegistrations.Any(r => r.Id_excWorkouts != null && r.Id_trainee != null && r.Id_excWorkouts.Id == w.Id && r.Id_trainee.Id == currentTraineeId);

//                    return new WorkoutDisplayItem
//                    {
//                        RawWorkout = w,
//                        WorkoutKind = kind,
//                        TrainerPerson = trainerData,
//                        CurrentBooked = bookedCount,
//                        IsUserRegistered = isRegistered
//                    };
//                }).OrderBy(w => w.TimeStr).ToList();

//            WorkoutsListView.ItemsSource = new ObservableCollection<WorkoutDisplayItem>(dailyWorkouts);
//        }

//        private void WorkoutsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {
//            var selectedItem = WorkoutsListView.SelectedItem as WorkoutDisplayItem;
//            if (selectedItem == null) return;
//            RefreshDetailsPanel(selectedItem);
//            DetailsPanel.Visibility = Visibility.Visible;
//        }

//        private void RefreshDetailsPanel(WorkoutDisplayItem item)
//        {
//            if (item == null) return;

//            DetailWorkoutName.Text = item.WorkoutName;
//            DetailTrainer.Text = $"מאמן/ת: {item.TrainerName}";
//            DetailDate.Text = $"תאריך: {item.RawWorkout.Workout_date:dd/MM/yyyy}";
//            DetailTime.Text = $"שעה: {item.TimeStr}";
//            DetailStatus.Text = $"סטטוס: {item.StatusText}";

//            UpdateRegisteredTraineesList(item.RawWorkout.Id);
//            UpdateActionButtonState(item);

//            bool isOwnWorkout = item.RawWorkout.Id_trainer?.Id == currentTrainerId;
//            if (_isTrainer && isOwnWorkout)
//            {
//                DeleteWorkoutButton.Visibility = Visibility.Visible;
//            }
//            else
//            {
//                DeleteWorkoutButton.Visibility = Visibility.Collapsed;
//            }
//        }

//        private void UpdateRegisteredTraineesList(int workoutId)
//        {
//            var activeRegs = _allRegistrations
//                .Where(r => r.Id_excWorkouts != null && r.Id_excWorkouts.Id == workoutId && r.Id_trainee != null)
//                .Select(r => r.Id_trainee.Id)
//                .ToList();

//            var registeredPeople = _allPeople.Where(p => activeRegs.Contains(p.Id)).ToList();

//            RegisteredTraineesListView.ItemsSource = null;
//            RegisteredTraineesListView.ItemsSource = registeredPeople;
//        }

//        private void UpdateActionButtonState(WorkoutDisplayItem item)
//        {
//            if (_isTrainer) return;

//            if (item.RawWorkout.Workout_date < DateTime.Now)
//            {
//                ActionButton.Content = "אימון זה עבר";
//                ActionButton.Background = Brushes.DarkGray;
//                ActionButton.IsEnabled = false;
//                return;
//            }
//            ActionButton.IsEnabled = true;

//            if (item.IsUserRegistered)
//            {
//                ActionButton.Content = "ביטול רישום";
//                ActionButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
//            }
//            else
//            {
//                ActionButton.Content = item.IsFull ? "כניסה להמתנה" : "הרשמה לאימון";
//                ActionButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1493"));
//            }
//        }

//        private async void ActionButton_Click(object sender, RoutedEventArgs e)
//        {
//            var itemToUpdate = WorkoutsListView.SelectedItem as WorkoutDisplayItem;
//            if (itemToUpdate == null) return;

//            LoadingOverlay.Visibility = Visibility.Visible;
//            try
//            {
//                if (itemToUpdate.IsUserRegistered)
//                {
//                    var registration = _allRegistrations.FirstOrDefault(r => r.Id_excWorkouts != null && r.Id_trainee != null && r.Id_excWorkouts.Id == itemToUpdate.RawWorkout.Id && r.Id_trainee.Id == currentTraineeId);
//                    if (registration != null)
//                    {
//                        int res = await _apiService.DeleteATraining_registration(registration.Id);
//                        if (res > 0)
//                        {
//                            _allRegistrations.Remove(registration);
//                            itemToUpdate.CurrentBooked--;
//                            itemToUpdate.IsUserRegistered = false;
//                            MessageBox.Show("הרישום בוטל בהצלחה.");
//                        }
//                    }
//                }
//                else
//                {
//                    var currentTraineeObj = _allTrainees.FirstOrDefault(t => t.Id == currentTraineeId) ?? new Trainee { Id = currentTraineeId };
//                    var currentWorkoutObj = _allExcWorkouts.FirstOrDefault(w => w.Id == itemToUpdate.RawWorkout.Id);

//                    var newReg = new Training_registration { Id_trainee = currentTraineeObj, Id_excWorkouts = currentWorkoutObj };
//                    int res = await _apiService.InsertATraining_registration(newReg);
//                    if (res > 0)
//                    {
//                        newReg.Id = res;
//                        _allRegistrations.Add(newReg);
//                        itemToUpdate.CurrentBooked++;
//                        itemToUpdate.IsUserRegistered = true;
//                        MessageBox.Show("נרשמת בהצלחה!");
//                    }
//                }
//                RefreshDetailsPanel(itemToUpdate);
//            }
//            catch (Exception ex) { MessageBox.Show($"שגיאה בביצוע הפעולה: {ex.Message}"); }
//            finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
//        }

//        private void AddWorkoutButton_Click(object sender, RoutedEventArgs e)
//        {
//            AddWorkoutPopup.IsOpen = true;
//            if (DaysListBox.SelectedItem != null) { dynamic selectedDay = DaysListBox.SelectedItem; DpDate.SelectedDate = selectedDay.Date; }
//            else { DpDate.SelectedDate = DateTime.Today; }
//            TxtTime.Text = "18:00";
//            TxtKindId.Clear();
//            TxtTrainerId.Text = currentTrainerId.ToString();
//        }

//        private void BtnCancelPopup_Click(object sender, RoutedEventArgs e)
//        {
//            AddWorkoutPopup.IsOpen = false;
//        }

//        private async void BtnSaveWorkout_Click(object sender, RoutedEventArgs e)
//        {
//            if (DpDate.SelectedDate == null || string.IsNullOrWhiteSpace(TxtTime.Text) || string.IsNullOrWhiteSpace(TxtKindId.Text))
//            {
//                MessageBox.Show("נא למלא את כל השדות!");
//                return;
//            }

//            if (!int.TryParse(TxtKindId.Text, out int kindId))
//            {
//                MessageBox.Show("קוד סוג האימון חייב להיות מספר.");
//                return;
//            }

//            var selectedKind = _allKinds.FirstOrDefault(k => k.Id == kindId);
//            if (selectedKind == null)
//            {
//                MessageBox.Show($"שגיאה: קוד סוג אימון {kindId} לא קיים במערכת. נא להזין קוד תקין.");
//                return;
//            }

//            AddWorkoutPopup.IsOpen = false;
//            LoadingOverlay.Visibility = Visibility.Visible;

//            try
//            {
//                DateTime selectedDate = DpDate.SelectedDate.Value;
//                string[] timeParts = TxtTime.Text.Split(':');
//                if (timeParts.Length == 2 && int.TryParse(timeParts[0], out int hour) && int.TryParse(timeParts[1], out int minute))
//                {
//                    selectedDate = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, hour, minute, 0);
//                }

//                var currentTrainerObj = _currentUser as Trainer;

//                var newWorkout = new List_of_Exc_workouts
//                {
//                    Workout_date = selectedDate,
//                    Id_kindOf_workouts = selectedKind,
//                    Id_trainer = currentTrainerObj
//                };

//                int newId = await _apiService.InsertAList_of_Exc_workouts(newWorkout);
//                if (newId > 0)
//                {
//                    newWorkout.Id = newId;

//                    newWorkout.Id_kindOf_workouts = selectedKind;
//                    newWorkout.Id_trainer = currentTrainerObj;

//                    _allExcWorkouts.Add(newWorkout);
//                    MessageBox.Show("האימון התווסף בהצלחה למערכת של פיטלי (Fitali)!");

//                    dynamic selectedDay = DaysListBox.SelectedItem;
//                    if (selectedDay != null) LoadWorkoutsForDate(selectedDay.Date);
//                }
//                else
//                {
//                    MessageBox.Show("השרת סירב להוסיף את האימון. ודא שכל הנתונים תקינים.");
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"שגיאה בהוספת אימון: {ex.Message}");
//            }
//            finally
//            {
//                LoadingOverlay.Visibility = Visibility.Collapsed;
//            }
//        }

//        private async void DeleteWorkoutButton_Click(object sender, RoutedEventArgs e)
//        {
//            var itemToDelete = WorkoutsListView.SelectedItem as WorkoutDisplayItem;
//            if (itemToDelete == null) return;

//            if (itemToDelete.RawWorkout.Id_trainer?.Id != currentTrainerId)
//            {
//                MessageBox.Show("אינך מורשה למחוק אימון זה כיוון שהוא משויך למאמן אחר.", "חסימת אבטחה", MessageBoxButton.OK, MessageBoxImage.Stop);
//                return;
//            }

//            var result = MessageBox.Show($"האם אתה בטוח שברצונך למחוק את אימון ה-{itemToDelete.WorkoutName}?", "אישור מחיקה", MessageBoxButton.YesNo, MessageBoxImage.Warning);
//            if (result == MessageBoxResult.Yes)
//            {
//                LoadingOverlay.Visibility = Visibility.Visible;
//                try
//                {
//                    if (await _apiService.DeleteAList_of_Exc_workouts(itemToDelete.RawWorkout.Id) > 0)
//                    {
//                        _allExcWorkouts.Remove(itemToDelete.RawWorkout);
//                        MessageBox.Show("האימון נמחק בהצלחה.");

//                        dynamic selectedDay = DaysListBox.SelectedItem;
//                        if (selectedDay != null) LoadWorkoutsForDate(selectedDay.Date);
//                        DetailsPanel.Visibility = Visibility.Collapsed;
//                    }
//                    else
//                    {
//                        MessageBox.Show("מחיקת האימון נכשלה בשרת.");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show($"שגיאה במחיקה: {ex.Message}");
//                }
//                finally
//                {
//                    LoadingOverlay.Visibility = Visibility.Collapsed;
//                }
//            }
//        }
//    }
//}


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
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

        private object _currentUser;
        private bool _isTrainer = false;
        private int currentTraineeId = -1;
        private int currentTrainerId = -1;

        private DateTime _currentMonthDateTime = new DateTime(2026, 6, 1);

        public WorkoutsPage(object loggedInUser)
        {
            InitializeComponent();
            _currentUser = loggedInUser;
            DetermineUserRole();
            Loaded += WorkoutsPage_Loaded;
        }

        private void DetermineUserRole()
        {
            if (_currentUser is Trainer trainer)
            {
                _isTrainer = true;
                currentTrainerId = trainer.Id;
            }
            else if (_currentUser is Trainee trainee)
            {
                _isTrainer = false;
                currentTraineeId = trainee.Id;
            }
        }

        private async void WorkoutsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                await Task.Run(async () =>
                {
                    _allPeople = (await _apiService.GetAllPerson())?.Cast<Person>().ToList() ?? new List<Person>();
                    _allTrainees = (await _apiService.GetAllTrainee())?.Cast<Trainee>().ToList() ?? new List<Trainee>();
                    _allKinds = (await _apiService.GetAllKinds_of_workouts())?.Cast<Kinds_of_workouts>().ToList() ?? new List<Kinds_of_workouts>();
                    _allExcWorkouts = (await _apiService.GetAllList_of_Exc_workouts())?.Cast<List_of_Exc_workouts>().ToList() ?? new List<List_of_Exc_workouts>();
                    _allRegistrations = (await _apiService.GetAllTraining_registration())?.Cast<Training_registration>().ToList() ?? new List<Training_registration>();
                    _allSubscriptions = (await _apiService.GetAllSubscription())?.Cast<Subscription>().ToList() ?? new List<Subscription>();
                });

                if (!_isTrainer && currentTraineeId == -1 && _allTrainees.Count > 0)
                {
                    var personUser = _currentUser as Trainee;
                    if (personUser != null) currentTraineeId = personUser.Id;
                }

                ApplyRolePermissions();
                DisplayMonth(_currentMonthDateTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בתקשורת: {ex.Message}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void ApplyRolePermissions()
        {
            if (_isTrainer)
            {
                AddWorkoutButton.Visibility = Visibility.Visible;
                ActionButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                AddWorkoutButton.Visibility = Visibility.Collapsed;
                ActionButton.Visibility = Visibility.Visible;
                DeleteWorkoutButton.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnBackToHome_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null)
            {
                if (_isTrainer)
                {
                    Trainer currentTrainer = _currentUser as Trainer;
                    NavigationService.Navigate(new HomePageTr(currentTrainer));
                }
                else
                {
                    Trainee currentTrainee = _currentUser as Trainee;
                    NavigationService.Navigate(new HomePageTe(currentTrainee));
                }
            }
        }

        private void DisplayMonth(DateTime monthDate)
        {
            string hebrewMonth = monthDate.ToString("MMMM yyyy", new System.Globalization.CultureInfo("he-IL"));
            MonthYearTextBlock.Text = hebrewMonth;

            int daysInMonth = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);
            var days = Enumerable.Range(1, daysInMonth).Select(day => {
                var date = new DateTime(monthDate.Year, monthDate.Month, day);
                return new { Date = date, DayNumber = day.ToString(), DayOfWeekHebrew = date.ToString("ddd", new System.Globalization.CultureInfo("he-IL")) };
            }).ToList();

            DaysListBox.ItemsSource = days;
            DaysListBox.SelectedIndex = (monthDate.Year == DateTime.Today.Year && monthDate.Month == DateTime.Today.Month) ? DateTime.Today.Day - 1 : 0;
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
            LoadWorkoutsForDate(selectedDay.Date);
            DetailsPanel.Visibility = Visibility.Collapsed;
        }

        private void LoadWorkoutsForDate(DateTime date)
        {
            var dailyWorkouts = _allExcWorkouts
                .Where(w => w.Workout_date.Date == date.Date)
                .Select(w => {
                    var kind = _allKinds.FirstOrDefault(k => k.Id == w.Id_kindOf_workouts?.Id);
                    var trainerData = _allPeople.FirstOrDefault(p => p.Id == w.Id_trainer?.Id);
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

            bool isOwnWorkout = item.RawWorkout.Id_trainer?.Id == currentTrainerId;
            if (_isTrainer && isOwnWorkout)
            {
                DeleteWorkoutButton.Visibility = Visibility.Visible;
            }
            else
            {
                DeleteWorkoutButton.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateRegisteredTraineesList(int workoutId)
        {
            var activeRegs = _allRegistrations
                .Where(r => r.Id_excWorkouts != null && r.Id_excWorkouts.Id == workoutId && r.Id_trainee != null)
                .Select(r => r.Id_trainee.Id)
                .ToList();

            var registeredPeople = _allPeople.Where(p => activeRegs.Contains(p.Id)).ToList();

            RegisteredTraineesListView.ItemsSource = null;
            RegisteredTraineesListView.ItemsSource = registeredPeople;
        }

        private void UpdateActionButtonState(WorkoutDisplayItem item)
        {
            if (_isTrainer) return;

            if (item.RawWorkout.Workout_date < DateTime.Now)
            {
                ActionButton.Content = "אימון זה עבר";
                ActionButton.Background = Brushes.DarkGray;
                ActionButton.IsEnabled = false;
                return;
            }
            ActionButton.IsEnabled = true;

            if (item.IsUserRegistered)
            {
                ActionButton.Content = "ביטול רישום";
                ActionButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
            }
            else
            {
                ActionButton.Content = item.IsFull ? "כניסה להמתנה" : "הרשמה לאימון";
                ActionButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1493"));
            }
        }

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var itemToUpdate = WorkoutsListView.SelectedItem as WorkoutDisplayItem;
            if (itemToUpdate == null) return;

            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                if (itemToUpdate.IsUserRegistered)
                {
                    var registration = _allRegistrations.FirstOrDefault(r => r.Id_excWorkouts != null && r.Id_trainee != null && r.Id_excWorkouts.Id == itemToUpdate.RawWorkout.Id && r.Id_trainee.Id == currentTraineeId);
                    if (registration != null)
                    {
                        int res = await _apiService.DeleteATraining_registration(registration.Id);
                        if (res > 0)
                        {
                            _allRegistrations.Remove(registration);
                            itemToUpdate.CurrentBooked--;
                            itemToUpdate.IsUserRegistered = false;
                            MessageBox.Show("הרישום בוטל בהצלחה.");
                        }
                    }
                }
                else
                {
                    var currentTraineeObj = _allTrainees.FirstOrDefault(t => t.Id == currentTraineeId) ?? new Trainee { Id = currentTraineeId };
                    var currentWorkoutObj = _allExcWorkouts.FirstOrDefault(w => w.Id == itemToUpdate.RawWorkout.Id);

                    var newReg = new Training_registration { Id_trainee = currentTraineeObj, Id_excWorkouts = currentWorkoutObj };
                    int res = await _apiService.InsertATraining_registration(newReg);
                    if (res > 0)
                    {
                        newReg.Id = res;
                        _allRegistrations.Add(newReg);
                        itemToUpdate.CurrentBooked++;
                        itemToUpdate.IsUserRegistered = true;
                        MessageBox.Show("נרשמת בהצלחה!");
                    }
                }
                RefreshDetailsPanel(itemToUpdate);
            }
            catch (Exception ex) { MessageBox.Show($"שגיאה בביצוע הפעולה: {ex.Message}"); }
            finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
        }

        // עודכן: מאכלס את הקומבו-בוקס החדש בסוגי האימונים שהגיעו מהשרת
        private void AddWorkoutButton_Click(object sender, RoutedEventArgs e)
        {
            AddWorkoutPopup.IsOpen = true;

            CmbWorkoutKind.ItemsSource = _allKinds;
            CmbWorkoutKind.SelectedIndex = -1;

            if (DaysListBox.SelectedItem != null) { dynamic selectedDay = DaysListBox.SelectedItem; DpDate.SelectedDate = selectedDay.Date; }
            else { DpDate.SelectedDate = DateTime.Today; }
            TxtTime.Text = "18:00";
            TxtTrainerId.Text = currentTrainerId.ToString();
        }

        private void BtnCancelPopup_Click(object sender, RoutedEventArgs e)
        {
            AddWorkoutPopup.IsOpen = false;
        }

        // עודכן: שואב את האובייקט וה-ID שלו ישירות מהקומבו בוקס במקום מטקסטבוקס
        private async void BtnSaveWorkout_Click(object sender, RoutedEventArgs e)
        {
            if (DpDate.SelectedDate == null || string.IsNullOrWhiteSpace(TxtTime.Text) || CmbWorkoutKind.SelectedItem == null)
            {
                MessageBox.Show("נא למלא את כל השדות ולבחור סוג אימון!");
                return;
            }

            var selectedKind = CmbWorkoutKind.SelectedItem as Kinds_of_workouts;
            if (selectedKind == null)
            {
                MessageBox.Show("שגיאה בבחירת סוג האימון.");
                return;
            }

            AddWorkoutPopup.IsOpen = false;
            LoadingOverlay.Visibility = Visibility.Visible;

            try
            {
                DateTime selectedDate = DpDate.SelectedDate.Value;
                string[] timeParts = TxtTime.Text.Split(':');
                if (timeParts.Length == 2 && int.TryParse(timeParts[0], out int hour) && int.TryParse(timeParts[1], out int minute))
                {
                    selectedDate = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, hour, minute, 0);
                }

                var currentTrainerObj = _currentUser as Trainer;

                var newWorkout = new List_of_Exc_workouts
                {
                    Workout_date = selectedDate,
                    Id_kindOf_workouts = selectedKind,
                    Id_trainer = currentTrainerObj
                };

                int newId = await _apiService.InsertAList_of_Exc_workouts(newWorkout);
                if (newId > 0)
                {
                    newWorkout.Id = newId;
                    newWorkout.Id_kindOf_workouts = selectedKind;
                    newWorkout.Id_trainer = currentTrainerObj;

                    _allExcWorkouts.Add(newWorkout);
                    MessageBox.Show("האימון התווסף בהצלחה למערכת של פיטלי (Fitali)!");

                    dynamic selectedDay = DaysListBox.SelectedItem;
                    if (selectedDay != null) LoadWorkoutsForDate(selectedDay.Date);
                }
                else
                {
                    MessageBox.Show("השרת סירב להוסיף את האימון. ודא שכל הנתונים תקינים.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בהוספת אימון: {ex.Message}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void DeleteWorkoutButton_Click(object sender, RoutedEventArgs e)
        {
            var itemToDelete = WorkoutsListView.SelectedItem as WorkoutDisplayItem;
            if (itemToDelete == null) return;

            if (itemToDelete.RawWorkout.Id_trainer?.Id != currentTrainerId)
            {
                MessageBox.Show("אינך מורשה למחוק אימון זה כיוון שהוא משויך למאמן אחר.", "חסימת אבטחה", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            var result = MessageBox.Show($"האם אתה בטוח שברצונך למחוק את אימון ה-{itemToDelete.WorkoutName}?", "אישור מחיקה", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                try
                {
                    if (await _apiService.DeleteAList_of_Exc_workouts(itemToDelete.RawWorkout.Id) > 0)
                    {
                        _allExcWorkouts.Remove(itemToDelete.RawWorkout);
                        MessageBox.Show("האימון נמחק בהצלחה.");

                        dynamic selectedDay = DaysListBox.SelectedItem;
                        if (selectedDay != null) LoadWorkoutsForDate(selectedDay.Date);
                        DetailsPanel.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MessageBox.Show("מחיקת האימון נכשלה בשרת.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"שגיאה במחיקה: {ex.Message}");
                }
                using (var task = Task.Delay(1)) // fallback matching finally context logic 
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}