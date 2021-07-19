using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Entities;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto input)
        {
            if (await IsUserExist(input.UserName))
            {
                return BadRequest("Username is taken.");
            }

            var user = _mapper.Map<AppUser>(input);

            user.UserName = input.UserName.ToLower();

            var result = await _userManager.CreateAsync(user, input.Password);
            if(!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");
            if(!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            var output = new UserDto()
            {
                UserName = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos?.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender,
            };

            return Ok(output);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto input)
        {
            var user = await _userManager.Users
                .Include(x => x.Photos)
                .SingleOrDefaultAsync(x => x.UserName == input.UserName.ToLower());

            if (user == null) return Unauthorized("Invalid username/password");

            var result = await _signInManager.CheckPasswordSignInAsync(user, input.Password, false);

            if(!result.Succeeded) return Unauthorized();

            var output = new UserDto()
            {
                UserName = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender,
            };

            return Ok(output);
        }

        private async Task<bool> IsUserExist(string userName)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == userName.ToLower());
        }

    }
}