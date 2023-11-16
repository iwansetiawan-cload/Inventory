using Microsoft.AspNetCore.Mvc;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using Microsoft.Extensions.Hosting;
using E_OneWeb.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.UI.Services;
using E_OneWeb.DataAccess.Data;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Security.Claims;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ArticleController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        //private readonly ApplicationUser _loginUser;
        private readonly ApplicationDbContext _db;

        public ArticleController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;

        }
        public IActionResult Index()
        {
            return View();
        }
        //private readonly SignInManager<IdentityUser> _signInManager;
        //public ArticleController(SignInManager<IdentityUser> signInManager )
        //{
        //    _signInManager = signInManager;
        //}
        //public IList<AuthenticationScheme> ExternalLogins { get; set; }
    
        [HttpGet]
        public IActionResult GetAll()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            //var allObj = _unitOfWork.Article.GetAll().Where(z=>z.PhoneNumber == user.UserName);

            var datalist = (from z in _unitOfWork.Article.GetAll().Where(z => z.PhoneNumber == user.UserName)
                            select new
                            {
                                id = z.Id,
                                title = z.Title,
                                point = z.Point,
                                pointdesc = z.PointDesc,
                                entrydate = z.EntryDate != null ? Convert.ToDateTime(z.EntryDate).ToString("dd-MM-yyyy") : "",
                                flag = z.Flag == 0 ? "One Time Only" : "Repeated Points"
                            }).ToList();
            return Json(new { data = datalist });
        }


        public async Task<IActionResult> Upsert(int? id)
        {           
            ArticleVM articleVM = new ArticleVM();         
            ViewBag.Status = "";
            if (id == null)
            {
                //this is for create
                return View(articleVM);
            }
            //this is for edit
            articleVM.Article = _unitOfWork.Article.Get(id.GetValueOrDefault());
            if (articleVM.Article == null)
            {
                return NotFound();
            }
            return View(articleVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ArticleVM articleVM)
        {
           
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

                if (articleVM.Article.Id == 0)
                {
                    articleVM.Article.EntryBy = user.Name;
                    articleVM.Article.PhoneNumber = user.UserName;
                    articleVM.Article.EntryDate = DateTime.Now;
                    _unitOfWork.Article.Add(articleVM.Article);
                    ViewBag.Status = "Save Success";

                }
                else
                {
                    _unitOfWork.Article.Update(articleVM.Article);
                    ViewBag.Status = "Edit Success";
                }
            
                _unitOfWork.Save();
            }

            return View(articleVM);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Article.Get(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
          
            _unitOfWork.Article.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }
    }
}
