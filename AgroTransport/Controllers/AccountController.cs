using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AgroTransport.Models;
using AgroTransport.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgroTransport.Controllers
{
    /// <summary>
    /// Handles user registration, authentication, authorization, and profile management.
    /// Demonstrates Unit IV ASP.NET Core Identity & Role-based Security.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? role = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectBasedOnRole();
            }

            var model = new RegisterViewModel
            {
                UserRole = (role?.Equals("Transporter", StringComparison.OrdinalIgnoreCase) == true) ? "Transporter" : "Farmer"
            };

            return View(model);
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Security Rule: Prevent direct administrator registration
            if (model.UserRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("UserRole", "Administrator accounts cannot be self-registered.");
                return View(model);
            }

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "An account with this email address already exists.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName.Trim(),
                PhoneNumber = model.MobileNumber.Trim(),
                UserRole = model.UserRole,
                Village = model.Village.Trim(),
                District = model.District.Trim(),
                State = model.State.Trim(),
                Address = model.Address?.Trim(),
                IsActive = true,
                ProfileCreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Ensure role exists and assign
                if (!await _roleManager.RoleExistsAsync(model.UserRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.UserRole));
                }
                await _userManager.AddToRoleAsync(user, model.UserRole);

                // Sign in the newly registered user
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["SuccessMessage"] = $"Welcome to AgroTransport, {user.FullName}! Your {model.UserRole} account has been created.";

                if (model.UserRole == "Transporter")
                {
                    return RedirectToAction("Index", "Transporter");
                }
                return RedirectToAction("Index", "Farmer");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectBasedOnRole(returnUrl);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials. Please check your email and password.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Your account has been deactivated by administrator. Please contact support.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Welcome back, {user.FullName}!";
                return RedirectBasedOnRole(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            TempData["InfoMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Account/Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                UserRole = user.UserRole,
                Village = user.Village,
                District = user.District,
                State = user.State,
                Address = user.Address
            };

            return View(model);
        }

        // POST: /Account/Profile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber.Trim();
            user.Village = model.Village?.Trim();
            user.District = model.District?.Trim();
            user.State = model.State?.Trim();
            user.Address = model.Address?.Trim();

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile details updated successfully!";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        private IActionResult RedirectBasedOnRole(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (User.IsInRole("Administrator"))
            {
                return RedirectToAction("Index", "Admin");
            }
            if (User.IsInRole("Transporter"))
            {
                return RedirectToAction("Index", "Transporter");
            }
            return RedirectToAction("Index", "Farmer");
        }
    }
}

