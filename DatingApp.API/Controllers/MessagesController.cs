using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.DTOs;
using DatingApp.API.Entities;
using DatingApp.API.Extensions;
using DatingApp.API.Helpers;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MessagesController(IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var senderUserName = User.GetUserName();

            if(senderUserName == createMessageDto.ReceipientUserName.ToLower())
                return BadRequest("You can not send messages for yourself");

            var sender = await _unitOfWork.UserRepository.GetUserByUserNameAsync(senderUserName);
            var receipient = await _unitOfWork.UserRepository.GetUserByUserNameAsync(createMessageDto.ReceipientUserName);

            if(receipient == null) return NotFound();

            var message = new Message()
            {
                Sender = sender,
                Recipient = receipient,
                SenderUserName = sender.UserName,
                RecipientUserName = receipient.UserName,
                Content = createMessageDto.Content,
            };

            _unitOfWork.MessageRepository.AddMessage(message);

            if(await _unitOfWork.Complete())
                return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to add message");
        } 

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.GetUserName();

            var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return Ok(messages);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var userName = User.GetUserName();
            var message = await _unitOfWork.MessageRepository.GetMessage(id);

            if(message.Sender.UserName != userName && message.Recipient.UserName != userName) 
            {
                return Unauthorized();
            }

            if(message.Sender.UserName == userName) 
            {
                message.SenderDeleted = true;
            }
            else if(message.Recipient.UserName == userName) 
            {
                message.RecipientDeleted = true;
            }
            
            _unitOfWork.MessageRepository.DeleteMessage(message);

            if(await _unitOfWork.Complete()) return Ok();

            return BadRequest("Falid to delete message.");
        }
    }
}