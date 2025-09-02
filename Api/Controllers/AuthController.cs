using Api.Data;
using Api.Dtos.WaguriDtos;
using Api.Models;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Api.Controllers
{
    public class AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        JwtTokenProvider jwtTokenProvider,
        ClientRepository clientRepository,
        SessionService sessionService,
        AuthService authService,
        ClientStorageService clientStorageService
        ) : Controller
    {
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly UserManager<User> _userManager = userManager;
        private readonly JwtTokenProvider _jwtTokenProvider = jwtTokenProvider;
        private readonly ClientRepository _clientRepository = clientRepository;
        private readonly SessionService _sessionService = sessionService;
        private readonly AuthService _authService = authService;
        private readonly ClientStorageService _clientStorageService = clientStorageService;


        [HttpGet("{projectId}")]
        public IActionResult Login([FromRoute] string projectId)
        {
            try
            {
                var url = _clientRepository.GetClientUrlById(projectId);
                HttpContext.Session.SetString("projectId", projectId);
                HttpContext.Session.SetString("projectUrl", url);

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login(get) error: {ex}");
                return RedirectToActionPermanent("Failure", "Error");
            }
        }

        // Login view for redirecting from EmailConfirmation view!
        [HttpGet()]
        public IActionResult Login()
        {
            if (_sessionService.IsExpired(HttpContext) is true)
                return RedirectToActionPermanent("Failure", "Error");

            return View();
        }

        [HttpPost()]
        public async Task<IActionResult> Login([FromForm] LoginDto dto)
        {
            try
            {
                if (_sessionService.IsExpired(HttpContext) is true)
                    return RedirectToActionPermanent("Failure", "Error");
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
                return RedirectPermanent(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex}");
                return RedirectToActionPermanent("Failure", "Error");
            }
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp([FromForm] SignUpDto dto)
        {
            try
            {
                if (_sessionService.IsExpired(HttpContext) is true)
                    return RedirectToActionPermanent("Failure", "Error");
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

                    return RedirectToActionPermanent("Failure", "Error");
                }

                var dbUser = await _userManager.FindByEmailAsync(dto.EmailId);
                await _authService.GetConfirmationEmail(dbUser);

                return RedirectToActionPermanent("EmailConfirmation");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignUp error: {ex}");
                return RedirectToActionPermanent("Failure", "Error");
            }
        }

        [HttpGet]
        public IActionResult EmailConfirmation()
        {
            return View();
        }

        [HttpGet("/sign-up/confirm/{userId}/{code}")]
        public async Task<IActionResult> EmailConfirmed(
            [FromRoute] string userId,
            [FromRoute] string code
            )
        {
            try
            {
                if (_sessionService.IsExpired(HttpContext) is true)
                    return RedirectToActionPermanent("Failure", "Error");

                var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    Console.WriteLine("user doesnot exists!");
                    return RedirectToActionPermanent("Failure", "Error");
                }
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded is false)
                {
                    Console.WriteLine($"couldn't confirm email for : {user.UserName}");
                    return RedirectToActionPermanent("Failure", "Error");
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
                    return RedirectToActionPermanent("Failure", "Error");
                }

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ConfirmSignUp error:\n{ex}");
                return RedirectToActionPermanent("Failure", "Error");
            }
        }
    }
}
