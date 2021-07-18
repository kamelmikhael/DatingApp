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
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepository, 
            IMessageRepository messageRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var senderUserName = User.GetUserName();

            if(senderUserName == createMessageDto.ReceipientUserName.ToLower())
                return BadRequest("You can not send messages for yourself");

            var sender = await _userRepository.GetUserByUserNameAsync(senderUserName);
            var receipient = await _userRepository.GetUserByUserNameAsync(createMessageDto.ReceipientUserName);

            if(receipient == null) return NotFound();

            var message = new Message()
            {
                Sender = sender,
                Recipient = receipient,
                SenderUserName = sender.UserName,
                RecipientUserName = receipient.UserName,
                Content = createMessageDto.Content,
            };

            _messageRepository.AddMessage(message);

            if(await _messageRepository.SaveAllAsync())
                return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to add message");
        } 

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.GetUserName();

            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return Ok(messages);
        }

        [HttpGet("thread/{userName}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string userName)
        {
            var currentUserName = User.GetUserName();

            return Ok(await _messageRepository.GetMessageThread(currentUserName, userName));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var userName = User.GetUserName();
            var message = await _messageRepository.GetMessage(id);

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
            
            _messageRepository.DeleteMessage(message);

            if(await _messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Falid to delete message.");
        }
    }
}