using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HotelStaffManagement.Web.Models.Manager
{
    public class CreateScheduleViewModel
    {
        [Required(ErrorMessage = "Ամսաթվի լրացումը պարտադիր է։")]
        [Display(Name = "Ամսաթիվ")]
        [DataType(DataType.Date)]
        public DateTime SelectedDate { get; set; }

        [Required(ErrorMessage = "Խնդրում ենք ընտրել հաստիք։")]
        [Display(Name = "Հաստիք")]
        public string SelectedPosition { get; set; } = null!;

        [Required(ErrorMessage = "Խնդրում ենք ընտրել հերթափոխ։")]
        [Display(Name = "Հերթափոխ")]
        public string SelectedShift { get; set; } = null!;

        [Required(ErrorMessage = "Խնդրում ենք ընտրել գոնե մեկ աշխատակից։")]
        [Display(Name = "Ընտրված աշխատակիցներ")]
        public List<int> SelectedEmployeeIds { get; set; } = new();

        public List<SelectListItem> Positions { get; set; } = new();
        public List<SelectListItem> Shifts { get; set; } = new();
        public List<SelectListItem> Employees { get; set; } = new();
    }
}
