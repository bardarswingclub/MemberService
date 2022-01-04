﻿// <auto-generated />
using System;
using MemberService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MemberService.Data.Migrations
{
    [DbContext(typeof(MemberContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("MemberService.Data.AnnualMeeting", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("MeetingEndsAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("MeetingInfo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MeetingInvitation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("MeetingStartsAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("MeetingSummary")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("SurveyId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SurveyId");

                    b.ToTable("AnnualMeetings");
                });

            modelBuilder.Entity("MemberService.Data.AnnualMeetingAttendee", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AnnualMeetingId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastVisited")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Visits")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AnnualMeetingId");

                    b.HasIndex("UserId");

                    b.ToTable("AnnualMeetingAttendee");
                });

            modelBuilder.Entity("MemberService.Data.Event", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Archived")
                        .HasColumnType("bit");

                    b.Property<bool>("Cancelled")
                        .HasColumnType("bit");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("LessonCount")
                        .HasColumnType("int");

                    b.Property<bool>("Published")
                        .HasColumnType("bit");

                    b.Property<Guid?>("SemesterId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("SurveyId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedBy");

                    b.HasIndex("SemesterId");

                    b.HasIndex("SurveyId");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("MemberService.Data.EventOrganizer", b =>
                {
                    b.Property<Guid>("EventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("CanAddPresenceLesson")
                        .HasColumnType("bit");

                    b.Property<bool>("CanEdit")
                        .HasColumnType("bit");

                    b.Property<bool>("CanEditOrganizers")
                        .HasColumnType("bit");

                    b.Property<bool>("CanEditSignup")
                        .HasColumnType("bit");

                    b.Property<bool>("CanSetPresence")
                        .HasColumnType("bit");

                    b.Property<bool>("CanSetSignupStatus")
                        .HasColumnType("bit");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("EventId", "UserId");

                    b.HasIndex("UpdatedBy");

                    b.HasIndex("UserId");

                    b.ToTable("EventOrganizers");
                });

            modelBuilder.Entity("MemberService.Data.EventSignup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PartnerEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PaymentId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<Guid?>("ResponseId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("SignedUpAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.HasIndex("PaymentId")
                        .IsUnique()
                        .HasFilter("[PaymentId] IS NOT NULL");

                    b.HasIndex("ResponseId");

                    b.HasIndex("UserId");

                    b.ToTable("EventSignups");
                });

            modelBuilder.Entity("MemberService.Data.EventSignupAuditEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("EventSignupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("OccuredAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("EventSignupId");

                    b.HasIndex("UserId");

                    b.ToTable("EventSignupAuditEntry");
                });

            modelBuilder.Entity("MemberService.Data.EventSignupOptions", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("AllowPartnerSignup")
                        .HasColumnType("bit");

                    b.Property<string>("AllowPartnerSignupHelp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("AutoAcceptedSignups")
                        .HasColumnType("int");

                    b.Property<bool>("IncludedInClassesFee")
                        .HasColumnType("bit");

                    b.Property<bool>("IncludedInTrainingFee")
                        .HasColumnType("bit");

                    b.Property<decimal>("PriceForMembers")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PriceForNonMembers")
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool>("RequiresClassesFee")
                        .HasColumnType("bit");

                    b.Property<bool>("RequiresMembershipFee")
                        .HasColumnType("bit");

                    b.Property<bool>("RequiresTrainingFee")
                        .HasColumnType("bit");

                    b.Property<bool>("RoleSignup")
                        .HasColumnType("bit");

                    b.Property<string>("RoleSignupHelp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("SignupClosesAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("SignupHelp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("SignupOpensAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("SignupClosesAt");

                    b.HasIndex("SignupOpensAt");

                    b.ToTable("EventSignupOptions");
                });

            modelBuilder.Entity("MemberService.Data.MemberRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("MemberService.Data.Payment", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IncludesClasses")
                        .HasColumnType("bit");

                    b.Property<bool>("IncludesMembership")
                        .HasColumnType("bit");

                    b.Property<bool>("IncludesTraining")
                        .HasColumnType("bit");

                    b.Property<string>("ManualPayment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("PayedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Refunded")
                        .HasColumnType("bit");

                    b.Property<string>("StripeChargeId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("PayedAtUtc");

                    b.HasIndex("UserId");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("MemberService.Data.Presence", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Lesson")
                        .HasColumnType("int");

                    b.Property<bool>("Present")
                        .HasColumnType("bit");

                    b.Property<DateTime>("RegisteredAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("RegisteredById")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("SignupId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("RegisteredById");

                    b.HasIndex("SignupId");

                    b.ToTable("Presence");
                });

            modelBuilder.Entity("MemberService.Data.Question", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("AnswerableFrom")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("AnswerableUntil")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<Guid>("SurveyId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SurveyId");

                    b.ToTable("Questions");
                });

            modelBuilder.Entity("MemberService.Data.QuestionAnswer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("AnsweredAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("OptionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ResponseId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("OptionId");

                    b.HasIndex("ResponseId");

                    b.ToTable("QuestionAnswers");
                });

            modelBuilder.Entity("MemberService.Data.QuestionOption", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<Guid>("QuestionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.ToTable("QuestionOptions");
                });

            modelBuilder.Entity("MemberService.Data.Response", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("SurveyId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("SurveyId");

                    b.HasIndex("UserId");

                    b.ToTable("Response");
                });

            modelBuilder.Entity("MemberService.Data.Semester", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SignupHelpText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("SignupOpensAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("SurveyId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SurveyId");

                    b.ToTable("Semesters");
                });

            modelBuilder.Entity("MemberService.Data.Survey", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Surveys");
                });

            modelBuilder.Entity("MemberService.Data.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("ExemptFromClassesFee")
                        .HasColumnType("bit");

                    b.Property<bool>("ExemptFromTrainingFee")
                        .HasColumnType("bit");

                    b.Property<string>("FullName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("MemberService.Data.UserRole", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("MemberService.Data.AnnualMeeting", b =>
                {
                    b.HasOne("MemberService.Data.Survey", "Survey")
                        .WithMany()
                        .HasForeignKey("SurveyId");

                    b.Navigation("Survey");
                });

            modelBuilder.Entity("MemberService.Data.AnnualMeetingAttendee", b =>
                {
                    b.HasOne("MemberService.Data.AnnualMeeting", "AnnualMeeting")
                        .WithMany("Attendees")
                        .HasForeignKey("AnnualMeetingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemberService.Data.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("AnnualMeeting");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MemberService.Data.Event", b =>
                {
                    b.HasOne("MemberService.Data.User", "CreatedByUser")
                        .WithMany()
                        .HasForeignKey("CreatedBy");

                    b.HasOne("MemberService.Data.Semester", "Semester")
                        .WithMany("Courses")
                        .HasForeignKey("SemesterId");

                    b.HasOne("MemberService.Data.Survey", "Survey")
                        .WithMany()
                        .HasForeignKey("SurveyId");

                    b.Navigation("CreatedByUser");

                    b.Navigation("Semester");

                    b.Navigation("Survey");
                });

            modelBuilder.Entity("MemberService.Data.EventOrganizer", b =>
                {
                    b.HasOne("MemberService.Data.Event", "Event")
                        .WithMany("Organizers")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemberService.Data.User", "UpdatedByUser")
                        .WithMany()
                        .HasForeignKey("UpdatedBy");

                    b.HasOne("MemberService.Data.User", "User")
                        .WithMany("Organizes")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("UpdatedByUser");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MemberService.Data.EventSignup", b =>
                {
                    b.HasOne("MemberService.Data.Event", "Event")
                        .WithMany("Signups")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemberService.Data.Payment", "Payment")
                        .WithOne("EventSignup")
                        .HasForeignKey("MemberService.Data.EventSignup", "PaymentId");

                    b.HasOne("MemberService.Data.Response", "Response")
                        .WithMany()
                        .HasForeignKey("ResponseId");

                    b.HasOne("MemberService.Data.User", "User")
                        .WithMany("EventSignups")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("Payment");

                    b.Navigation("Response");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MemberService.Data.EventSignupAuditEntry", b =>
                {
                    b.HasOne("MemberService.Data.EventSignup", "EventSignup")
                        .WithMany("AuditLog")
                        .HasForeignKey("EventSignupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemberService.Data.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("EventSignup");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MemberService.Data.EventSignupOptions", b =>
                {
                    b.HasOne("MemberService.Data.Event", "Event")
                        .WithOne("SignupOptions")
                        .HasForeignKey("MemberService.Data.EventSignupOptions", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");
                });

            modelBuilder.Entity("MemberService.Data.Payment", b =>
                {
                    b.HasOne("MemberService.Data.User", "User")
                        .WithMany("Payments")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("MemberService.Data.Presence", b =>
                {
                    b.HasOne("MemberService.Data.User", "RegisteredBy")
                        .WithMany()
                        .HasForeignKey("RegisteredById");

                    b.HasOne("MemberService.Data.EventSignup", "Signup")
                        .WithMany("Presence")
                        .HasForeignKey("SignupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RegisteredBy");

                    b.Navigation("Signup");
                });

            modelBuilder.Entity("MemberService.Data.Question", b =>
                {
                    b.HasOne("MemberService.Data.Survey", "Survey")
                        .WithMany("Questions")
                        .HasForeignKey("SurveyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Survey");
                });

            modelBuilder.Entity("MemberService.Data.QuestionAnswer", b =>
                {
                    b.HasOne("MemberService.Data.QuestionOption", "Option")
                        .WithMany("Answers")
                        .HasForeignKey("OptionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MemberService.Data.Response", "Response")
                        .WithMany("Answers")
                        .HasForeignKey("ResponseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Option");

                    b.Navigation("Response");
                });

            modelBuilder.Entity("MemberService.Data.QuestionOption", b =>
                {
                    b.HasOne("MemberService.Data.Question", "Question")
                        .WithMany("Options")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Question");
                });

            modelBuilder.Entity("MemberService.Data.Response", b =>
                {
                    b.HasOne("MemberService.Data.Survey", "Survey")
                        .WithMany("Responses")
                        .HasForeignKey("SurveyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemberService.Data.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Survey");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MemberService.Data.Semester", b =>
                {
                    b.HasOne("MemberService.Data.Survey", "Survey")
                        .WithMany()
                        .HasForeignKey("SurveyId");

                    b.Navigation("Survey");
                });

            modelBuilder.Entity("MemberService.Data.UserRole", b =>
                {
                    b.HasOne("MemberService.Data.MemberRole", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemberService.Data.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("MemberService.Data.MemberRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("MemberService.Data.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("MemberService.Data.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("MemberService.Data.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MemberService.Data.AnnualMeeting", b =>
                {
                    b.Navigation("Attendees");
                });

            modelBuilder.Entity("MemberService.Data.Event", b =>
                {
                    b.Navigation("Organizers");

                    b.Navigation("SignupOptions");

                    b.Navigation("Signups");
                });

            modelBuilder.Entity("MemberService.Data.EventSignup", b =>
                {
                    b.Navigation("AuditLog");

                    b.Navigation("Presence");
                });

            modelBuilder.Entity("MemberService.Data.MemberRole", b =>
                {
                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("MemberService.Data.Payment", b =>
                {
                    b.Navigation("EventSignup");
                });

            modelBuilder.Entity("MemberService.Data.Question", b =>
                {
                    b.Navigation("Options");
                });

            modelBuilder.Entity("MemberService.Data.QuestionOption", b =>
                {
                    b.Navigation("Answers");
                });

            modelBuilder.Entity("MemberService.Data.Response", b =>
                {
                    b.Navigation("Answers");
                });

            modelBuilder.Entity("MemberService.Data.Semester", b =>
                {
                    b.Navigation("Courses");
                });

            modelBuilder.Entity("MemberService.Data.Survey", b =>
                {
                    b.Navigation("Questions");

                    b.Navigation("Responses");
                });

            modelBuilder.Entity("MemberService.Data.User", b =>
                {
                    b.Navigation("EventSignups");

                    b.Navigation("Organizes");

                    b.Navigation("Payments");

                    b.Navigation("UserRoles");
                });
#pragma warning restore 612, 618
        }
    }
}
