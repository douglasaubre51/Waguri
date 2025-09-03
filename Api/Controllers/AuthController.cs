using Api.Data;
using Api.Dtos.WaguriDtos;
using Api.Models;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics;
using System.Text;

namespace Api.Controllers
{
    [Route("[controller]")]
    public class AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        JwtTokenProvider jwtTokenProvider,
        SessionService sessionService,
        AuthService authService,
        ClientStorageService clientStorageService,
        ClientRepository clientRepository
        ) : Controller
    {
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly UserManager<User> _userManager = userManager;
        private readonly JwtTokenProvider _jwtTokenProvider = jwtTokenProvider;
        private readonly SessionService _sessionService = sessionService;
        private readonly AuthService _authService = authService;
        private readonly ClientStorageService _clientStorageService = clientStorageService;
        private readonly ClientRepository _clientRepository = clientRepository;


        [HttpGet("Login/{projectId}")]
        public IActionResult Login([FromRoute] string projectId)
        {
            try
            {
                Console.WriteLine("1.hit Login(get)!");
                Console.WriteLine("projectId : " + projectId);

                var url = _clientRepository.GetClientUrlById(projectId);
                HttpContext.Session.SetString("projectId", projectId);
                HttpContext.Session.SetString("projectUrl", url);

                return View();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login(get) error: {ex}");
                return View();
            }
        }

        // Login view for redirecting from EmailConfirmation view!
        [HttpGet("Login")]
        public IActionResult Login()
        {
            Console.WriteLine("hit login(get) emailconfirm!");
            return View();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromForm] LoginDto dto)
        {
            try
            {
                Console.WriteLine("3.hit Login(post)!");
                if (_sessionService.IsExpired(HttpContext) is true)
                    Console.WriteLine("session expired -Login");
                if (ModelState.IsValid is false)
                    return View(dto);

                var result = await _signInManager.PasswordSignInAsync(
                    dto.EmailId,
                    dto.Password,
                    false,
                    false
                    );
                if (result.Succeeded is false)
                {
                    dto.ErrorMessage = "invalid email or password!";
                    return View(dto);
                }
                // login success
                var user = await _userManager.FindByEmailAsync(dto.EmailId);
                var token = _jwtTokenProvider.CreateToken(user);
                string url = HttpContext.Session.GetString("projectUrl");
                url += $"/login/{token}";

                // go back to client!
                return Redirect(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex}");
                return View();
            }
        }

        [HttpGet("SignUp")]
        public IActionResult SignUp()
        {
            Console.WriteLine("4.hit SignUp(get)!");
            return View();
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromForm] SignUpDto dto)
        {
            try
            {
                Console.WriteLine("5.hit SignUp(post)!");
                if (_sessionService.IsExpired(HttpContext) is true)
                    Console.WriteLine("session expired -SignUp");
                if (ModelState.IsValid is false)
                    return View(dto);

                var user = new User
                {
                    UserName = dto.EmailId,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.EmailId,
                    ProjectId = HttpContext.Session.GetString("projectId")
                };
                IdentityResult? result = await _userManager.CreateAsync(user, dto.Password);
                if (result.Succeeded is false)
                {
                    Console.WriteLine($"account for {dto.EmailId} couldnot be created!");
                    foreach (var e in result.Errors)
                        Console.WriteLine(e.Description);

                    return View();
                }

                var dbUser = await _userManager.FindByEmailAsync(dto.EmailId);
                await _authService.GetConfirmationEmail(dbUser);

                return RedirectToAction("EmailConfirmation");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignUp error: {ex}");
                return View();
            }
        }

        [HttpGet("EmailConfirmation")]
        public IActionResult EmailConfirmation()
        {
            Console.WriteLine("6.hit EmailConfirmation(get)!");
            return View();
        }

        [HttpGet("ConfirmEmail/{userId}/{code}")]
        public async Task<IActionResult> ConfirmEmail(
            [FromRoute] string userId,
            [FromRoute] string code
            )
        {
            try
            {
                Console.WriteLine("7.hit EmailConfirmed(get)!");
                if (_sessionService.IsExpired(HttpContext) is true)
                    Console.WriteLine("session expired -SignUp");

                var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    Console.WriteLine("user doesnot exists!");
                    return View();
                }
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded is false)
                {
                    Console.WriteLine($"couldn't confirm email for : {user.UserName}");
                    return View();
                }

                // save user to client db
                Dtos.ClientDtos.UserDto dto = new()
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
                string url = _sessionService.GetProjectUrl(HttpContext);
                url += "/user/create";
                bool status = await _clientStorageService.CreateUserOnClient(url, dto);
                if (status is false)
                {
                    Console.WriteLine($"couldn't create user account on client");
                    return View();
                }

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ConfirmEmail error:\n{ex}");
                return View();
            }
        }
    }
}
