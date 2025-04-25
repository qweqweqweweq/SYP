using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYP.Models
{
    public class Vacations
    {
        [Key]
        public int Id { get; set; }
        public int EmployeeId {  get; set; }
        public DateTime StartDate {  get; set; }
        public DateTime EndDate { get; set; }
        public string Type {  get; set; }
    }
}
