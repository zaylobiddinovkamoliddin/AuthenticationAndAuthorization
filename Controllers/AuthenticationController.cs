using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Part_2_CreateUser.Models;
using Part_2_CreateUser.Models.Authentication.Login;
using Part_2_CreateUser.Models.Authentication.SignUp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        private readonly IConfiguration _configuration;

        public AuthenticationController(UserManager<IdentityUser> userManager,
                                        RoleManager<IdentityRole> roleManager,
                                        IServiceEmail emailService,
                                        IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
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


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel )
        {
            // checking user isvalid or not
            // checking the password

            var user = await _userManager.FindByNameAsync(loginModel.Username);
            // claimlist creation 

            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {

                var userClaim = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                var userRole = await _userManager.GetRolesAsync(user);
                // we add roles to the list

                foreach (var role in userRole)
                {
                    userClaim.Add(new Claim(ClaimTypes.Role, role));
                }

                // returning the token
                var jwtToken = GetToken(userClaim);
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expiration = jwtToken.ValidTo
                });
            }

            return Unauthorized();

        }

        // generate token with the list

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT: Validaudience"],
                expires: DateTime.Now.AddHours(3),  
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return token;


                

        }
    }
}
