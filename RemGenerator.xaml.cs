using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.GData.Calendar;
using Google.GData.Client;
using System.Net;
using Google.GData.Extensions;
using Microsoft.Windows.Controls;

namespace RemGenerator
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private const String calendarApiUri = "http://www.google.com/calendar/feeds/default/private/full";

        public Window1()
        {
            InitializeComponent();

            txtUserName.Text = Settings.Default.MostRecentUserName;

            DateTime current = DateTime.Now.AddMinutes(5);
            dtpDate.SelectedDate = current.Date;
            txtTime.Text = new TimeSpan(current.TimeOfDay.Hours, current.TimeOfDay.Minutes, 0).ToString();

            txtPassword.Focus();
        }

        public void CreateReminder()
        {
            if (!dtpDate.SelectedDate.HasValue)
            {
                MessageBox.Show("You must enter a valid date");
                return;
            }
            TimeSpan reminderTime;
            if (!TimeSpan.TryParse(txtTime.Text, out reminderTime))
            {
                MessageBox.Show("You must enter a valid time");
                return;
            }

            DateTime reminderDateTime = dtpDate.SelectedDate.Value.Date + reminderTime;
            if (reminderDateTime <= DateTime.Now)
            {
                MessageBox.Show("The reminder date/time must be in the future");
                return;
            }

            Settings.Default.MostRecentUserName = txtUserName.Text;
            Settings.Default.Save();

            CalendarService calendarService = CreateCalendarService();
            CreateReminder(calendarService, txtReminderText.Text, reminderDateTime);

            //GDataRequestFactory requestFactory = (GDataRequestFactory)myService.RequestFactory;
            //IWebProxy iProxy = WebRequest.DefaultWebProxy;
            //WebProxy myProxy = new WebProxy(iProxy.GetProxy(query.Uri));
            //// potentially, setup credentials on the proxy here
            //myProxy.Credentials = CredentialCache.DefaultCredentials;
            //myProxy.UseDefaultCredentials = true;
            //requestFactory.Proxy = myProxy;

            //CalendarQuery query = new CalendarQuery();
            //query.Uri = new Uri("http://www.google.com/calendar/feeds/default/allcalendars/full");

            //CalendarFeed resultFeed = myService.Query(query);
            //Console.WriteLine("Cals:");
            //foreach (CalendarEntry entry in resultFeed.Entries)
            //{
            //    Console.WriteLine(entry.Title.Text);
            //}
        }

        private void CreateReminder(CalendarService calendarService, String reminderText, DateTime reminderTime)
        {
            EventEntry newEvent = new EventEntry();
            newEvent.Title.Text = reminderText;
            newEvent.Content.Content = reminderText + " (Reminder added by RemGen from " + Environment.MachineName + " by " + Environment.UserName + " " + Environment.UserDomainName + ")";

            //Where eventLocation = new Where();
            //eventLocation.ValueString = "Test event location 2";
            //newEvent.Locations.Add(eventLocation);

            When eventTime = new When(reminderTime, reminderTime);
            newEvent.Times.Add(eventTime);

            Reminder reminder = new Reminder();
            reminder.Method = Reminder.ReminderMethod.sms;
            reminder.AbsoluteTime = reminderTime;

            newEvent.Reminders.Add(reminder);

            Uri postUri = new Uri(calendarApiUri);
            try
            { 
                AtomEntry insertedEntry = calendarService.Insert(postUri, newEvent);
                MessageBox.Show("Reminder added successfully");
            }
            catch (Exception ex)
            { 
                MessageBox.Show("Failed to create event" + Environment.NewLine + Environment.NewLine + ex.ToString(), "RemGen");
            }
        }

        private CalendarService CreateCalendarService()
        {
            CalendarService calendarService = new CalendarService("RemGen");

            if (Settings.Default.UseProxy)
            {
                GDataRequestFactory requestFactory = (GDataRequestFactory)calendarService.RequestFactory;
                WebProxy myProxy = new WebProxy(Settings.Default.ProxyAddress, true);
                if (Settings.Default.AuthProxy)
                {
                    myProxy.Credentials = new NetworkCredential(Settings.Default.ProxyUsername, Settings.Default.ProxyPassword);
                    myProxy.UseDefaultCredentials = false;
                }
                else
                {
                    myProxy.UseDefaultCredentials = true;
                }
                
                requestFactory.Proxy = myProxy;
            }

            calendarService.setUserCredentials(txtUserName.Text, txtPassword.Password);
            return calendarService;
        }

        private void AddReminder_Click(object sender, RoutedEventArgs e)
        {
            CreateReminder();
        }
    }
}