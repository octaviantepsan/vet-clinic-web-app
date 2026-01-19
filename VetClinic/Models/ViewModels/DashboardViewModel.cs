using System;
using System.Collections.Generic;

namespace VetClinic.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Summary Data
        public decimal TotalUnpaidBills { get; set; }
        public int ActivePetCount { get; set; }

        // The Next Appointment (Just the soonest one)
        public Appointment? NextAppointment { get; set; }

        // Recent Medical History (Last 3 visits)
        public List<Appointment> RecentHistory { get; set; } = new List<Appointment>();
    }
}