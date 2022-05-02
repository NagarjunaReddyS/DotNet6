using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.CompilerServices;

namespace MinimalAPIsWithJWT.Models
{
    public class Job
    {
        public Guid JobId { get; set; }
        public int JobStateId { get; set; }
        public string JobMessage { get; set; }
        // public string Jobtype { get; set; }
        // public string JobSubtype { get; set; }
        // public DateTime LastupdatedUTC { get; set; }
        // public int OracleJobProcessId { get; set; }
    }

    public class JobDB : DbContext
    {
        public JobDB(DbContextOptions<JobDB> options): base(options){}
        public DbSet<Job> Job => Set<Job>();
    }
}