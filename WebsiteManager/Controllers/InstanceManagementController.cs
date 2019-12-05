using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteManager.Data;
using WebsiteManager.Models;
using WebsiteManager.Services;

namespace WebsiteManager.Controllers
{
    [Authorize]
    public class InstanceManagementController : Controller
    {
        private readonly ILogger<InstanceManagementController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IInstanceManagerService _instanceManagerService;

        public InstanceManagementController(
            UserManager<ApplicationUser> userManager,
            ILogger<InstanceManagementController> logger,
            ApplicationDbContext applicationDbContext,
            IInstanceManagerService instanceManagerService)
        {
            _userManager = userManager;
            _logger = logger;
            _dbContext = applicationDbContext;
            _instanceManagerService = instanceManagerService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _dbContext.Users
                .AsNoTracking()
                .Include(user => user.InstanceConfigurations)
                .FirstOrDefaultAsync(user => user.Id == userId);

            //var userInstanceConfigurations = await _dbContext.UserInstanceConfigurations
            //    .AsNoTracking()
            //    .Where(userInstanceConfiguration => userInstanceConfiguration.UserId == userId)
            //    .ToListAsync();

            //var instanceConfigurations = await _dbContext.InstanceConfigurations
            //    .AsNoTracking()
            //    .Where(instanceConfiguration => userInstanceConfigurations.Any(userInstanceConfiguration => userInstanceConfiguration.InstanceConfigurationId == instanceConfiguration.Id))
            //    .ToListAsync();

            return View(user.InstanceConfigurations);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id, Name")]InstanceConfiguration instanceConfiguration)
        {
            if (_dbContext.InstanceConfigurations.Any(instanceConfiguration0 => instanceConfiguration0.Name == instanceConfiguration.Name))
            {
                ModelState.AddModelError("Name", "There is already a instance configuration with that name in the database.");
                return View(instanceConfiguration);
            }
            else
            {
                var isInstanceCreated = await _instanceManagerService.TryCreateNewInstance(instanceConfiguration);

                if (isInstanceCreated)
                {
                    _dbContext.InstanceConfigurations.Add(instanceConfiguration);
                    var userId = _userManager.GetUserId(User);
                    var user = await _dbContext.Users
                        .Include(user => user.InstanceConfigurations)
                        .FirstOrDefaultAsync(user => user.Id == userId);

                    if (user == null)
                    {
                        TempData["SuccessMessage"] = "User was null";
                        return View(instanceConfiguration);
                    }

                    user.InstanceConfigurations.Add(instanceConfiguration);
                    _dbContext.Users.Update(user);
                    await _dbContext.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Created instance: {instanceConfiguration.Name}";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Failed to create instance: {instanceConfiguration.Name}";
                    return View(instanceConfiguration);
                }

                return RedirectToAction(nameof(Index), "InstanceManagement");
            }
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instanceConfiguration = await _dbContext.InstanceConfigurations
                .AsNoTracking()
                .FirstOrDefaultAsync(instanceConfiguration0 => instanceConfiguration0.Id == id);

            if (instanceConfiguration == null)
            {
                return NotFound();
            }

            return View(instanceConfiguration);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instanceConfiguration = await _dbContext.InstanceConfigurations
                 .AsNoTracking()
                 .FirstOrDefaultAsync(instanceConfiguration0 => instanceConfiguration0.Id == id);

            var wasInstanceDeleted = _instanceManagerService.TryDeleteInstance(instanceConfiguration);

            if (wasInstanceDeleted)
            {
                _dbContext.InstanceConfigurations.Remove(instanceConfiguration);
                await _dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Removed instance: {instanceConfiguration.Name}";
                return RedirectToAction(nameof(Index), "InstanceManagement");
            }
            else
            {
                TempData["SuccessMessage"] = $"Instance could not be removed removed: {instanceConfiguration.Name}";
                return RedirectToAction(nameof(Index), "InstanceManagement");
            }
        }
    }
}
