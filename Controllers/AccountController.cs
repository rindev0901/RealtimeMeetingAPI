using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtimeMeetingAPI.Dtos;
using RealtimeMeetingAPI.Entities;
using RealtimeMeetingAPI.Helpers;
using RealtimeMeetingAPI.Interfaces;
using RealtimeMeetingAPI.Responses;

namespace RealtimeMeetingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            RoleManager<AppRole> roleManager, ITokenService tokenService, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _tokenService = tokenService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register(RegisterDto register)
        {
            if (await UserExists(register.UserName))
                return StatusCode(StatusCodes.Status409Conflict,
                        Response<object>.Result(null, "Username is already existed", 
                        StatusCodes.Status409Conflict)
                    );

            var user = _mapper.Map<AppUser>(register);

            user.UserName = register.UserName.ToLower();

            var result = await _userManager.CreateAsync(user, register.Password);

            if (!result.Succeeded) return StatusCode(StatusCodes.Status400BadRequest,
                        Response<IEnumerable<IdentityError>>.Result(result.Errors, 
                        "Register account unsuccessfully", StatusCodes.Status400BadRequest)
                    );

            bool guestRoleExists = await _roleManager.RoleExistsAsync("Guest");
            var newRole = new AppRole { Name = "Guest", Description = "Role for guest"};

            if (!guestRoleExists)
            {
                var newRoleResult = await _roleManager.CreateAsync(newRole);
                if(!newRoleResult.Succeeded)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        Response<IEnumerable<IdentityError>>.Result(newRoleResult.Errors, 
                        "Create role unsuccessfully", StatusCodes.Status400BadRequest)
                    );
                }
            }

            var roleResult = await _userManager.AddToRoleAsync(user, newRole.Name);

            if (!roleResult.Succeeded) return StatusCode(StatusCodes.Status400BadRequest,
                Response<IEnumerable<IdentityError>>.Result(roleResult.Errors,
                "Assign role for account unsuccessfully",
                StatusCodes.Status400BadRequest));

            var registerResponse = new RegisterResponse
            {
                UserName = user.UserName,
                FullName = user.FullName,
            };

            return StatusCode(StatusCodes.Status200OK,
                    Response<RegisterResponse>.Result(registerResponse, "Register account successfully",
                    StatusCodes.Status200OK)
                );
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users
                .SingleOrDefaultAsync(x => x.UserName == loginDto.UserName.ToLower());

            if (user == null)
                return Unauthorized("Invalid Username");

            if (user.Locked)//true = locked
                return BadRequest("This account is loked by admin");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized("Invalid password");

            var loginResponse = new LoginResponse
            {
                UserName = user.UserName,
                FullName = user.FullName,
                LastActive = user.LastActive.HasValue ? user.LastActive : null,
                Token = await _tokenService.CreateTokenAsync(user),
                PhotoUrl = user.PhotoUrl
            };

            user.LastActive = DateTime.UtcNow;

            await _unitOfWork.Complete();

            
            return StatusCode(StatusCodes.Status200OK,
                        Response<LoginResponse>.Result(loginResponse, "Login account successfully", StatusCodes.Status200OK)
                    );
        }

        [HttpPost("login-social")]
        public async Task<ActionResult<LoginResponse>> LoginSocial(LoginSocialDto loginDto)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.UserName == loginDto.Email);
            // email = username
            if (user != null)//có rồi thì đăng nhập bình thường
            {
                if (user.Locked)//true = locked
                    return BadRequest("This account is loked by admin");

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Email, false);

                if (!result.Succeeded) return Unauthorized("Invalid password");
                
                var loginResponse = new LoginResponse
                {
                    UserName = user.UserName,
                    FullName = user.FullName,
                    LastActive = user.LastActive,
                    Token = await _tokenService.CreateTokenAsync(user),
                    PhotoUrl = user.PhotoUrl
                };
                return StatusCode(StatusCodes.Status200OK,
                        Response<LoginResponse>.Result(loginResponse, "Login social account successfully", StatusCodes.Status200OK)
                    );
            }
            else//Chưa có thì tạo mới user
            {
                var appUser = new AppUser
                {
                    UserName = loginDto.Email,
                    Email = loginDto.Email,
                    FullName = loginDto.Name,
                    PhotoUrl = loginDto.PhotoUrl
                };

                var result = await _userManager.CreateAsync(appUser, loginDto.Email);//password là email

                if (!result.Succeeded) return BadRequest(result.Errors);

                var roleResult = await _userManager.AddToRoleAsync(appUser, "Guest");

                if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

                var loginResponse = new LoginResponse
                {
                    UserName = appUser.UserName,
                    FullName = appUser.FullName,
                    LastActive = appUser.LastActive,
                    Token = await _tokenService.CreateTokenAsync(appUser),
                    PhotoUrl = loginDto.PhotoUrl
                };

                return StatusCode(StatusCodes.Status200OK,
                        Response<LoginResponse>.Result(loginResponse, "Login social account successfully", StatusCodes.Status200OK)
                    );
            }
        }

        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
