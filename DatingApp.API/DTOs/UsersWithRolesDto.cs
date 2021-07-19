using System.Collections.Generic;

namespace DatingApp.API.DTOs
{
    public class UsersWithRolesDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
    }
}