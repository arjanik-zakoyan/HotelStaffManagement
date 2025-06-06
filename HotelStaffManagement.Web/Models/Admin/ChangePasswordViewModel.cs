using System.ComponentModel.DataAnnotations;

namespace HotelStaffManagement.Web.Models.Admin
{
    public class ChangePasswordViewModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Նոր գաղտնաբառը պարտադիր է։")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Նոր գաղտնաբառը պետք է ունենա առնվազն 5 նիշ։")]
        [Display(Name = "Նոր գաղտնաբառ")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Հաստատումը պարտադիր է։")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Գաղտնաբառերը չեն համընկնում։")]
        [Display(Name = "Հաստատել գաղտնաբառը")]
        public string ConfirmPassword { get; set; }
    }
}
