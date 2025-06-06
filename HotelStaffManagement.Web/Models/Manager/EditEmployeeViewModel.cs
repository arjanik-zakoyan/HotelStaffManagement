using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HotelStaffManagement.Web.Models.Manager
{
    public class EditEmployeeViewModel
    {
        [Required]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Անունը Ազգանունը պարտադիր է։")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Անունը Ազգանունը պետք է լինի առնվազն 5 սիմվոլ")]
        [Display(Name = "Անուն Ազգանուն")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Խնդրում ենք ընտրել հաստիքը։")]
        [Display(Name = "Հաստիք")]
        public string Position { get; set; }

        [MaxLength(20)]
        [RegularExpression(@"^0([1-9][0-9])\d{6}$", ErrorMessage = "Մուտքագրեք վավեր հայկական հեռախոսահամար։")]
        [Display(Name = "Հեռախոսահամար")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Ժամավճարը պարտադիր է։")]
        [Range(0, 10000, ErrorMessage = "Ժամավճարը պետք է լինի 0-ից մեծ։")]
        [Display(Name = "Ժամավճար")]
        public decimal SalaryPerHour { get; set; }

        public List<SelectListItem> Positions { get; set; } = new List<SelectListItem>();
    }
}
