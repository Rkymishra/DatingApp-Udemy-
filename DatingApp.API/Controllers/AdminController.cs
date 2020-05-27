using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers {
    [Route ("api/[controller]")]
    public class AdminController : ControllerBase {
        public DataContext _context { get; set; }
        public UserManager<User> _userManager { get; set; }
        public AdminController (DataContext context, UserManager<User> userManager) {
            _userManager = userManager;
            _context = context;

        }

        [Authorize (Policy = "RequireAdminRole")]
        [HttpGet ("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles () {
            var usersList = await _context.Users.OrderBy (x => x.UserName).Select (user => new {
                Id = user.Id,
                    UserName = user.UserName,
                    Roles = (from userRole in user.UserRoles join role in _context.Roles on userRole.RoleId equals role.Id select role.Name).ToList ()
            }).ToListAsync ();
            return Ok (usersList);
        }

        [Authorize (Policy = "ModeratePhotoRole")]
        [HttpGet ("photosForModeration")]
        public IActionResult GetPhotosForModeration () {
            return Ok ("Admin and moderator can see this!");
        }

        [Authorize (Policy = "RequireAdminRole")]
        [HttpPost ("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles (string userName, RoleEditDto editDto) {
            var user = await _userManager.FindByNameAsync (userName);
            var userRole = await _userManager.GetRolesAsync (user);
            var selectedRoles = editDto.RoleNames;
            selectedRoles = selectedRoles ?? new string[] { };
            var result = await _userManager.AddToRolesAsync (user, selectedRoles.Except (userRole));
            if (!result.Succeeded) {
                return BadRequest ("Failed to add to roles!");
            }
            result = await _userManager.RemoveFromRolesAsync (user, userRole.Except (selectedRoles));
            if (!result.Succeeded) {
                return BadRequest ("Failed to remove roles!");
            }
            return Ok (await _userManager.GetRolesAsync (user));

        }
    }
}