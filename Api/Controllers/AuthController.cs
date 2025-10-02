namespace Api.Controllers
{
    [Route("/api/[controller]")]
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
                var clientUrl = _clientRepository.GetClientUrlById(projectId);
                var apiUrl = _clientRepository.GetApiUrlById(projectId);
                HttpContext.Session.SetString("projectId", projectId);
                HttpContext.Session.SetString("projectUrl", clientUrl);
                HttpContext.Session.SetString("apiUrl", apiUrl);

                return View(new LoginViewModel());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login(get) error: {ex}");
                return View(new LoginViewModel());
            }
        }

        [HttpPost("Login/{projectId}")]
        public async Task<IActionResult> Login(
            LoginViewModel viewModel
            )
        {
            try
            {
                if (_sessionService.IsExpired(HttpContext) is true)
                    Console.WriteLine("session expired -Login");
                if (ModelState.IsValid is false)
                    return View(viewModel);

                var result = await _signInManager.PasswordSignInAsync(
                    viewModel.Email,
                    viewModel.Password,
                    false,
                    false
                    );
                if (result.Succeeded is false)
                {
                    viewModel.ErrorMessage = "invalid email or password!";
                    return View(viewModel);
                }

                // craft token
                var user = await _userManager.FindByEmailAsync(viewModel.Email);
                var audience = _sessionService.GetApiUrl(HttpContext);
                var issuer = _clientRepository.GetApiUrlById(
                    _sessionService.GetProjectId(HttpContext)
                    );
                var token = _jwtTokenProvider.CreateToken(user, audience, issuer);
                string url = _sessionService.GetApiUrl(HttpContext);
                url += $"/login/{token}";

                // go back to client!
                return Redirect(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex}");
                viewModel.ErrorMessage = "error!";
                return View();
            }
        }

        [HttpGet("SignUp")]
        public IActionResult SignUp()
            => View(new SignUpViewModel());

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp(SignUpViewModel viewModel)
        {
            try
            {
                if (_sessionService.IsExpired(HttpContext) is true)
                    Console.WriteLine("session expired -SignUp");
                if (ModelState.IsValid is false)
                    return View(viewModel);

                var user = new User
                {
                    UserName = viewModel.Email,
                    FirstName = viewModel.FirstName,
                    LastName = viewModel.LastName,
                    Email = viewModel.Email,
                    ProjectId = HttpContext.Session.GetString("projectId")
                };
                IdentityResult? result = await _userManager.CreateAsync(user, viewModel.Password);
                if (result.Succeeded is false)
                {
                    Console.WriteLine($"account for {viewModel.Email} couldnot be created!");
                    foreach (var e in result.Errors)
                    {
                        viewModel.ErrorMessage += e.Description;
                        Console.WriteLine(e.Description);
                    }

                    return View(viewModel);
                }

                var dbUser = await _userManager.FindByEmailAsync(viewModel.Email);
                await _authService.GetConfirmationEmail(dbUser);

                return RedirectToAction("EmailConfirmation");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignUp error: {ex}");
                return View(viewModel);
            }
        }

        [HttpGet("EmailConfirmation")]
        public IActionResult EmailConfirmation()
            => View();

        [HttpGet]
        public IActionResult RedirectToLogin()
            => RedirectToAction(
                "Login",
                new
                {
                    projectId = _sessionService.GetProjectId(HttpContext)
                }
        );

        // processess user token 
        [HttpGet("ConfirmEmail/{userId}/{code}")]
        public async Task<IActionResult> ConfirmEmail(
            [FromRoute] string userId,
            [FromRoute] string code
            )
        {
            try
            {
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
                string url = _sessionService.GetApiUrl(HttpContext);
                url += "/api/user/create";
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
