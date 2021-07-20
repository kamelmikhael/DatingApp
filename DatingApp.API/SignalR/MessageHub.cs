using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.SignalR;
using DatingApp.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using DatingApp.API.DTOs;
using DatingApp.API.Entities;
using System.Linq;
using System;

namespace DatingApp.API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _tracker;

        public MessageHub(IUnitOfWork unitOfWork,
            IMapper mapper,
            IHubContext<PresenceHub> presenceHub,
            PresenceTracker tracker)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _presenceHub = presenceHub;
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var currentUser = Context.User.GetUserName();
            var otherUser = httpContext.Request.Query["user"].ToString();

            var groupName = GetGroupName(currentUser, otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToMessageGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _unitOfWork.MessageRepository.GetMessageThread(currentUser, otherUser);

            if(_unitOfWork.HasChanges()) await _unitOfWork.Complete();

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var senderUserName = Context.User.GetUserName();

            if(senderUserName == createMessageDto.ReceipientUserName.ToLower())
                throw new HubException("You can not send messages for yourself");

            var sender = await _unitOfWork.UserRepository.GetUserByUserNameAsync(senderUserName);
            var receipient = await _unitOfWork.UserRepository.GetUserByUserNameAsync(createMessageDto.ReceipientUserName);

            if(receipient == null) throw new HubException("Not found user.");

            var message = new Message()
            {
                Sender = sender,
                Recipient = receipient,
                SenderUserName = sender.UserName,
                RecipientUserName = receipient.UserName,
                Content = createMessageDto.Content,
            };

            var groupName = GetGroupName(sender.UserName, receipient.UserName);
            
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            // if 2-user in the same group
            if(group.Connections.Any(x => x.UserName == receipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else 
            {
                var connectionIds = await _tracker.GetConnectionsForUser(receipient.UserName);
                if(connectionIds != null && connectionIds.Count > 0)
                {
                    await _presenceHub.Clients.Clients(connectionIds).SendAsync("NewMessageReceived", 
                        new {UserName = sender.UserName, KnownAs = sender.KnownAs});
                }
            }

            _unitOfWork.MessageRepository.AddMessage(message);

            if(await _unitOfWork.Complete())
            {
                var messageDto = _mapper.Map<MessageDto>(message);
                await Clients.Group(groupName).SendAsync("NewMessage", messageDto);
            }
        }

        private async Task<Group> AddToMessageGroup(string groupName)
        {
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());

            if(group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if(await _unitOfWork.Complete()) return group;

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            if(group != null)
            {
                var connection = group.Connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
                _unitOfWork.MessageRepository.RemoveConnection(connection);
                await _unitOfWork.Complete();
                return group;
            }

            throw new HubException("Failed to remove from group");
        }

        private string GetGroupName(string caller, string other)
        {
            bool stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}