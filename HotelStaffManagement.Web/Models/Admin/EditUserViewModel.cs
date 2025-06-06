using System.ComponentModel.DataAnnotations;

namespace HotelStaffManagement.Web.Models.Admin
{
    public class EditUserViewModel
    {
        [Required]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Անունը Ազգանունը պարտադիր է։")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Անունը Ազգանունը պետք է լինի առնվազն 5 սիմվոլ")]
        [Display(Name = "Անուն Ազգանուն")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Մուտքանունը պարտադիր է")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Օգտանունը պետք է լինի առնվազն 5 սիմվոլ")]
        [Display(Name = "Մուտքանուն")]
        public string Username { get; set; }

    }
}
