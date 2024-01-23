using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandleData_StudentScore.Models; // Add this if necessary

namespace HandleData_StudentScore.Models
{
    public partial class Student
    {
        public int Id { get; set; }
        public string StudentCode { get; set; }
        public int SchoolYearId { get; set; }
    }
}