using System.ComponentModel.DataAnnotations;

namespace HotelStaffManagement.Web.Models.Admin
{
    public class CreateManagerViewModel
    {
        [Required(ErrorMessage = "Անունը Ազգանունը պարտադիր է։")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Անունը Ազգանունը պետք է լինի առնվազն 5 սիմվոլ")]
        [Display(Name = "Անուն Ազգանուն")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Մուտքանունը պարտադիր է")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Մուտքանունը պետք է լինի առնվազն 5 սիմվոլ")]
        [Display(Name = "Մուտքանուն")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Գաղտնաբառը պարտադիր է")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Գաղտնաբառը պետք է լինի առնվազն 5 նիշ")]
        [Display(Name = "Գաղտնաբառ")]
        public string Password { get; set; }

        public decimal SalaryPerHour { get; set; } = 0; 
    }
}
