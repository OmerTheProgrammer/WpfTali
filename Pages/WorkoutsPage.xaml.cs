//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
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

//        // משתני המשתמש המחובר כרגע
//        private object _currentUser;
//        private bool _isTrainer = false; // האם המשתמש הנוכחי הוא מאמן
//        private int currentTraineeId = -1;
//        private int currentTrainerId = -1; // מזהה המאמן במידה ונכנס מאמן

//        private DateTime _currentMonthDateTime = DateTime.Today;

//        // עדכון הבנאי לקבלת המשתמש המחובר
//        public WorkoutsPage(object loggedInUser)
//        {
//            InitializeComponent();
//            _currentUser = loggedInUser;

//            // זיהוי סוג המשתמש שקיבלנו מהלוגין
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
//            else
//            {
//                _isTrainer = false;
//            }
//        }

//        private async void WorkoutsPage_Loaded(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                _allPeople = (await _apiService.GetAllPerson())?.Cast<Person>().ToList() ?? new List<Person>();
//                _allTrainees = (await _apiService.GetAllTrainee())?.Cast<Trainee>().ToList() ?? new List<Trainee>();
//                _allKinds = (await _apiService.GetAllKinds_of_workouts())?.Cast<Kinds_of_workouts>().ToList() ?? new List<Kinds_of_workouts>();
//                _allExcWorkouts = (await _apiService.GetAllList_of_Exc_workouts())?.Cast<List_of_Exc_workouts>().ToList() ?? new List<List_of_Exc_workouts>();
//                _allRegistrations = (await _apiService.GetAllTraining_registration())?.Cast<Training_registration>().ToList() ?? new List<Training_registration>();
//                _allSubscriptions = (await _apiService.GetAllSubscription())?.Cast<Subscription>().ToList() ?? new List<Subscription>();

//                if (!_isTrainer && currentTraineeId == -1 && _allTrainees.Count > 0)
//                {
//                    currentTraineeId = _allTrainees.First().Id;
//                }

//                ApplyRolePermissions();

//                DisplayMonth(_currentMonthDateTime);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"שגיאה בטעינת הנתונים מהשרת: {ex.Message}");
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

//        private void DisplayMonth(DateTime monthDate)
//        {
//            string hebrewMonth = monthDate.ToString("MMMM yyyy", new System.Globalization.CultureInfo("he-IL"));
//            MonthYearTextBlock.Text = hebrewMonth;

//            int daysInMonth = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);

//            var days = Enumerable.Range(1, daysInMonth).Select(day => {
//                var date = new DateTime(monthDate.Year, monthDate.Month, day);
//                return new
//                {
//                    Date = date,
//                    DayNumber = day.ToString(),
//                    DayOfWeekHebrew = date.ToString("ddd", new System.Globalization.CultureInfo("he-IL"))
//                };
//            }).ToList();

//            DaysListBox.ItemsSource = days;

//            if (monthDate.Year == DateTime.Today.Year && monthDate.Month == DateTime.Today.Month)
//            {
//                DaysListBox.SelectedIndex = DateTime.Today.Day - 1;
//            }
//            else
//            {
//                DaysListBox.SelectedIndex = 0;
//            }

//            if (DaysListBox.SelectedItem != null)
//            {
//                DaysListBox.ScrollIntoView(DaysListBox.SelectedItem);
//            }
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
//            DateTime targetDate = selectedDay.Date;

//            LoadWorkoutsForDate(targetDate);
//            DetailsPanel.Visibility = Visibility.Collapsed;
//        }

//        private void LoadWorkoutsForDate(DateTime date)
//        {
//            var dailyWorkouts = _allExcWorkouts
//                .Where(w => w.Workout_date.Date == date.Date)
//                .Select(w => {
//                    var kind = _allKinds.FirstOrDefault(k => k.Id == w.Id_kindOf_workouts.Id);
//                    var trainerData = _allPeople.FirstOrDefault(p => p.Id == w.Id_trainer.Id);

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

//            if (_isTrainer && item.RawWorkout.Id_trainer != null && item.RawWorkout.Id_trainer.Id == currentTrainerId)
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
//            var traineeIds = _allRegistrations
//                .Where(r => r.Id_excWorkouts != null && r.Id_excWorkouts.Id == workoutId && r.Id_trainee != null)
//                .Select(r => r.Id_trainee.Id)
//                .ToList();

//            var registeredPeople = _allPeople
//                .Where(p => traineeIds.Contains(p.Id))
//                .ToList();

//            RegisteredTraineesListView.ItemsSource = null;
//            RegisteredTraineesListView.ItemsSource = registeredPeople;
//        }

//        private void UpdateActionButtonState(WorkoutDisplayItem item)
//        {
//            if (item.RawWorkout.Workout_date < DateTime.Now)
//            {
//                ActionButton.Content = "אימון זה כבר עבר";
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
//            else if (item.IsFull)
//            {
//                ActionButton.Content = "כניסה לרשימת המתנה";
//                ActionButton.Background = Brushes.Orange;
//            }
//            else
//            {
//                ActionButton.Content = "הרשמה לאימון";
//                ActionButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1493"));
//            }
//        }

//        private async void ActionButton_Click(object sender, RoutedEventArgs e)
//        {
//            var itemToUpdate = WorkoutsListView.SelectedItem as WorkoutDisplayItem;
//            if (itemToUpdate == null) return;

//            if (itemToUpdate.RawWorkout.Workout_date < DateTime.Now)
//            {
//                MessageBox.Show("לא ניתן לבצע פעולות ברישום עבור אימון שכבר עבר.", "האירוע עבר", MessageBoxButton.OK, MessageBoxImage.Warning);
//                return;
//            }

//            var workoutId = itemToUpdate.RawWorkout.Id;

//            if (itemToUpdate.IsUserRegistered)
//            {
//                var registration = _allRegistrations.FirstOrDefault(r => r.Id_excWorkouts != null && r.Id_trainee != null && r.Id_excWorkouts.Id == workoutId && r.Id_trainee.Id == currentTraineeId);
//                if (registration != null)
//                {
//                    int res = await _apiService.DeleteATraining_registration(registration.Id);
//                    if (res > 0)
//                    {
//                        _allRegistrations.Remove(registration);
//                        itemToUpdate.CurrentBooked--;
//                        itemToUpdate.IsUserRegistered = false;

//                        MessageBox.Show("ההרשמה בוטלה בהצלחה.");
//                    }
//                    else
//                    {
//                        MessageBox.Show("פעולת הביטול נכשלה בשרת.");
//                    }
//                }
//            }
//            else
//            {
//                if (!CheckSubscriptionLimit())
//                {
//                    MessageBox.Show("אזהרה: עברת את מגבלת האימונים השבועית!", "מגבלת מנוי", MessageBoxButton.OK, MessageBoxImage.Warning);
//                    return;
//                }

//                var currentTraineeObj = _allTrainees.FirstOrDefault(t => t.Id == currentTraineeId);
//                var currentWorkoutObj = _allExcWorkouts.FirstOrDefault(w => w.Id == workoutId);
//                var newReg = new Training_registration
//                {
//                    Id_trainee = currentTraineeObj,
//                    Id_excWorkouts = currentWorkoutObj
//                };

//                int res = await _apiService.InsertATraining_registration(newReg);
//                if (res > 0)
//                {
//                    newReg.Id = res;
//                    _allRegistrations.Add(newReg);
//                    itemToUpdate.CurrentBooked++;
//                    itemToUpdate.IsUserRegistered = true;

//                    MessageBox.Show(itemToUpdate.IsFull ? "נכנסת לרשימת ההמתנה בהצלחה!" : "נרשמת לאימון בהצלחה!");
//                }
//                else
//                {
//                    MessageBox.Show("פעולת ההרשמה נכשלה בשרת.");
//                }
//            }

//            RefreshDetailsPanel(itemToUpdate);
//        }

//        private void BackButton_Click(object sender, RoutedEventArgs e)
//        {
//            if (NavigationService.CanGoBack)
//            {
//                NavigationService.GoBack();
//            }
//        }

//        private async void DeleteWorkoutButton_Click(object sender, RoutedEventArgs e)
//        {
//            var itemToDelete = WorkoutsListView.SelectedItem as WorkoutDisplayItem;
//            if (itemToDelete == null) return;

//            var result = MessageBox.Show($"האם אתה בטוח שברצונך לבטל ולמחוק את אימון ה-{itemToDelete.WorkoutName} לחלוטין?", "אישור מחיקה", MessageBoxButton.YesNo, MessageBoxImage.Question);
//            if (result == MessageBoxResult.Yes)
//            {
//                // קריאה לפונקציית המחיקה של האימון ב-API
//                int res = await _apiService.DeleteAList_of_Exc_workouts(itemToDelete.RawWorkout.Id);
//                if (res > 0)
//                {
//                    MessageBox.Show("האימון נמחק בהצלחה.");
//                    _allExcWorkouts.Remove(itemToDelete.RawWorkout);

//                    dynamic selectedDay = DaysListBox.SelectedItem;
//                    if (selectedDay != null) LoadWorkoutsForDate(selectedDay.Date);
//                    DetailsPanel.Visibility = Visibility.Collapsed;
//                }
//                else
//                {
//                    MessageBox.Show("מחיקת האימון נכשלה בשרת.");
//                }
//            }
//        }

//        private bool CheckSubscriptionLimit()
//        {
//            var trainee = _allTrainees.FirstOrDefault(t => t.Id == currentTraineeId);
//            if (trainee == null || trainee.Id_Sub == null) return true;

//            var sub = _allSubscriptions.FirstOrDefault(s => s.Id == trainee.Id_Sub.Id);
//            if (sub == null) return true;

//            int maxAllowedPerWeek = 3;
//            if (sub.Name_of_sub.Contains("חופשי חודשי") || sub.Name_of_sub.Contains("ללא הגבלה")) maxAllowedPerWeek = int.MaxValue;
//            else if (sub.Name_of_sub.Contains("דו שבועי")) maxAllowedPerWeek = 2;
//            else if (sub.Name_of_sub.Contains("חד שבועי")) maxAllowedPerWeek = 1;

//            DateTime startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
//            DateTime endOfWeek = startOfWeek.AddDays(7);

//            var bookedThisWeekCount = _allRegistrations.Count(r =>
//                r.Id_trainee != null && r.Id_trainee.Id == currentTraineeId &&
//                _allExcWorkouts.Any(w => r.Id_excWorkouts != null && w.Id == r.Id_excWorkouts.Id && w.Workout_date >= startOfWeek && w.Workout_date < endOfWeek)
//            );

//            return bookedThisWeekCount < maxAllowedPerWeek;
//        }

//        // ========================================================
//        // קוד פונקציות ה-Popup החדשות עבור הוספת האימון
//        // ========================================================

//        /// <summary>
//        /// פתיחת חלונית ה-Popup הפנימית (בלחיצה על הוספת אימון)
//        /// </summary>
//        /// <summary>
//        /// פתיחת חلوנית ה-Popup הפנימית (בלחיצה על הוספת אימון)
//        /// </summary>
//        private void AddWorkoutButton_Click(object sender, RoutedEventArgs e)
//        {
//            AddWorkoutPopup.IsOpen = true;

//            // ברירת מחדל: מילוי התאריך הנוכחי שנבחר בלוח השנה, או היום
//            if (DaysListBox.SelectedItem != null)
//            {
//                dynamic selectedDay = DaysListBox.SelectedItem;
//                DpDate.SelectedDate = selectedDay.Date;
//            }
//            else
//            {
//                DpDate.SelectedDate = DateTime.Today;
//            }

//            TxtTime.Text = DateTime.Now.ToString("HH:mm");
//            TxtKindId.Clear();

//            // מילוי חובה של ה-ID של המאמן המחובר - אי אפשר לשנות את זה ידנית בגלל ה-IsReadOnly ב-XAML
//            if (currentTrainerId != -1)
//            {
//                TxtTrainerId.Text = currentTrainerId.ToString();
//            }
//            else
//            {
//                TxtTrainerId.Clear();
//                MessageBox.Show("שים לב: לא זוהה מזהה מאמן מחובר במערכת.");
//            }
//        }

//        /// <summary>
//        /// ביטול וסגירת ה-Popup הפנימי
//        /// </summary>
//        private void BtnCancelPopup_Click(object sender, RoutedEventArgs e)
//        {
//            AddWorkoutPopup.IsOpen = false;
//        }

//        /// <summary>
//        /// שמירת האימון החדש מה-Popup למסד הנתונים
//        /// </summary>
//        private async void BtnSaveWorkout_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                // 1. בדיקת תקינות בסיסית של שדות טקסט ומספרים
//                if (DpDate.SelectedDate == null || !int.TryParse(TxtKindId.Text, out int kindId) || !int.TryParse(TxtTrainerId.Text, out int trainerId))
//                {
//                    MessageBox.Show("נא למלא תאריך, קוד סוג אימון וקוד מאמן תקינים.");
//                    return;
//                }

//                // 2. המרה וחיבור של השעה והתאריך
//                DateTime baseDate = DpDate.SelectedDate.Value;
//                if (!TimeSpan.TryParse(TxtTime.Text, out TimeSpan timeParsed))
//                {
//                    MessageBox.Show("נא להזין שעה תקינה בפורמט HH:mm (לדוגמה: 15:30)");
//                    return;
//                }
//                DateTime fullDateTime = baseDate.Date + timeParsed;

//                // 3. שליפה ואימות מול הרשימות שכבר נטענו מהשרת
//                var matchedKind = _allKinds.FirstOrDefault(k => k.Id == kindId);
//                // שליפת המאמן מרשימת ה-People או בדיקה ישירה
//                var allTrainers = await _apiService.GetAllTrainer();
//                var matchedTrainer = allTrainers?.Cast<Trainer>().FirstOrDefault(t => t.Id == trainerId);

//                if (matchedKind == null || matchedTrainer == null)
//                {
//                    MessageBox.Show("קוד מאמן או קוד סוג אימון לא קיימים במסד הנתונים!");
//                    return;
//                }

//                // 4. יצירת האובייקט החדש
//                var newWorkout = new List_of_Exc_workouts
//                {
//                    Workout_date = fullDateTime,
//                    Id_kindOf_workouts = matchedKind,
//                    Id_trainer = matchedTrainer
//                };

//                // 5. שמירה לשרת דרך ה-API
//                int newId = await _apiService.InsertAList_of_Exc_workouts(newWorkout);
//                if (newId > 0)
//                {
//                    newWorkout.Id = newId;

//                    // הוספה לרשימה המקומית כדי שלא נצטרך לטעון הכל מחדש מהמסד
//                    _allExcWorkouts.Add(newWorkout);

//                    MessageBox.Show("האימון נוסף בהצלחה!");
//                    AddWorkoutPopup.IsOpen = false; // סגירת ה-Popup

//                    // רענון אוטומטי של רשימת האימונים המוצגת למסך עבור היום הנוכחי
//                    LoadWorkoutsForDate(baseDate.Date);
//                }
//                else
//                {
//                    MessageBox.Show("פעולת השמירה נכשלה בשרת.");
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"שגיאה בשמירת האימון: {ex.Message}");
//            }
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        private DateTime _currentMonthDateTime = DateTime.Today;

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
            else
            {
                _isTrainer = false;
            }
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

                if (!_isTrainer && currentTraineeId == -1 && _allTrainees.Count > 0)
                {
                    currentTraineeId = _allTrainees.First().Id;
                }

                ApplyRolePermissions();
                DisplayMonth(_currentMonthDateTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת הנתונים מהשרת: {ex.Message}");
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

            if (_isTrainer && item.RawWorkout.Id_trainer != null && item.RawWorkout.Id_trainer.Id == currentTrainerId)
            {
                DeleteWorkoutButton.Visibility = Visibility.Visible;
            }
            else
            {
                DeleteWorkoutButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// פונקציית טעינת המתאמנים המתוקנת - מונעת איבוד כפילויות ומציגה את כולם במדויק
        /// </summary>
        private void UpdateRegisteredTraineesList(int workoutId)
        {
            // שליפת כל הרישומים המשויכים לקוד האימון הנוכחי
            var activeRegistrations = _allRegistrations
                .Where(r => r.Id_excWorkouts != null && r.Id_excWorkouts.Id == workoutId && r.Id_trainee != null)
                .ToList();

            List<Person> registeredPeople = new List<Person>();

            foreach (var reg in activeRegistrations)
            {
                // חיפוש ה-Person המתאים לכל רישום ספציפי
                var person = _allPeople.FirstOrDefault(p => p.Id == reg.Id_trainee.Id);
                if (person != null)
                {
                    registeredPeople.Add(person);
                }
            }

            RegisteredTraineesListView.ItemsSource = null;
            RegisteredTraineesListView.ItemsSource = registeredPeople;
        }

        private void UpdateActionButtonState(WorkoutDisplayItem item)
        {
            if (item.RawWorkout.Workout_date < DateTime.Now)
            {
                ActionButton.Content = "אימון זה כבר עבר";
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
                        MessageBox.Show("פעולת הביטול נכשלה בשרת.");
                    }
                }
            }
            else
            {
                if (!CheckSubscriptionLimit())
                {
                    MessageBox.Show("אזהרה: עברת את מגבלת האימונים השבועית!", "מגבלת מנוי", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MessageBox.Show("פעולת ההרשמה נכשלה בשרת.");
                }
            }

            RefreshDetailsPanel(itemToUpdate);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private async void DeleteWorkoutButton_Click(object sender, RoutedEventArgs e)
        {
            var itemToDelete = WorkoutsListView.SelectedItem as WorkoutDisplayItem;
            if (itemToDelete == null) return;

            var result = MessageBox.Show($"האם אתה בטוח שברצונך לבטל ולמחוק את אימון ה-{itemToDelete.WorkoutName} לחלוטין?", "אישור מחיקה", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                int res = await _apiService.DeleteAList_of_Exc_workouts(itemToDelete.RawWorkout.Id);
                if (res > 0)
                {
                    MessageBox.Show("האימון נמחק בהצלחה.");
                    _allExcWorkouts.Remove(itemToDelete.RawWorkout);

                    dynamic selectedDay = DaysListBox.SelectedItem;
                    if (selectedDay != null) LoadWorkoutsForDate(selectedDay.Date);
                    DetailsPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show("מחיקת האימון נכשלה בשרת.");
                }
            }
        }

        private bool CheckSubscriptionLimit()
        {
            var trainee = _allTrainees.FirstOrDefault(t => t.Id == currentTraineeId);
            if (trainee == null || trainee.Id_Sub == null) return true;

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

        private void AddWorkoutButton_Click(object sender, RoutedEventArgs e)
        {
            AddWorkoutPopup.IsOpen = true;

            if (DaysListBox.SelectedItem != null)
            {
                dynamic selectedDay = DaysListBox.SelectedItem;
                DpDate.SelectedDate = selectedDay.Date;
            }
            else
            {
                DpDate.SelectedDate = DateTime.Today;
            }

            TxtTime.Text = DateTime.Now.ToString("HH:mm");
            TxtKindId.Clear();
            TxtTrainerId.Text = currentTrainerId.ToString();
        }

        private void BtnCancelPopup_Click(object sender, RoutedEventArgs e)
        {
            AddWorkoutPopup.IsOpen = false;
        }

        private async void BtnSaveWorkout_Click(object sender, RoutedEventArgs e)
        {
            // פונקציית שמירה הקיימת שלך...
        }
    }
}