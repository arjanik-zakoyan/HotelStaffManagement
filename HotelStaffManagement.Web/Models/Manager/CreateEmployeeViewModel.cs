using HotelStaffManagement.DataAccess.Enums;
using HotelStaffManagement.Web.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HotelStaffManagement.Web.Models.Manager
{
    // ViewModel
    public class CreateEmployeeViewModel
    {
        [Required(ErrorMessage = "Անունը Ազգանունը պարտադիր է։")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Անունը Ազգանունը պետք է լինի առնվազն 5 սիմվոլ")]
        [Display(Name = "Անուն Ազգանուն")]
        public string FullName { get; set; }

        [MaxLength(20)]
        [RegularExpression(@"^0([1-9][0-9])\d{6}$", ErrorMessage = "Մուտքագրեք վավեր հայկական հեռախոսահամար։")]
        [Display(Name = "Հեռախոսահամար")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Մուտքանունը պարտադիր է։")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Մուտքանունը պետք է լինի առնվազն 5 սիմվոլ")]
        [Display(Name = "Մուտքանուն")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Գաղտնաբառը պարտադիր է։")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Գաղտնաբառը պետք է լինի առնվազն 5 նիշ")]
        [DataType(DataType.Password)]
        [Display(Name = "Գաղտնաբառ")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Հաստիքը պարտադիր է։")]
        [Display(Name = "Հաստիք")]
        public EmployeePosition? Position { get; set; }

        [Required(ErrorMessage = "Ժամավճարը պարտադիր է։")]
        [Range(1, 10000, ErrorMessage = "Ժամավճարը պետք է լինի 0-ից բարձր թիվ")]
        [Display(Name = "Աշխատավարձը (1 ժամվա համար)")]
        public decimal SalaryPerHour { get; set; }

        public List<SelectListItem> Positions { get; set; } = new List<SelectListItem>();

    }
}
