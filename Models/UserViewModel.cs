using System;
using System.Collections.Generic;

namespace QL_Spa.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string FullName { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> UserRoles { get; set; }
        public List<string> AllRoles { get; set; }
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
}
