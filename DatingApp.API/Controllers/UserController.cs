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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UserController(IUnitOfWork unitOfWork,
            IMapper mapper,
            IPhotoService photoService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
        }

        //GET: api/user
        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserGender = await _unitOfWork.UserRepository.GetUserGenderAsync(User.GetUserName());
            userParams.CurrentUserName = User.GetUserName();

            if(string.IsNullOrEmpty(userParams.Gender)) 
            {
                userParams.Gender = currentUserGender == "male" ? "female" : "male";
            }

            var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }

        //GET: api/user/smith
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return Ok(await _unitOfWork.UserRepository.GetMemberAsync(username));
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            //Get the current logged-in user
            // var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.GetUserName();

            var dbUser = await _unitOfWork.UserRepository.GetUserByUserNameAsync(userName);
            
            _mapper.Map(memberUpdateDto, dbUser);

            _unitOfWork.UserRepository.Update(dbUser);

            if(await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to update user.");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var dbUser = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());

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

            if(await _unitOfWork.Complete())
            {
                return CreatedAtRoute("GetUser", new {username=dbUser.UserName}, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo.");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var dbUser = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());

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

            if(await _unitOfWork.Complete())
            {
                return NoContent();
            }

            return BadRequest("Set main photo failed.");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var dbUser = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());

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

            if(await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to delete the photo.");
        }
    }
}