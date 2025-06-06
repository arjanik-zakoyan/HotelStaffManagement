using System.Globalization;

namespace HotelStaffManagement.Web.Models.Manager
{
    public class SalaryOverviewViewModel
    {
        public string Month { get; set; } = null!;
        public List<SalaryEntryViewModel> Salaries { get; set; } = new();
        public string DisplayMonth => DateTime.ParseExact(Month, "yyyy-MM", CultureInfo.InvariantCulture)
                                                .ToString("MMMM yyyy", new CultureInfo("hy-AM"));
    }
}
