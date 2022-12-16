using DataAccessLogic.DatabaseModels;
using DataAccessLogic.HelperServices;
using DataAccessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationTechSale.HelperServices;
using WebApplicationTechSale.Models;

namespace WebApplicationTechSale.Controllers
{
    [Authorize(Roles = "regular user")]
    public class UserController : Controller
    {
        private readonly ICrudLogic<Item> itemLogic;
        private readonly IWebHostEnvironment environment;
        private readonly ISavedLogic savedListLogic;
        private readonly UserManager<User> userManager;
        private readonly ICrudLogic<Order> orderLogic;

        public UserController(ICrudLogic<Item> itemLogic, IWebHostEnvironment environment,
            UserManager<User> userManager, ISavedLogic savedListLogic, ICrudLogic<Order> orderLogic)
        {
            this.itemLogic = itemLogic;
            this.environment = environment;
            this.userManager = userManager;
            this.savedListLogic = savedListLogic;
            this.orderLogic = orderLogic;
        }

        [HttpGet]
        public IActionResult CreateItem()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItem(CreateItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                Item toAdd = new Item
                {
                    Name = model.Name,
                    User = new User
                    {
                        UserName = User.Identity.Name
                    },
                    Description = model.Description,
                    Price = (int)model.Price
                };

                string dbPhotoPath = $"/images/{User.Identity.Name}/{model.Name}/photo{Path.GetExtension(model.Photo.FileName)}";
                toAdd.PhotoSrc = dbPhotoPath;

                try
                {
                    await itemLogic.Create(toAdd);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }

                string physicalDirectory = Path.GetDirectoryName($"{environment.WebRootPath + dbPhotoPath}");
                if (!Directory.Exists(physicalDirectory))
                {
                    Directory.CreateDirectory(physicalDirectory);
                }

                using (FileStream fs = new FileStream($"{environment.WebRootPath + dbPhotoPath}", FileMode.Create))
                {
                    await model.Photo.CopyToAsync(fs);
                }

                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.ItemCreatedMessages(),
                    RedirectUrl = "/Home/Item",
                    SecondsToRedirect = ApplicationConstantsProvider.GetMaxRedirectionTime()
                });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> OpenItem(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                Item item = (await itemLogic.Read(new Item
                {
                    Id = id
                })).First();

                User user = await userManager.FindByNameAsync(User.Identity.Name);

                SavedList userList = await savedListLogic.Read(user);

                if (userList.Item.Any(item => item.Id == id))
                {
                    ViewBag.IsSaved = true;
                }
                else
                {
                    ViewBag.IsSaved = false;
                }

                if (item == null)
                {
                    return NotFound();
                }
                return View(item);
            }
            return NotFound();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveItem(string itemId)
        {
            if (!string.IsNullOrWhiteSpace(itemId))
            {
                User user = await userManager.FindByNameAsync(User.Identity.Name);
                Item itemToAdd = new Item { Id = itemId };
                await savedListLogic.Add(user, itemToAdd);
                return RedirectToAction("OpenItem", "User", new { id = itemId });
            }
            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> MySavedList()
        {
            User user = await userManager.FindByNameAsync(User.Identity.Name);

            SavedList userSavedList = await savedListLogic.Read(user);

            return View(userSavedList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(string itemId)
        {
            if (!string.IsNullOrWhiteSpace(itemId))
            {
                User user = await userManager.FindByNameAsync(User.Identity.Name);
                Item itemToAdd = new Item { Id = itemId };
                await savedListLogic.Remove(user, itemToAdd);
                return RedirectToAction("OpenItem", "User", new { id = itemId });
            }
            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> EditItem(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                Item itemToEdit = (await itemLogic.Read(new Item { Id = id })).First();
                if (itemToEdit.Status == ItemStatusProvider.GetRejectedStatus()
                    || itemToEdit.Status == ItemStatusProvider.GetAcceptedStatus())
                {
                    if (itemToEdit.Status == ItemStatusProvider.GetRejectedStatus())
                    {
                        ViewBag.RejectNote = "Причина, по которой ваш товар не был опубликован: "
                            + itemToEdit.Note.Text;
                    }
                    else
                    {
                        ViewBag.RejectNote = string.Empty;
                    }
                    return View(new EditItemViewModel
                    {
                        Id = itemToEdit.Id,
                        Description = itemToEdit.Description,
                        Name = itemToEdit.Name,
                        OldName = itemToEdit.Name,
                        Price = itemToEdit.Price,
                        OldPhotoSrc = itemToEdit.PhotoSrc
                    });
                }
                else
                {
                    return NotFound();
                }
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(EditItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(model.Id))
                {
                    return NotFound();
                }

                Item itemToEdit = new Item
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    Status = ItemStatusProvider.GetOnModerationStatus(),
                    Price = (int)model.Price
                };

                string newDbPath = $"/images/{User.Identity.Name}/{model.Name}/photo{Path.GetExtension(model.Photo.FileName)}";
                itemToEdit.PhotoSrc = newDbPath;

                try
                {
                    await itemLogic.Update(itemToEdit);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }

                string oldPath = $"{environment.WebRootPath + Path.GetDirectoryName(model.OldPhotoSrc)}";
                if (Directory.Exists(oldPath))
                {
                    Directory.Delete(oldPath, true);
                }

                string newPhysicalDirectory = Path.GetDirectoryName($"{environment.WebRootPath + newDbPath}");

                if (!Directory.Exists(newPhysicalDirectory))
                {
                    Directory.CreateDirectory(newPhysicalDirectory);
                }

                using (FileStream fs = new FileStream($"{environment.WebRootPath + newDbPath}", FileMode.Create))
                {
                    await model.Photo.CopyToAsync(fs);
                }

                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.ItemUpdatedMessages(),
                    RedirectUrl = "/Home/Item",
                    SecondsToRedirect = ApplicationConstantsProvider.GetMaxRedirectionTime()
                });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(string itemId)
        {
            await orderLogic.Create(new Order
            {
                ItemId = itemId,
                UserName = User.Identity.Name
            });

            return View("Redirect", new RedirectModel
            {
                InfoMessages = RedirectionMessageProvider.OrderCreateMessage(),
                RedirectUrl = "/Account/MyOrders",
                SecondsToRedirect = ApplicationConstantsProvider.GetShortRedirectionTime()
            });
        }
    }
}