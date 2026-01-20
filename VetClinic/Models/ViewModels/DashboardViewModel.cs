using System;
using System.Collections.Generic;

namespace VetClinic.Models.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalUnpaidBills { get; set; }
        public int ActivePetCount { get; set; }

        public Appointment? NextAppointment { get; set; }

        public List<Appointment> RecentHistory { get; set; } = new List<Appointment>();
    }
}