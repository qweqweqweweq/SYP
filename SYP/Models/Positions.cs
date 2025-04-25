using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYP.Models
{
    public class Positions
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public double Salary {  get; set; }
    }
}
