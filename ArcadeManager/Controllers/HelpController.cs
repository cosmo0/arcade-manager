using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ArcadeManager.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult Basics() => View();

        public IActionResult CustomCsv() => View();

        public IActionResult DatFiles() => View();

        public IActionResult Emulators() => View();

        public IActionResult Install() => View();

        public IActionResult KnownSystems() => View();

        public IActionResult Romsets() => View();

        public IActionResult Shares() => View();

        public IActionResult Tips() => View();

        public IActionResult What() => View();
    }
}
