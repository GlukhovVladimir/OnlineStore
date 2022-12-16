using DataAccessLogic.DatabaseModels;
using DataAccessLogic.HelperServices;
using DataAccessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationTechSale.HelperServices;
using WebApplicationTechSale.Models;

namespace WebApplicationTechSale.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IPagination<Item> itemLogic;
        private readonly ISavedLogic savedListLogic;
        private readonly ICrudLogic<User> userLogic;
        private readonly ICrudLogic<Order> orderLogic;

        public AccountController(IPagination<Item> itemLogic, ISavedLogic savedListLogic,
            UserManager<User> userManager, SignInManager<User> signInManager,
            ICrudLogic<User> userLogic, ICrudLogic<Order> orderLogic)
        {
            this.itemLogic = itemLogic;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.savedListLogic = savedListLogic;
            this.userLogic = userLogic;
            this.orderLogic = orderLogic;
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.OldPassword == model.NewPassword)
                {
                    ModelState.AddModelError(string.Empty, "Новый и старый пароли не должны совпадать");
                    return View(model);
                }

                User user = await userManager.FindByNameAsync(User.Identity.Name);

                user.UserName += ApplicationConstantsProvider.AvoidValidationCode();
                user.Email += ApplicationConstantsProvider.AvoidValidationCode();

                var changePasswordResult = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                if (changePasswordResult.Succeeded)
                {
                    return View("Redirect", new RedirectModel
                    {
                        InfoMessages = RedirectionMessageProvider.AccountUpdatedMessages(),
                        RedirectUrl = "/Account/Personal",
                        SecondsToRedirect = ApplicationConstantsProvider.GetShortRedirectionTime()
                    });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Вы ввели неверный старый пароль");
                }
            }
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Personal(int page = 1)
        {
            User user = await userManager.FindByNameAsync(User.Identity.Name);

            List<Item> userItem = new List<Item>();
            int count = 0;

            if (await userManager.IsInRoleAsync(user, "regular user"))
            {
                userItem = await itemLogic.GetPage(page, new Item
                {
                    User = user
                });
                count = await itemLogic.GetCount(new Item
                {
                    User = user
                });
            }

            PersonalAccountViewModel model = new PersonalAccountViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                PersonalItemList = new ItemViewModel
                {
                    Item = userItem,
                    PageViewModel = new PageViewModel(count, page, ApplicationConstantsProvider.GetPageSize())
                }
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Item", "Home");
            }
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult loginResult;
                User user = await userManager.FindByEmailAsync(model.Login);
                if (user != null)
                {
                    loginResult = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                }
                else
                {
                    user = await userManager.FindByNameAsync(model.Login);
                    loginResult = await signInManager.PasswordSignInAsync(model.Login, model.Password, model.RememberMe, false);
                }

                if (loginResult.Succeeded)
                {
                    if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {

                        if (await userManager.IsInRoleAsync(user, "admin"))
                        {
                            return RedirectToAction("UsersList", "Admin");
                        }
                        if (await userManager.IsInRoleAsync(user, "moderator"))
                        {
                            return RedirectToAction("Item", "Moderator");
                        }
                        if (await userManager.IsInRoleAsync(user, "regular user"))
                        {
                            return RedirectToAction("Item", "Home");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Неверный логин или пароль");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Item", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Item", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                
                User user = new User
                {
                    Email = model.Email,
                    UserName = model.UserName,
                };

                var registerResult = await userManager.CreateAsync(user, model.Password);
                if (registerResult.Succeeded)
                {
                    user.Email += ApplicationConstantsProvider.AvoidValidationCode();
                    user.UserName += ApplicationConstantsProvider.AvoidValidationCode();
                    await userManager.AddToRoleAsync(user, "regular user");
                    await savedListLogic.Create(user);
                    await signInManager.SignInAsync(user, false);
                    return View("Redirect", new RedirectModel
                    {
                        InfoMessages = RedirectionMessageProvider.AccountCreatedMessages(),
                        RedirectUrl = "/Home/Item",
                        SecondsToRedirect = ApplicationConstantsProvider.GetShortRedirectionTime()
                    });
                }
                else
                {
                    foreach (var error in registerResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        [Authorize(Roles = "regular user")]
        [HttpGet]
        public IActionResult Update()
        {
            return View();
        }

        [Authorize(Roles = "regular user")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                User userToUpdate = await userManager.FindByNameAsync(User.Identity.Name);

                if (!string.IsNullOrWhiteSpace(model.NewEmail))
                {

                    if (model.NewEmail == userToUpdate.Email)
                    {
                        ModelState.AddModelError(string.Empty, "Новый email совпадает со старым");
                        return View(model);
                    }
                    else
                    {
                        userToUpdate.UserName += ApplicationConstantsProvider.AvoidValidationCode();
                        var updateEmailResult = await userManager.SetEmailAsync(userToUpdate, model.NewEmail);
                        if (!updateEmailResult.Succeeded)
                        {
                            foreach (var updateEmailError in updateEmailResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, updateEmailError.Description);
                            }
                            return View(model);
                        }
                    }
                }
                return RedirectToAction("Personal", "Account");
            }
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            User user = await userManager.FindByNameAsync(User.Identity.Name);

            List<Order> userOrders = await orderLogic.Read(new Order
            {
                User = new User
                {
                    UserName = user.UserName
                }
            });

            return View(userOrders);
        }
    }
}