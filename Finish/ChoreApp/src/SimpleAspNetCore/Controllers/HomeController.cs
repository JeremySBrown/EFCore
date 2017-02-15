using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChoreApp;
using ChoreApp.Contracts;
using SimpleAspNetCore.Models;

namespace SimpleAspNetCore.Controllers
{
    public class HomeController : Controller
    {
        public IChoreRepository Repo { get; private set; }

        public HomeController(IChoreRepository repo)
        {
            Repo = repo;
        }
        public IActionResult Index()
        {
            ViewData["users"] = Repo.GetAllUsers().Select(user => new UserViewModel { Name = user.Name }).ToList();
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
