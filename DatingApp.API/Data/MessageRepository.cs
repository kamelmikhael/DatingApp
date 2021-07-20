using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.API.DTOs;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using DatingApp.API.Extensions;

namespace DatingApp.API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                .Include(c => c.Connections)
                .FirstOrDefaultAsync(x => x.Connections.Any(c => c.ConnectionId == connectionId));
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                .Include(x => x.Sender)
                .Include(x => x.Recipient)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                .Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(x => x.DateSend)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(x => x.Recipient.UserName == messageParams.UserName && !x.RecipientDeleted),
                "Outbox" => query.Where(x => x.Sender.UserName == messageParams.UserName && !x.SenderDeleted),
                _ => query.Where(x => x.Recipient.UserName == messageParams.UserName && !x.RecipientDeleted && x.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        // public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        // {
        //     var messages = await _context.Messages
        //         .Include(x => x.Sender.Photos)
        //         .Include(x => x.Recipient.Photos)
        //         .Where(x => 
        //             x.Sender.UserName == currentUserName && x.Recipient.UserName == recipientUserName
        //             ||
        //             x.Sender.UserName == recipientUserName && x.Recipient.UserName == currentUserName
        //         )
        //         .OrderBy(x => x.DateSend)
        //         .ToListAsync();

        //     var unreadMessages = messages
        //         .Where(x => x.DateRead == null && x.Recipient.UserName == currentUserName)
        //         .Select(x => {
        //             x.DateRead = DateTime.Now;
        //             return x;
        //         }).ToList();

        //     if(unreadMessages.Any()) await _context.SaveChangesAsync();

        //     return _mapper.Map<IEnumerable<MessageDto>>(messages);
        // }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername,
            string recipientUsername)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender).ThenInclude(s => s.Photos)
                .Include(m => m.Recipient).ThenInclude(r => r.Photos)
                .Where(m => m.Recipient.UserName == currentUsername && m.RecipientDeleted == false
                        && m.Sender.UserName == recipientUsername
                        || m.Recipient.UserName == recipientUsername
                        && m.Sender.UserName == currentUsername && m.SenderDeleted == false
                )
                //.MarkUnreadAsRead(currentUsername)
                .OrderBy(m => m.DateSend)
                //.ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUserName == currentUsername).ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }

            //return messages;
            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}