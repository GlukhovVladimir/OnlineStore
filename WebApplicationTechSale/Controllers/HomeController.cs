using DataAccessLogic.DatabaseModels;
using DataAccessLogic.HelperServices;
using DataAccessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationTechSale.Models;

namespace WebApplicationTechSale.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPagination<Item> itemLogic;

        public HomeController(IPagination<Item> itemLogic)
        {
            this.itemLogic = itemLogic;
        }

        [HttpGet]
        public IActionResult Rules()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Item(int page = 1)
        {
            List<Item> itemToDisplay = await itemLogic.GetPage(page, new Item
            {
                Status = ItemStatusProvider.GetAcceptedStatus()
            });

            int itemCount = await itemLogic.GetCount(new Item
            {
                Status = ItemStatusProvider.GetAcceptedStatus()
            });

            return View(new ItemViewModel()
            {
                PageViewModel = new PageViewModel(itemCount, page, ApplicationConstantsProvider.GetPageSize()),
                Item = itemToDisplay
            });
        }
    }
}