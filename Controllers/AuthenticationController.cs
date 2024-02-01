using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Part_2_CreateUser.Models;
using Part_2_CreateUser.Models.Authentication.SignUp;
using User.Management.Service.Models;
using User.Management.Service.Services;

namespace Part_2_CreateUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IServiceEmail _emailService;

        public AuthenticationController(UserManager<IdentityUser> userManager,
                                        RoleManager<IdentityRole> roleManager,
                                        IServiceEmail emailService )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
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

            
            if ( await _roleManager.RoleExistsAsync(role)) // check the role exist or not
            {
                var result = await _userManager.CreateAsync(user, registerUser.Password);
                if (!result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "User failed to create !" });
                }
                // Add role to the user
                 await _userManager.AddToRoleAsync(user, role);

                // Add tokne to verifiy the email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authentication", new {token, email = user.Email}, Request.Scheme);
                var message = new Message(new string[] { user.Email! }, "Confirmation email link", confirmationLink);
                _emailService.SendEmail(message);


                return StatusCode(StatusCodes.Status200OK,
                        new Response { Status = "Success", Message = $"Usre Created email sent to {user.Email} successfully" });
                    
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "this Role Doesnot exists" });
            }

           



        }


        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByIdAsync(email);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK,
                                    new Response { Status = "success", Message = "Email varified     successfylly !" });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "This user doesnot exist" });
        }
    }
}
