using Frontend.Areas.Admin.Models;
using Frontend.Controllers;
using Frontend.Core.HttpClients;
using Frontend.DTO.Request;
using Frontend.DTO.Response;
using Frontend.DTO.Response.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using NToastNotify;

namespace Frontend.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class AdministrativeController : BaseController
    {
        private readonly IGenericHttpClient _client;
        private readonly IToastNotification _toastService;

        public AdministrativeController(IGenericHttpClient client,
            IToastNotification toastService) : base(toastService)
        {
            _client = client;
            _toastService = toastService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            DashbordViewModel model = new DashbordViewModel();
            try
            {
                var result = await _client.GetAsyncSingle<DashbordViewModel>(ApiConstants.DashboardList);
                return View(result.Response);
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
                return View(model);
            }

        }




        [HttpGet]
        public async Task<IActionResult> RolesList()
        {
            return View();

        }

        [HttpGet]
        public async Task<JsonResult> RoleListData()
        {
            try
            {
                var result = await _client.GetAllAsync<IdentityRole>(ApiConstants.GetAllRoles);
                return Json(result.Response);
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
                return Json(ex.Message);
            }


        }

        [HttpGet]
        public async Task<IActionResult> SingleRole(string roleId)
        {
            try
            {
                var result = await _client.GetAsync<IdentityRole>($"{ApiConstants.SingleRole}?roleId={roleId}");
                return View(result);
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateRole()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole(Result<RoleRequestDTO> model)
        {
            try
            {
                var result = await _client.PostAsync<SuccessResultDTO>(ApiConstants.GetAllRoles, model.Response);
                return RedirectToAction(nameof(RolesList));
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            try
            {
                var result = await _client.GetAsyncSingle<RoleRequestDTO>($"{ApiConstants.SingleRole}?roleId={id}");
                return View(result.Response);
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
                return View();
            }
        }
        [HttpGet]
        public async Task<IActionResult> UserInrole(string roleid)
        {
            try
            {
                var result = await _client.GetAllAsync<UserResponseDTO>($"{ApiConstants.UserInRole}?roleId={roleid}");
                return View(result.Response);
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
                return View();
            }



        }


        [HttpPost]
        public async Task<IActionResult> EditRole(RoleRequestDTO model)
        {
            try
            {
                var result = await _client.PutAsync<SuccessResultDTO>(ApiConstants.SingleRole, model);

                if (result.Response.Succeeded)
                    _toastService.AddSuccessToastMessage(result.Response.Message);

                return RedirectToAction(nameof(RolesList));
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddRemoveRoles(string userId)
        {
            try
            {
                var result = await _client.GetAsyncSingle<AddRemoveRoles>($"{ApiConstants.UserRoles}?userId={userId}");
                return View(result);
            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddRemoveRoles(Result<AddRemoveRoles> model)
        {
            try
            {
                var result = await _client.PostAsync<SuccessResultDTO>(ApiConstants.UserRoles, model.Response);
                if (result.Response.Succeeded)
                    _toastService.AddSuccessToastMessage(result.Response.Message);
                return RedirectToAction("UsersList", "Account",new { area="default"});

            }
            catch (Exception ex)
            {
                DisplayErrors(ex);
                return View();
            }


        }

    }
}
