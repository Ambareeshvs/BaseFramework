using Frontend.Core.HttpClients;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Frontend.DTO.Request;
using Frontend.DTO.Response;
using Newtonsoft.Json;
using NToastNotify;
using Frontend.DTO.Response.Common;
using Microsoft.AspNetCore.Authorization;

namespace Frontend.Controllers
{
    
    public class AccountController : BaseController
    {
        private readonly IGenericHttpClient _client;
        private readonly IWebHostEnvironment _env;
        private IToastNotification _toastService;
        public string Roles = "";
        public AccountController(IGenericHttpClient client,
            IWebHostEnvironment env, IToastNotification toastService) : base(toastService)
        {
            _client = client;
            _env = env;
            _toastService = toastService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.FindFirstValue("Roles")=="T")
                {
                    return RedirectToAction("index", "Administrative", new { area = "Admin" });

                }
                else
                {
                    return RedirectToAction("GetAllNews", "News", new { area = "default" });

                }
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequestDTO model)
        {
            try
            {
                var result = await _client.PostAsync<UserResponseDTO>(ApiConstants.Login, model);
                if (result.Response != null)
                {
                    var flag = await CreateIdentity(result.Response);
                    if (flag)
                    {
                        if (Roles=="F")
                        {
                            _toastService.AddSuccessToastMessage($"Welcome {result.Response.firstName} {result.Response.lastName}");
                            return RedirectToAction("GetAllNews", "News", new { area = "default" });
                        }
                        else
                        {
                            _toastService.AddSuccessToastMessage($"Welcome {result.Response.firstName} {result.Response.lastName}");
                            return RedirectToAction("index", "Administrative", new { area = "Admin" });

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
            }
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            RegistrationRequestDTO user = new RegistrationRequestDTO();
            user.DOB = DateTime.Parse("2006/01/01");
            user.DOJ = DateTime.Now;
            return View(user);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegistrationRequestDTO model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    if (model.File != null)
                    {
                        string folderPath = Path.Combine(_env.WebRootPath, "Images");
                        string filePath = Path.Combine(folderPath, model.File.FileName);
                        model.File.CopyTo(new FileStream(filePath, FileMode.Create));
                        model.Image = model.File.FileName;
                    }
                    var result = await _client.PostAsync<SuccessResultDTO>(ApiConstants.Register, model);

                    if (result.Response.Succeeded)
                        _toastService.AddSuccessToastMessage(result.Response.Message);

                    return RedirectToAction(nameof(Login));
                }
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
            }
            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> EditUser(string userId)
        {
            try
            {
                var result = await _client.GetAsync<UserResponseDTO>($"{ApiConstants.SingleUser}?userId={userId}");
                return View(result);
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> /*EditUser*/ Profile(Result<UserResponseDTO> model)
        {
            try
            {
                RegistrationRequestDTO updateModel = new RegistrationRequestDTO()
                {
                    UserId = model.Response.userId,
                    EmailId = model.Response.user.email,
                    PhoneNumber = model.Response.user.phoneNumber,
                    FirstName = model.Response.firstName,
                    LastName = model.Response.lastName,
                    DOB = model.Response.dob,
                    DOJ = model.Response.doj,
                    Gender = model.Response.gender,
                    Address = model.Response.address,
                    Image = model.Response.image,
                    Password = "DefaultPassword",
                    ConfirmPassword = "DefaultPassword",
                };

                if (model.Response.File != null)
                {
                    string folderPath = Path.Combine(_env.WebRootPath, "Images");
                    string filePath = Path.Combine(folderPath, model.Response.File.FileName);
                    model.Response.File.CopyTo(new FileStream(filePath, FileMode.Create));
                    updateModel.Image = model.Response.File.FileName;
                }

                var result = await _client.PutAsync<SuccessResultDTO>($"{ApiConstants.SingleUser}", updateModel);

                if (result.Response.Succeeded)
                    _toastService.AddSuccessToastMessage(result.Response.Message);

                return RedirectToAction(nameof(Logout));
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails(string userId)
        {
            try
            {
                var result = await _client.GetAsync<UserResponseDTO>($"{ApiConstants.SingleUser}?userId={userId}");
                return View(result);
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
                return View();
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDTO model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var result = await _client.PutAsync<SuccessResultDTO>(ApiConstants.ChangePassword, model);
                    if (result.Response.Succeeded)
                    {
                        _toastService.AddSuccessToastMessage(result.Response.Message);
                        await HttpContext.SignOutAsync();
                        return RedirectToAction(nameof(Login));
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _client.GetAsyncSingle<UserResponseDTO>($"{ApiConstants.SingleUser}?userId={userId}");
                return View(result);
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
                return View();
            }
        }

        [NonAction]
        private async Task<bool> CreateIdentity(UserResponseDTO model)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, model.userId),
                new Claim(ClaimTypes.Name, model.user.userName),
                new Claim(ClaimTypes.Email, model.user.email),
                new Claim(ClaimTypes.GivenName,$"{model.firstName} {model.lastName}"),
                new Claim(ClaimTypes.MobilePhone, model.user.phoneNumber),
                new Claim("Images", model.image ?? "Blank.png"),
                new Claim("Roles", model?.Role?.Count() > 0 ? "T" : "F")

            };

            foreach(var item in model.Role)
            {
                claims.Add(new Claim(ClaimTypes.Role,item));
            }

            Roles = model?.Role?.Count() > 0 ? "T" : "F";
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = false });

            return true;
        }

        [NonAction]
        public void DisplayErrors(Exception ex)
        {
            if (ex.Message.Contains("Response", StringComparison.CurrentCultureIgnoreCase))
            {
                var errObject = JsonConvert.DeserializeObject<Result<Error>>(ex.Message);
                foreach (var err in errObject.Errors)
                {
                    _toastService.AddErrorToastMessage($"Status Code: {err.StatusCode}</br>Error Message: {err.ErrorMessage}" +
                        $"<br>{err.InnerException}");
                }
            }
            else
            {
                _toastService.AddErrorToastMessage(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> UsersList()
        {
            return View();

        }


        [HttpGet]
        public async Task<JsonResult> UsersListdata()
        {
            try
            {
                var result = await _client.GetAllAsync<UserResponseDTO>(ApiConstants.GetAllUsers);
                return Json(result.Response);
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
                return Json(ex.Message);
            }
        }

        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> LandingPage()
        {
            var response = await _client.GetAllAsync<NewsResponseDTO>(ApiConstants.GetAllNews);
            return View(response.Response);
        }
    }
}
