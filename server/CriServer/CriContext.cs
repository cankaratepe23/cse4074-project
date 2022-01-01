﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CriServer
{
    class CriContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .HasMany(p => p.Groups)
                .WithMany(p => p.Users);
        }

        public CriContext(DbContextOptions<CriContext> options) : base(options) { }
    }

    class User
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public IPAddress IpAddress { get; set; }
        public ICollection<Group> Groups { get; set; }
    }

    class Group
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public ICollection<User> Users { get; set; }
    }
}