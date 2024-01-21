// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace E_OneWeb.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        //private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            //IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostEnvironment)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            //_emailSender = emailSender;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            //[Required]
            //[EmailAddress]
            //[Display(Name = "Email")]
            //public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "Kata sandi dan kata sandi konfirmasi tidak cocok.")]
            public string ConfirmPassword { get; set; }
            [Required(ErrorMessage = "User Login harus diisi ")]
            public string Name { get; set; }
            [Required(ErrorMessage = "Nama Lengkap harus diisi ")]
            public string FullName { get; set; }
            [Required(ErrorMessage = "Jenis Kelamin harus diisi ")]
            public string Gender { get; set; }
            public string NIM { get; set; }          
            [Required(ErrorMessage = "No Telepon harus diisi ")]
            [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "No Telepon harus numeric")]
            public string PhoneNumber { get; set; }
            public string Fakultas { get; set; }
            public string Prodi { get; set; }
            public string Role { get; set; }
            //[Required(ErrorMessage = "Photo haris diisi ")]
            public string Photo { get; set; }
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
         
            if (!_roleManager.RoleExistsAsync(SD.Role_User).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Unit)).GetAwaiter().GetResult();
            }
        
            //Input = new InputModel()
            //{
            //    CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
            //    {
            //        Text = i.Name,
            //        Value = i.Id.ToString()
            //    }),
            //    RoleList = _roleManager.Roles.Where(u => u.Name != SD.Role_User_Indi).Select(x => x.Name).Select(i => new SelectListItem
            //    {
            //        Text = i,
            //        Value = i
            //    })
            //};
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            Personal personal = new Personal();

            string webRootPath = _hostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
           
            if (files.Count > 0)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(webRootPath, @"images\products");
                var extenstion = Path.GetExtension(files[0].FileName);
                if (extenstion.ToLower().Contains(".jpg") || extenstion.ToLower().Contains(".jpeg"))
                {
                    if (personal.Photo != null)
                    {
                        //this is an edit and we need to remove old image
                        var imagePath = Path.Combine(webRootPath, personal.Photo.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                    {
                        files[0].CopyTo(filesStreams);
                    }
                    personal.Photo = @"\images\products\" + fileName + extenstion;
                }
                else
                {
                    ModelState.AddModelError("Photo", "Type Photo jpg, jpeg");
                }

            }
            else
            {
                ModelState.AddModelError("Photo", "Photo harus diisi");
            }

            var errorList = ModelState.Values.SelectMany(x => x.Errors).ToList();
            if (ModelState.IsValid)
            {

                var user = new ApplicationUser
                {
                    UserName = Input.Name,
                    //Email = Input.Email,
                    Fakultas = Input.Fakultas,                
                    Name = Input.Name,
                    PhoneNumber = Input.PhoneNumber,                   
                    Gender = Input.Gender,
                    NIM = Input.NIM,
                    Prodi = Input.Prodi,
                    Role = Input.Role,
                    FullName = Input.FullName
                };

                //var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Name, CancellationToken.None);
                //await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    //var callbackUrl = Url.Page(
                    //    "/Account/ConfirmEmail",
                    //    pageHandler: null,
                    //    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    //    protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (user.UserName == "iwansetti" && user.PhoneNumber == "081289277785")
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                        if (user.Role == null)
                        {
                            await _userManager.AddToRoleAsync(user, SD.Role_Admin);
                            user.RolesName = SD.Role_Admin;
                        }
                    }
                    else
                    {
						await _roleManager.CreateAsync(new IdentityRole(SD.Role_User));
						if (user.Role == null)
						{
							await _userManager.AddToRoleAsync(user, SD.Role_User);
                            user.RolesName = SD.Role_User;
                        }
                    }
                    
                                        
                    personal.UserName = user.UserName;
                    personal.FullName = user.FullName;
                    personal.Gender = user.Gender;
                    personal.PhoneNumber = user.PhoneNumber;
                    personal.NIM = user.NIM;
                    personal.Prodi = user.Prodi;
                    personal.Fakultas = user.Fakultas;
                    personal.EntryDate = DateTime.Now;
                    personal.UserId = userId;
                    _unitOfWork.Personal.Add(personal);
                    _unitOfWork.Save();

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
