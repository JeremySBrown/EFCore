using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ChoreApp.DataStore;

namespace ChoreApp.DataStore.Migrations
{
    [DbContext(typeof(ChoreAppDbContext))]
    [Migration("20170215001130_AddChores")]
    partial class AddChores
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ChoreApp.Models.Chore", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ChildId");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 500);

                    b.Property<bool>("OnFriday");

                    b.Property<bool>("OnMonday");

                    b.Property<bool>("OnSaturday");

                    b.Property<bool>("OnSunday");

                    b.Property<bool>("OnThursday");

                    b.Property<bool>("OnTuesday");

                    b.Property<bool>("OnWednesday");

                    b.HasKey("Id");

                    b.HasIndex("ChildId");

                    b.ToTable("Chores");
                });

            modelBuilder.Entity("ChoreApp.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 50);

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ChoreApp.Models.Chore", b =>
                {
                    b.HasOne("ChoreApp.Models.User", "Child")
                        .WithMany("Chores")
                        .HasForeignKey("ChildId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
