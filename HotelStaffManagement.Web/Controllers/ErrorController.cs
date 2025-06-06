using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HotelStaffManagement.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            ViewBag.ErrorTitle = "Սխալ";
            ViewBag.ErrorMessage = "Ինչ-որ սխալ տեղի ունեցավ։";

            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorTitle = "Էջը չի գտնվել !";
                    ViewBag.ErrorMessage = "Ներեցեք, այս էջը գոյություն չունի։";
                    break;
                case 500:
                    ViewBag.ErrorTitle = "Սերվերի սխալ !";
                    ViewBag.ErrorMessage = "Տեղի ունեցավ անսպասելի սխալ։ Խնդրում ենք կրկին փորձել։";
                    break;
                default:
                    ViewBag.ErrorTitle = $"Սխալ կոդ՝ {statusCode}";
                    ViewBag.ErrorMessage = "Խնդրում ենք դիմել ադմինիստրատորին։";
                    break;
            }

            return View("GeneralError");
        }
        [Route("Error/GeneralError")]
        public IActionResult GeneralError()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            ViewBag.ExceptionPath = exceptionDetails?.Path;
            ViewBag.ExceptionMessage = exceptionDetails?.Error.Message;
            ViewBag.StackTrace = exceptionDetails?.Error.StackTrace;

            return View("GeneralError");
        }

    }
}
