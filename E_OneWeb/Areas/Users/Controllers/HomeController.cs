﻿using E_OneWeb.DataAccess.Repository;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        public async Task<IActionResult> Index()
        {
            var countNotice = (from z in await  _unitOfWork.VehicleReservationUser.GetAllAsync()
                               select new GridVehicleReservationAdmin
                               {
                                   id = z.Id,
                                   flag = z.Flag
                               }).Where(i => i.flag == 1).Count();
          
            if (countNotice > 0)
            {
                HttpContext.Session.SetInt32(SD.ssNotice, countNotice);
            }
            else
            {
                HttpContext.Session.SetString(SD.ssNotice, "o");
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            var datalist = (from z in await _unitOfWork.RoomReservationUser.GetAllAsync(includeProperties: "RoomReservationAdmin")
                            select new
                            {
                                id = z.Id,
                                roomname = z.RoomReservationAdmin.RoomName + " (" + z.RoomReservationAdmin.LocationName + ")",
                                startdate = Convert.ToDateTime(z.StartDate).ToString("dd/MM/yyyy"),
                                enddate = Convert.ToDateTime(z.EndDate).ToString("dd/MM/yyyy"),
                                status = z.Status,
                                description = z.Description,
                                entryby = z.EntryBy,
                            }).Where(i => i.entryby == user.ToString()).ToList().OrderByDescending(o => o.id);

            return Json(new { data = datalist });
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
        public IActionResult ManualBook()
        {
            return View();
        }
    }
}