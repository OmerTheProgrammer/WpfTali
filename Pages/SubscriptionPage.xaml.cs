
using Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfTali.Pages
{
    public partial class SubscriptionPage : Page
    {
        // רשימת המנויים
        public ObservableCollection<SubItem> Subscriptions
        {
            get;
            set;
        }

        // המנוי שנבחר
        private SubItem selectedPlanToBuy;

        // המשתמש המחובר
        private Trainee currentTrainee;

        // constructor
        public SubscriptionPage(Trainee trainee)
        {
            InitializeComponent();

            currentTrainee = trainee;

            // יצירת רשימת המנויים
            Subscriptions =
                new ObservableCollection<SubItem>
            {
                new SubItem
                {
                    id = 1,
                    name_of_sub = "Once a week",
                    price_display = "₪ 120.00",
                    BackgroundColor = Brushes.White
                },

                new SubItem
                {
                    id = 2,
                    name_of_sub = "Twice a week",
                    price_display = "₪ 240.00",
                    BackgroundColor = Brushes.White
                },

                new SubItem
                {
                    id = 3,
                    name_of_sub = "Three times a week",
                    price_display = "₪ 360.00",
                    BackgroundColor = Brushes.White
                },

                new SubItem
                {
                    id = 45,
                    name_of_sub = "Four times a week",
                    price_display = "₪ 480.00",
                    BackgroundColor = Brushes.White
                },

                new SubItem
                {
                    id = 46,
                    name_of_sub = "Five times a week",
                    price_display = "₪ 600.00",
                    BackgroundColor = Brushes.White
                },

                new SubItem
                {
                    id = 47,
                    name_of_sub = "Every day",
                    price_display = "₪ 720.00",
                    BackgroundColor = Brushes.White
                }
            };

            if (currentTrainee != null)
            {
                foreach (var plan in Subscriptions)
                {
                    if (currentTrainee.Id_Sub != null &&
                        plan.id == currentTrainee.Id_Sub.Id)
                    {
                        plan.BackgroundColor = Brushes.Pink;
                    }
                }
            }

            SubscriptionsList.ItemsSource =
                Subscriptions;
        }

        private void Purchase_Click(
            object sender,
            RoutedEventArgs e)
        {
            var button = sender as Button;

            selectedPlanToBuy =
                button.DataContext as SubItem;

            PaymentPanel.Visibility =
                Visibility.Visible;

            SubscriptionsList.IsEnabled = false;
        }

        private void ConfirmPurchase_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (selectedPlanToBuy != null)
            {
                foreach (var plan in Subscriptions)
                {
                    if (plan.id ==
                        selectedPlanToBuy.id)
                    {
                        plan.BackgroundColor =
                            Brushes.Pink;
                    }
                    else
                    {
                        plan.BackgroundColor =
                            Brushes.White;
                    }
                }

                //currentTrainee.Id_Sub.Id =
                //    selectedPlanToBuy.id;
                // הגנה: בדיקה האם אובייקט המנוי של המתאמן הנוכחי הוא נאל
                if (currentTrainee.Id_Sub == null)
                {
                    // יצירת מופע חדש של אובייקט המנוי כדי שלא יקרוס
                    currentTrainee.Id_Sub = new Subscription();
                }

                // עכשיו בטוח לגמרי לעדכן את ה-ID של המנוי שלו!
                currentTrainee.Id_Sub.Id = selectedPlanToBuy.id;

                SubscriptionsList.Items.Refresh();
                PaymentPanel.Visibility =
                    Visibility.Collapsed;

                SubscriptionsList.IsEnabled =
                    true;

                MessageBox.Show(
                    $"Success! You are now subscribed to: {selectedPlanToBuy.name_of_sub}");
            }
        }
        private void CancelSub_Click(
            object sender,
            RoutedEventArgs e)
        {
            foreach (var plan in Subscriptions)
            {
                plan.BackgroundColor =
                    Brushes.White;
            }
            currentTrainee.Id_Sub.Id = 0;

            SubscriptionsList.Items.Refresh();
        }

        private void Back_Click(
            object sender,
            RoutedEventArgs e)
        {
            PaymentPanel.Visibility =
                Visibility.Collapsed;

            SubscriptionsList.IsEnabled = true;
        }

        private void SubscriptionsList_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            SubscriptionsList.SelectedIndex = -1;
        }

        private void back_Click_1(
            object sender,
            RoutedEventArgs e)
        {
            NavigationService.Navigate(
                new HomePageTe(currentTrainee));
        }

        public class SubItem
        {
            public int id { get; set; }

            public string name_of_sub
            {
                get;
                set;
            }

            public string price_display
            {
                get;
                set;
            }

            public Brush BackgroundColor
            {
                get;
                set;
            }
        }
    }
}







