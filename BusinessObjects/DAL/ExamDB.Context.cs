﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BusinessObjects.DAL
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class productdbEntities : DbContext
    {
        public productdbEntities()
            : base("name=productdbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<SEAB_EXAM> SEAB_EXAM { get; set; }
        public virtual DbSet<SEAB_SCHOOL> SEAB_SCHOOL { get; set; }
        public virtual DbSet<SEAB_SUBJECT> SEAB_SUBJECT { get; set; }
    }
}
