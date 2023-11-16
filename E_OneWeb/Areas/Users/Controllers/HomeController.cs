using E_OneWeb.DataAccess.Repository;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace E_OneWeb.Areas.Users.Controllers
{
    [Area("Users")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
      
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Article> articleList = _unitOfWork.Article.GetAll();
            return View(articleList);
        }

        public IActionResult Details(int id)
        {
            var articleFromDb = _unitOfWork.Article.GetFirstOrDefault(u => u.Id == id);
            try
            {  
                ArticleVM article = new ArticleVM();
                article.Article = _unitOfWork.Article.GetFirstOrDefault(u => u.Id == id);         
                article.Article.PointExepiredDate = DateTime.Now;
                // untuk type 0 sekali dapat poin
                if (article.Article.Flag == 0) 
                {
                    article.Article.PointDesc = article.Article.PointDesc == null ? 0.002 : article.Article.PointDesc  + 0.002;
                    article.Article.Point = article.Article.Point == null ? 1  : article.Article.Point + 1;
                }
                _unitOfWork.Article.Update(article.Article);
                _unitOfWork.Save();


            }
            catch (Exception ex)
            {

                string err = ex.Message.ToString();
            }
            return View(articleFromDb);
        }
       
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult GetPoint(int id)
        {

            try
            {

                ArticleVM article = new ArticleVM();
                article.Article = _unitOfWork.Article.GetFirstOrDefault(u => u.Id == id);

                //untuk type 1 bisa dapat beberapa poin sesuai dengan timer
                if (article.Article.Flag == 1)
                {
                    DateTime startdate = Convert.ToDateTime(article.Article.PointExepiredDate);
                    DateTime enddate = DateTime.Now;
                    TimeSpan value = enddate.Subtract(startdate);
                    int point_per15second = value.Seconds / 15;
                    article.Article.Point = article.Article.Point != null ? article.Article.Point : 0;
                    article.Article.Point = article.Article.Point + point_per15second;
                    article.Article.PointDesc = article.Article.Point * 0.002;
                    _unitOfWork.Article.Update(article.Article);
                    _unitOfWork.Save();
                }

                
            }
            catch (Exception ex)
            {
                string err = ex.Message.ToString();
            }
           

            return RedirectToAction(nameof(Index));
        }
    }
}