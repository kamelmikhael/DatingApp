using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.DTOs;
using DatingApp.API.Entities;
using DatingApp.API.Extensions;
using DatingApp.API.Helpers;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    [Authorize]
    public class UserController : BaseApiController
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UserController(IUserRepository repository,
            IMapper mapper,
            IPhotoService photoService)
        {
            _repository = repository;
            _mapper = mapper;
            _photoService = photoService;
        }

        //GET: api/user
        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUser = await _repository.GetMemberAsync(User.GetUserName());
            userParams.CurrentUserName = currentUser.UserName;

            if(string.IsNullOrEmpty(userParams.Gender)) 
            {
                userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
            }

            var users = await _repository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }

        //GET: api/user/smith
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return Ok(await _repository.GetMemberAsync(username));
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            //Get the current logged-in user
            // var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.GetUserName();

            var dbUser = await _repository.GetUserByUserNameAsync(userName);
            
            _mapper.Map(memberUpdateDto, dbUser);

            _repository.Update(dbUser);

            if(await _repository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user.");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var dbUser = await _repository.GetUserByUserNameAsync(User.GetUserName());

            var uploadResult = await _photoService.AddPhotoAsync(file);
            if(uploadResult.Error != null) 
            {
                return BadRequest(uploadResult.Error.Message);
            }

            var photo = new Photo()
            {
                Url = uploadResult.SecureUrl.AbsoluteUri,
                PublicId = uploadResult.PublicId,
            };

            if(dbUser.Photos.Count == 0) 
            {
                photo.IsMain = true;
            }

            dbUser.Photos.Add(photo);

            if(await _repository.SaveAllAsync())
            {
                return CreatedAtRoute("GetUser", new {username=dbUser.UserName}, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo.");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var dbUser = await _repository.GetUserByUserNameAsync(User.GetUserName());

            var photo = dbUser.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo.IsMain)
            {
                return BadRequest("This is already your main photo.");
            }
            
            var currentMainPhoto = dbUser.Photos.FirstOrDefault(x => x.IsMain == true);
            if(currentMainPhoto != null) 
            {
                currentMainPhoto.IsMain = false;
            }

            photo.IsMain = true;

            if(await _repository.SaveAllAsync())
            {
                return NoContent();
            }

            return BadRequest("Set main photo failed.");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var dbUser = await _repository.GetUserByUserNameAsync(User.GetUserName());

            var photoToDelete = dbUser.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photoToDelete == null) 
            {
                return NotFound();
            }

            if(photoToDelete.IsMain)
            {
                return BadRequest("You cannot delete your main photo.");
            }

            if(!string.IsNullOrWhiteSpace(photoToDelete.PublicId))
            {
                //remove from cloudinary
                var deletionResult = await _photoService.DeletePhotoAsync(photoToDelete.PublicId);

                if(deletionResult.Error != null) return BadRequest(deletionResult.Error.Message);
            }

            dbUser.Photos.Remove(photoToDelete);

            if(await _repository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete the photo.");
        }
    }
}