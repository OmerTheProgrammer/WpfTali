//using Model;
//using System;
//using System.Collections.Generic;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Media;
//using ViewModel;
//using WpfTali.Pages;
//using WpfTali.UserControls;

//namespace WpfTali.Pages
//{
//    /// <summary>
//    /// Interaction logic for PageMoreInformation.xaml
//    /// </summary>
//    public partial class PageMoreInformation : Page
//    {
//        public PageMoreInformation()
//        {
//            InitializeComponent();
//        }
//        public PageMoreInformation(Trainer trainer)
//        {
//            InitializeComponent();

//            if (trainer == null) return;

//            List<TrainerMoreInfo> contacts = new List<TrainerMoreInfo>();

//            contacts.Add(new TrainerMoreInfo
//            {
//                Contactpayment_per_hour = trainer.Paymet_per_hour,
//                ContactCertifications = trainer.Certificate,
//                ContactExperience = trainer.Experience,
//                ContactDescription = trainer.Description

//            });

//            this.moreContacts.Children.Clear();

//            foreach (TrainerMoreInfo contact in contacts)
//            {
//                UserControlMoreInfo userControls = new UserControlMoreInfo(contact);
//                userControls.MyEvent += UserControlMoreInfo_EventHandler;
//                this.moreContacts.Children.Add(userControls);
//            }
//        }
//    private void UserControlMoreInfo_EventHandler(object sender, EventArgs e)
//    {
//        if (sender is UserControlMoreInfo specificUserControlss)
//        {
//            TrainerMoreInfo data = specificUserControlss.ContactNewdate;

//                string message = $"Payment Per Hour: {data.Contactpayment_per_hour}\n" +
//                                 $"Certification: {data.ContactCertifications}\n" +
//                                 $"Experience: {data.ContactExperience}\n" +
//                                 $"Description: {data.ContactDescription}\n";               

//            MessageBox.Show(message, "Trainer More  Information");
//        }
//        else
//        {
//            MessageBox.Show("Error: Sender is not a valid information control.");
//        }
//    }
//        private void Back_Click(object sender, RoutedEventArgs e)
//        {
//            NavigationService.Navigate(new HomePageTr(HomePageTr.currentTrainer));
//        }
//    }
//}
using Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Service; // מבטיח גישה למחלקת ה-Apiservice שלך
using WpfTali.Pages;
using WpfTali.UserControls;

namespace WpfTali.Pages
{
    /// <summary>
    /// Interaction logic for PageMoreInformation.xaml
    /// </summary>
    public partial class PageMoreInformation : Page
    {
        private Trainer currentTrainer;
        // יצירת מופע של שירות ה-API שלך
        private Apiservice apiService = new Apiservice();

        public PageMoreInformation()
        {
            InitializeComponent();
        }

        public PageMoreInformation(Trainer trainer)
        {
            InitializeComponent();

            if (trainer == null) return;

            this.currentTrainer = trainer;

            List<TrainerMoreInfo> contacts = new List<TrainerMoreInfo>();

            contacts.Add(new TrainerMoreInfo
            {
                Contactpayment_per_hour = trainer.Paymet_per_hour,
                ContactCertifications = trainer.Certificate,
                ContactExperience = trainer.Experience,
                ContactDescription = trainer.Description
            });

            this.moreContacts.Children.Clear();

            foreach (TrainerMoreInfo contact in contacts)
            {
                UserControlMoreInfo userControls = new UserControlMoreInfo(contact);
                userControls.MyEvent += UserControlMoreInfo_EventHandler;
                this.moreContacts.Children.Add(userControls);
            }
        }

        /// <summary>
        /// מנהל אירוע אסינכרוני השולח את השינויים ישירות לשרת ה-API של האפליקציה
        /// </summary>
        private async void UserControlMoreInfo_EventHandler(object sender, EventArgs e)
        {
            if (sender is UserControlMoreInfo specificUserControlss)
            {
                // 1. שליפת הנתונים החדשים מה-UserControl
                TrainerMoreInfo data = specificUserControlss.ContactNewdate;

                // 2. עדכון אובייקט המאמן בערכים החדשים שהוזנו
                currentTrainer.Certificate = data.ContactCertifications;
                currentTrainer.Experience = data.ContactExperience;
                currentTrainer.Description = data.ContactDescription;

                try
                {
                    // 3. קריאה לפעולת ה-API הקיימת שלך מה-ApiService
                    // הפעולה מחזירה 1 אם הצליח ו-0 אם נכשל
                    int result = await apiService.UpdateATrainer(currentTrainer);

                    if (result == 1)
                    {
                        MessageBox.Show("הנתונים נשמרו בהצלחה במסד הנתונים דרך ה-API!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("השרת קיבל את הבקשה אך נכשלה השמירה באקסס.", "שגיאה בעדכון", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"שגיאה בתקשורת עם ה-API:\n{ex.Message}", "שגיאה קריטית", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Error: Sender is not a valid information control.");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HomePageTr(HomePageTr.currentTrainer));
        }
    }
}