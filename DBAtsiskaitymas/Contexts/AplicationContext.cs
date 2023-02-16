using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBAtsiskaitymas.Models;
using Microsoft.EntityFrameworkCore;

namespace DBAtsiskaitymas.Contexts
{
    internal class AplicationContext : DbContext
    {
        public DbSet<Department> Departments { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Lecture> Lectures { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer($"Server=VAIDAS;Database=DBAtsiskaitymas;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}
