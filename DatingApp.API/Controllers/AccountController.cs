using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto input)
        {
            try
            {
                if (await IsUserExist(input.UserName))
                {
                    return BadRequest("Username is taken.");
                }

                using var hmac = new HMACSHA512();

                var user = new AppUser()
                {
                    UserName = input.UserName.ToLower(),
                    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input.Password)),
                    PasswordSalt = hmac.Key
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var output = new UserDto()
                {
                    UserName = user.UserName,
                    Token = _tokenService.CreateToken(user)
                };

                return Ok(output);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto input)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == input.UserName.ToLower());

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
                    Token = _tokenService.CreateToken(user)
                };

                return Ok(output);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<bool> IsUserExist(string userName)
        {
            return await _context.Users.AnyAsync(x => x.UserName == userName.ToLower());
        }

    }
}