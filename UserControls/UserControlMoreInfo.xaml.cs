
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Data.OleDb; // מאפשר עבודה עם מסד נתונים אקסס
namespace WpfTali.UserControls
{
    public partial class UserControlMoreInfo : UserControl
    {
        private bool isEditMode = false;

        public event EventHandler MyEvent;
        public TrainerMoreInfo ContactNewdate { get; set; }

        public UserControlMoreInfo()
        {
            InitializeComponent();
        }

        // בנאי המקבל נתונים
        public UserControlMoreInfo(TrainerMoreInfo contactNewdata)
        {
            InitializeComponent();
            UpdateData(contactNewdata);
        }

        /// <summary>
        /// פונקציה לעדכון נתונים ידני (למשל מתוך ה-MainWindow)
        /// </summary>
        public void SetCoachData(double paymet_per_hour, string certificate, bool experience, string description)
        {
            TxtViewPayment.Text = paymet_per_hour.ToString();
            TxtViewCertificate.Text = string.IsNullOrEmpty(certificate) ? "Empty" : certificate;
            TxtViewExperience.Text = experience ? "יש ניסיון" : "אין ניסיון";
            TxtViewDescription.Text = string.IsNullOrEmpty(description) ? "Empty" : description;


            TxtEditCertificate.Text = certificate;
            CmbEditExperience.SelectedIndex = experience ? 0 : 1; // 0="Have", 1="Doesn't have"
            TxtEditDescription.Text = description;
        }

        /// <summary>
        /// אירוע לחיצה על כפתור העיפרון/שמירה
        /// </summary>
        private void BtnEditMode_Click(object sender, RoutedEventArgs e)
        {
            if (!isEditMode)
            {
                isEditMode = true;
                ImgEditIcon.Source = new BitmapImage(new Uri("https://cdn-icons-png.flaticon.com/512/10419/10419616.png"));
                ToggleVisibility(true);
            }
            else
            {
                isEditMode = false;

                if (ContactNewdate == null)
                {
                    ContactNewdate = new TrainerMoreInfo();
                }

                ContactNewdate.ContactCertifications = TxtEditCertificate.Text;
                ContactNewdate.ContactDescription = TxtEditDescription.Text;
                ContactNewdate.ContactExperience = (CmbEditExperience.SelectedIndex == 0);

                TxtViewCertificate.Text = string.IsNullOrEmpty(ContactNewdate.ContactCertifications) ? "Empty" : ContactNewdate.ContactCertifications;
                TxtViewDescription.Text = string.IsNullOrEmpty(ContactNewdate.ContactDescription) ? "Empty" : ContactNewdate.ContactDescription;
                TxtViewExperience.Text = ContactNewdate.ContactExperience ? "Have" : "Doesn't have";

                ToggleVisibility(false);
                ImgEditIcon.Source = new BitmapImage(new Uri("https://cdn-icons-png.flaticon.com/512/116/116996.png"));

                MessageBox.Show("השינויים נשמרו בהצלחה!");
            }
        }

        public void UpdateData(TrainerMoreInfo contactNewdata)
        {
            if (contactNewdata == null)
            {
                TxtViewCertificate.Text = "Loading...";
                TxtViewExperience.Text = "Loading...";
                TxtViewDescription.Text = "Loading...";
                TxtViewPayment.Text = "0";
                ToggleVisibility(isEditMode);
                return;
            }

            this.ContactNewdate = contactNewdata;


            this.TxtViewCertificate.Text = string.IsNullOrEmpty(contactNewdata.ContactCertifications) ? "Empty" : contactNewdata.ContactCertifications;
            this.TxtViewDescription.Text = string.IsNullOrEmpty(contactNewdata.ContactDescription) ? "Empty" : contactNewdata.ContactDescription;
            this.TxtViewPayment.Text = contactNewdata.Contactpayment_per_hour.ToString();
            this.TxtViewExperience.Text = contactNewdata.ContactExperience ? "Have" : "Doesn't have";

            this.TxtEditCertificate.Text = contactNewdata.ContactCertifications;
            this.TxtEditDescription.Text = contactNewdata.ContactDescription;
            this.CmbEditExperience.SelectedIndex = contactNewdata.ContactExperience ? 0 : 1;

            ImgEditIcon.Source = new BitmapImage(new Uri("https://cdn-icons-png.flaticon.com/512/116/116996.png"));

            ToggleVisibility(isEditMode);

            MessageBox.Show("הנתונים עודכנו בהצלחה!");
        }

  
        private void ToggleVisibility(bool editMode)
        {
            Visibility viewVisibility = editMode ? Visibility.Collapsed : Visibility.Visible;
            Visibility editVisibility = editMode ? Visibility.Visible : Visibility.Collapsed;


            TxtViewCertificate.Visibility = viewVisibility;
            TxtViewExperience.Visibility = viewVisibility;
            TxtViewDescription.Visibility = viewVisibility;


            TxtEditCertificate.Visibility = editVisibility;
            CmbEditExperience.Visibility = editVisibility;
            TxtEditDescription.Visibility = editVisibility;
        }
    }
}




























