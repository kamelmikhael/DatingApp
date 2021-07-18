using System;
using System.Linq;
using DatingApp.API.Entities;

namespace DatingApp.API.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<Message> MarkUnreadAsRead(this IQueryable<Message> query, string currentUsername)
        {
            var unreadMessages = query.Where(m => m.DateRead == null
                && m.RecipientUserName == currentUsername);

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }

            return query;
        }
    }
}