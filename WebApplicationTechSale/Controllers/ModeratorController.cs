using DataAccessLogic.DatabaseModels;
using DataAccessLogic.HelperServices;
using DataAccessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationTechSale.HelperServices;
using WebApplicationTechSale.Models;

namespace WebApplicationTechSale.Controllers
{
    [Authorize(Roles = "moderator")]
    public class ModeratorController : Controller
    {
        private readonly IPagination<Item> paginationItemLogic;
        private readonly ICrudLogic<Item> crudItemLogic;
        private readonly ICrudLogic<Note> crudNoteLogic;

        public ModeratorController(IPagination<Item> paginationItemLogic,
            ICrudLogic<Item> crudItemLogic, ICrudLogic<Note> crudNoteLogic)
        {
            this.paginationItemLogic = paginationItemLogic;
            this.crudItemLogic = crudItemLogic;
            this.crudNoteLogic = crudNoteLogic;
        }

        [HttpGet]
        public async Task<IActionResult> Item(int page = 1)
        {
            List<Item> itemOnModeration = await paginationItemLogic.GetPage(page, new Item
            {
                Status = ItemStatusProvider.GetOnModerationStatus()
            });

            int itemCount = await paginationItemLogic.GetCount(new Item
            {
                Status = ItemStatusProvider.GetOnModerationStatus()
            });

            return View(new ItemViewModel
            {
                Item = itemOnModeration,
                PageViewModel = new PageViewModel(itemCount, page, ApplicationConstantsProvider.GetPageSize())
            });
        }

        [HttpGet]
        public async Task<IActionResult> CheckItem(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                Item itemToCheck = (await crudItemLogic.Read(new Item
                {
                    Id = id
                }))?.First();
                return View(new ItemModerationModel
                {
                    Item = itemToCheck
                });
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptItem(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                await crudItemLogic.Update(new Item
                {
                    Id = id,
                    Status = ItemStatusProvider.GetAcceptedStatus()
                });

                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.ItemAcceptedMessages(),
                    SecondsToRedirect = ApplicationConstantsProvider.GetLongRedirectionTime(),
                    RedirectUrl = "/Moderator/Item"
                });
            }
            return NotFound();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectItem(ItemModerationModel model)
        {
            if (ModelState.IsValid)
            {
                await crudItemLogic.Update(new Item
                {
                    Id = model.Item.Id,
                    Status = ItemStatusProvider.GetRejectedStatus()
                });
                await crudNoteLogic.Delete(new Note
                {
                    ItemId = model.Item.Id
                });
                await crudNoteLogic.Create(new Note
                {
                    ItemId = model.Item.Id,
                    Text = model.RejectNote
                });
                
                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.ItemRejectedMessages(),
                    SecondsToRedirect = ApplicationConstantsProvider.GetLongRedirectionTime(),
                    RedirectUrl = "/Moderator/Item"
                });
            }
            model.Expanded = true;
            return View("CheckItem", model);
        }

    }
}