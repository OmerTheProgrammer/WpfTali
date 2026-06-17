using Model;
using Service;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfTali.Pages
{
    public partial class SubscriptionPage : Page
    {
        public ObservableCollection<SubItem> Subscriptions { get; set; }

        private SubItem selectedPlanToBuy;
        private SubscriptionList apiSubs;
        private Trainee currentTrainee;
        private readonly Apiservice _apiService = new Apiservice();

        public SubscriptionPage(Trainee trainee)
        {
            InitializeComponent();

            currentTrainee = trainee;

            Subscriptions = new ObservableCollection<SubItem>();
            SubscriptionsList.ItemsSource = Subscriptions;

            LoadSubscriptionsAsync();
        }

        private async Task LoadSubscriptionsAsync()
        {
            try
            {
                apiSubs = await _apiService.GetAllSubscription();

                if (apiSubs != null)
                {
                    Subscriptions.Clear();
                    foreach (Subscription sub in apiSubs)
                    {
                        // בדיקה האם המנוי הזה הוא המנוי ששייך למתאמן כרגע
                        Brush initialColor = Brushes.White;
                        if (currentTrainee != null && currentTrainee.Id_Sub != null && sub.Id == currentTrainee.Id_Sub.Id)
                        {
                            initialColor = Brushes.Pink;
                        }

                        // שימי לב לשדה sub.price - אם באקסס השדה נקרא אחרת, שני אותו כאן!
                        Subscriptions.Add(new SubItem
                        {
                            id = sub.Id,
                            name_of_sub = sub.Name_of_sub,
                            price_display = "₪ " + sub.Price,
                            BackgroundColor = initialColor
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת המנויים מבסיס הנתונים: {ex.Message}");
            }
        }

        private void Purchase_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            selectedPlanToBuy = button.DataContext as SubItem;

            // מילוי אוטומטי של פרטי המתאמן הקיים בטופס
            if (currentTrainee != null)
            {
                TxtFirstName.Text = currentTrainee.First_name; // ודא שזה שם השדה ב-Trainee שלך
                TxtUsername.Text = currentTrainee.User_name;   // ודא שזה שם השדה
                TxtPhone.Text = currentTrainee.Telephone;
                TxtEmail.Text = currentTrainee.Email;
            }

            PaymentPanel.Visibility = Visibility.Visible;
            SubscriptionsList.IsEnabled = false;
        }

        private async void ConfirmPurchase_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPlanToBuy != null)
            {
                // 1. עדכון ויזואלי של צבעי הכרטיסיות במסך
                foreach (var plan in Subscriptions)
                {
                    if (plan.id == selectedPlanToBuy.id)
                    {
                        plan.BackgroundColor = Brushes.Pink;
                        currentTrainee.Id_Sub = apiSubs.Find(s => s.Id == plan.id)!;
                    }

                    else
                        plan.BackgroundColor = Brushes.White;
                }

                // נעדכן גם את ה-ID וגם את השם כדי שהאובייקט יהיה מלא ומסודר עבור ה-API
                currentTrainee.Id_Sub.Id = selectedPlanToBuy.id;
                currentTrainee.Id_Sub.Name_of_sub = selectedPlanToBuy.name_of_sub;

                // 3. שליחה ל-API לשמירה באקסס
                try
                {
                    this.IsEnabled = false; // חסימת המסך בזמן השמירה

                    // שליחת המתאמן המעודכן ל-API
                    int result = await _apiService.UpdateATrainee(currentTrainee);

                    if (result > 0)
                    {
                        MessageBox.Show($"Success! Subscription saved to database. You are now subscribed to: {selectedPlanToBuy.name_of_sub}");
                    }
                    else
                    {
                        // אם השרת החזיר 0 או מינוס, כנראה שיש בעיה בתוך ה-API (למשל שאילתת UPDATE לא נכונה)
                        MessageBox.Show("השרת קיבל את הבקשה אך נכשלה השמירה במסד הנתונים. ודא ששדות ה-Trainee תקינים.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"שגיאה בתקשורת עם השרת: {ex.Message}");
                }
                finally
                {
                    this.IsEnabled = true;
                    PaymentPanel.Visibility = Visibility.Collapsed;
                    SubscriptionsList.IsEnabled = true;
                }
            }
        }
        private void CancelSub_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var clickedPlan = button.DataContext as SubItem;

            if (clickedPlan != null)
            {
                clickedPlan.BackgroundColor = Brushes.White;
            }

            if (currentTrainee.Id_Sub != null)
            {
                currentTrainee.Id_Sub.Id = 0;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            PaymentPanel.Visibility = Visibility.Collapsed;
            SubscriptionsList.IsEnabled = true;
        }

        private void SubscriptionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SubscriptionsList.SelectedIndex = -1;
        }

        private void back_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HomePageTe(currentTrainee));
        }
    }

    // מחלקת עזר שמודיעה למסך לעדכן צבעים בזמן אמת בזכות ה-INotifyPropertyChanged
    public class SubItem : INotifyPropertyChanged
    {
        public int id { get; set; }
        public string name_of_sub { get; set; }
        public string price_display { get; set; }

        private Brush _backgroundColor;
        public Brush BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                OnPropertyChanged("BackgroundColor");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}