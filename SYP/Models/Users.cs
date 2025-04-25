using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYP.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }
        public string Username {  get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public int EmployeeId { get; set; }
    }
}
