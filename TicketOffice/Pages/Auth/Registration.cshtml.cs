using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketOffice.Data;
using TicketOffice.Models;
using TicketOffice.Services;

namespace TicketOffice.Pages.Auth;

public class RegistrationModel : PageModel
{
    // Error massage displaying when email validation failed.
    public string EmailValidationError = null!;
    
    // Error massage displaying when password validation failed.
    public string PasswordValidationError = null!;

    private readonly TicketOfficeContext context;
    private readonly UserValidationService validationService;
    
    public RegistrationModel(TicketOfficeContext context,
        UserValidationService validationService)
    {
        this.context = context;
        this.validationService = validationService;
    }
    
    [BindProperty]
    public new User User { get; set; } = null!;

    // Called when GET request is sent to the page. Validates the session and
    // redirects to "Account" page if user already logged in.
    public ActionResult OnGet()
    {
        if (validationService.IsAuthorized(HttpContext))
        {
            return RedirectToPage("/Auth/Account");
        }

        return Page();
    }

    // Called when POST request is sent to the page. Validates registration form,
    // adds new user to the database and redirects to "Account" page if the
    // validation succeed.
    public ActionResult OnPost()
    {
        if (ValidateForm())
        {
            context.User.Add(User);
            context.SaveChanges();

            User = context.User.FirstOrDefault(u => u.Email == User.Email)!;
            
            HttpContext.Session.SetInt32("UserId", User.Id);
            return RedirectToPage("/Auth/Account");
        }
            
        return Page();
    }

    private bool ValidateForm()
    {
        return ValidateEmail(User.Email, out EmailValidationError) &&
               ValidatePassword(User.Password,
                   out PasswordValidationError);
        

        bool ValidateEmail(string email, out string validationError)
        {
            Regex emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                
            if (String.IsNullOrWhiteSpace(email))
            {
                validationError = "???????? ?????? ???????? ????????????????????";
                return false;
            }
                
            if (!emailRegex.IsMatch(email))
            {
                validationError = "E-mail ??????????????????????";
                return false;
            }

            User? user = context.User
                .FirstOrDefault(u => u.Email == User.Email);

            if (user != null)
            {
                validationError = "E-mail ?????? ??????????????????????????";
                return false;
            }
        
            validationError = String.Empty;
            return true;
        }
        
        bool ValidatePassword(string passowrd, out string validationError)
        {
            if (String.IsNullOrWhiteSpace(passowrd))
            {
                validationError = "???????? ?????? ???????? ????????????????????";
                return false;
            }
                
            if (passowrd.Length < 8 || passowrd.Length > 32)
            { 
                validationError = "?????????????? ?????? ???????? ?????? 8 ???? 32 ????????????????"; 
                return false;
            }
                
            Regex passwordRegex = 
                new Regex(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$");
                
            if (!passwordRegex.IsMatch(passowrd))
            {
                validationError = "???????????? ?????? ?????????????? " +
                                  "???????????? ???? ???????? ?????????????????? ????????????, " +
                                  "?????????? ???? ???????????????????? ?????????? (@, $, % ???? ????.)";
                return false;
            }
        
            validationError = String.Empty;
            return true;
        }
    }
}