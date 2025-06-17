using System;
using System.Collections.Generic;

namespace QL_Spa.Models
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<RoleViewModel> Roles { get; set; }
    }
}
