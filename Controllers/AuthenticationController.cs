using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Part_2_CreateUser.Models;
using Part_2_CreateUser.Models.Authentication.SignUp;

namespace Part_2_CreateUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticationController(UserManager<IdentityUser> userManager,
                                        RoleManager<IdentityRole> roleManager,
                                        IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Gegister([FromBody] RegisterUser registerUser, string role)
        {
            // Chack User exist or not
            var userExists = await _userManager.FindByEmailAsync(registerUser.Email);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                                new Response { Status = "Error", Message = "User already exists !" }
                                );
            }

            // Add User to the database 
            IdentityUser user = new()
            {
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUser.UserName,
            };

            var result = await _userManager.CreateAsync(user, registerUser.Password);
            return result.Succeeded
                ? StatusCode(StatusCodes.Status201Created,
                new Response { Status = "Success", Message = "User created successfully !" })
                : StatusCode(StatusCodes.Status500InternalServerError,
                new Response { Status = "Error", Message = "User failed to create !" });

        }
    }
}
