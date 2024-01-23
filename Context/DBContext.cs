using HandleData_StudentScore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandleData_StudentScore.Context
{
    public class DBContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=ANN;" +
                "Initial Catalog=DataScore;" +
                "Persist Security Info=True;" +
                "User ID=sa;" +
                "Password=12345;" +
                "Encrypt=False;" +
                "TrustServerCertificate=True");
        }

        public DbSet<SchoolYear> SchoolYear { get; set; }

        public DbSet<Student> Student { get; set; }

        public DbSet<Subject> Subject { get; set; }

        public DbSet<Score> Score { get; set; }
    }
}