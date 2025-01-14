using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ManagedShell.Common.Helpers;
using RetroBar.Utilities;
using System.Windows;
using ManagedShell.Common.Logging;
using Microsoft.Win32;
using ManagedShell.AppBar;
using Windows.ApplicationModel.Appointments;
using System.Windows.Data;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace RetroBar
{
    /// <summary>
    /// Interaction logic for CalendarWindow.xaml
    /// </summary>
    public partial class CalendarWindow : Window
    {
        public CalendarWindow()
        {
            InitializeComponent();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            this.Close();
        }

        private async Task GetCalendarItems(DateTimeOffset? offset = null)
        {
            AppointmentStore appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadWrite);
            var opts = new FindAppointmentsOptions();
            opts.FetchProperties.Add(AppointmentProperties.StartTime);
            opts.FetchProperties.Add(AppointmentProperties.Duration);
            opts.FetchProperties.Add(AppointmentProperties.Subject);
            opts.FetchProperties.Add(AppointmentProperties.IsCanceledMeeting);
            opts.FetchProperties.Add(AppointmentProperties.Location);
            opts.FetchProperties.Add(AppointmentProperties.AllDay);

            var filterOffset = offset ?? DateTimeOffset.Now;
            var endOfDay = new TimeSpan((24 - filterOffset.Hour) - 1, (60 - filterOffset.Minute) - 1, (60 - filterOffset.Second) - 1);
            var meetings = await appointmentStore.FindAppointmentsAsync(filterOffset, endOfDay, opts);

            AppointmentList.DataContext = meetings;
            var bind = new Binding();

            AppointmentList.SetBinding(ItemsControl.ItemsSourceProperty, bind);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await GetCalendarItems();
        }

        private async void AppointmentCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            var calendar = (Calendar)sender;
            if (calendar.SelectedDate.HasValue)
            {
                DateTimeOffset date = new DateTimeOffset(calendar.SelectedDate.Value.ToUniversalTime());
                await GetCalendarItems(date);
            }
        }
        private void AppointmentList_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            PerformScroll(AppointmentScrollViewer, e);
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var viewer = (ScrollViewer)sender;
            PerformScroll(viewer, e);
        }

        private void PerformScroll(ScrollViewer viewer, System.Windows.Input.MouseWheelEventArgs e)
        {
            viewer.ScrollToVerticalOffset(viewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

    }
}