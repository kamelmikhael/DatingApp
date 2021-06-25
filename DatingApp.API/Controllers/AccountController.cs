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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(DataContext context,
            ITokenService tokenService,
            IMapper mapper)
        {
            _context = context;
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

            using var hmac = new HMACSHA512();

            user.UserName = input.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input.Password));
            user.PasswordSalt = hmac.Key;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var output = new UserDto()
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos?.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs
            };

            return Ok(output);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto input)
        {
            var user = await _context.Users
                .Include(x => x.Photos)
                .SingleOrDefaultAsync(x => x.UserName == input.UserName.ToLower());

            if (user == null) return Unauthorized("Invalid username/password");

            using var hmac = new HMACSHA256(user.PasswordSalt);

            var passwordHas = hmac.ComputeHash(Encoding.UTF8.GetBytes(input.Password));

            // for (int i = 0; i < passwordHas.Length; i++)
            // {
            //     if (passwordHas[i] != user.PasswordHash[i]) return Unauthorized("Invalid username/password");
            // }

            var output = new UserDto()
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs
            };

            return Ok(output);
        }

        private async Task<bool> IsUserExist(string userName)
        {
            return await _context.Users.AnyAsync(x => x.UserName == userName.ToLower());
        }

    }
}