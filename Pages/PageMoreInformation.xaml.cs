using Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ViewModel;
using WpfTali.Pages;
using WpfTali.UserControls;

namespace WpfTali.Pages
{
    /// <summary>
    /// Interaction logic for PageMoreInformation.xaml
    /// </summary>
    public partial class PageMoreInformation : Page
    {
        public PageMoreInformation()
        {
            InitializeComponent();
        }
        public PageMoreInformation(Trainer trainer)
        {
            InitializeComponent();

            if (trainer == null) return;

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
    private void UserControlMoreInfo_EventHandler(object sender, EventArgs e)
    {
        if (sender is UserControlMoreInfo specificUserControlss)
        {
            TrainerMoreInfo data = specificUserControlss.ContactNewdate;

                string message = $"Payment Per Hour: {data.Contactpayment_per_hour}\n" +
                                 $"Certification: {data.ContactCertifications}\n" +
                                 $"Experience: {data.ContactExperience}\n" +
                                 $"Description: {data.ContactDescription}\n";               

            MessageBox.Show(message, "Trainer More  Information");
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
