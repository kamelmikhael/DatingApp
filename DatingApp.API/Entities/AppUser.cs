using System;
using System.Collections;
using System.Collections.Generic;
using DatingApp.API.Extensions;

namespace DatingApp.API.Entities
{
    public class AppUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastActiveAt { get; set; } = DateTime.Now;
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public ICollection<Photo> Photos { get; set; }
        
        public ICollection<UserLike> LikedByUsers { get; set; }
        
        public ICollection<UserLike> LikedUsers { get; set; }

        public ICollection<Message> MessagesSend { get; set; }
        
        public ICollection<Message> MessagesReceived { get; set; }
    }
}