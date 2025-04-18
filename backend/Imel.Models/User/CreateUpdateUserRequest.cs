﻿namespace Imel.Models.User
{
    public class CreateUpdateUserRequest
    {
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }

        public int? RoleId { get; set; }

        public bool? IsActive { get; set; }
    }
}
