using System.ComponentModel.DataAnnotations;

namespace HotelStaffManagement.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Մուտքանունը պարտադիր է")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Օգտանունը պետք է լինի առնվազն 5 սիմվոլ")]
        [Display(Name = "Մուտքանուն")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Գաղտնաբառը պարտադիր է")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Գաղտնաբառը պետք է լինի առնվազն 5 նիշ")]
        [Display(Name = "Գաղտնաբառ")]
        public string Password { get; set; }

        [Display(Name = "Հիշել ինձ")]
        public bool RememberMe { get; set; }

    }
}
