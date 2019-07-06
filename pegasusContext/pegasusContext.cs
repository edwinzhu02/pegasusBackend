using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Pegasus_backend.pegasusContext
{
    public partial class pegasusContext : DbContext
    {
        public pegasusContext()
        {
        }

        public pegasusContext(DbContextOptions<pegasusContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Amendment> Amendment { get; set; }
        public virtual DbSet<ApplicationDetails> ApplicationDetails { get; set; }
        public virtual DbSet<AskOff> AskOff { get; set; }
        public virtual DbSet<AvailableDays> AvailableDays { get; set; }
        public virtual DbSet<ChatGroup> ChatGroup { get; set; }
        public virtual DbSet<ChatMessage> ChatMessage { get; set; }
        public virtual DbSet<Course> Course { get; set; }
        public virtual DbSet<CourseCategory> CourseCategory { get; set; }
        public virtual DbSet<CourseSchedule> CourseSchedule { get; set; }
        public virtual DbSet<Fund> Fund { get; set; }
        public virtual DbSet<GroupCourseInstance> GroupCourseInstance { get; set; }
        public virtual DbSet<Holiday> Holiday { get; set; }
        public virtual DbSet<Invoice> Invoice { get; set; }
        public virtual DbSet<InvoiceWaitingConfirm> InvoiceWaitingConfirm { get; set; }
        public virtual DbSet<Language> Language { get; set; }
        public virtual DbSet<Learner> Learner { get; set; }
        public virtual DbSet<LearnerAppForm> LearnerAppForm { get; set; }
        public virtual DbSet<LearnerGroupCourse> LearnerGroupCourse { get; set; }
        public virtual DbSet<LearnerOthers> LearnerOthers { get; set; }
        public virtual DbSet<LearnerTransaction> LearnerTransaction { get; set; }
        public virtual DbSet<Lesson> Lesson { get; set; }
        public virtual DbSet<LessonRemain> LessonRemain { get; set; }
        public virtual DbSet<LoginLog> LoginLog { get; set; }
        public virtual DbSet<Lookup> Lookup { get; set; }
        public virtual DbSet<One2oneCourseInstance> One2oneCourseInstance { get; set; }
        public virtual DbSet<OnlineUser> OnlineUser { get; set; }
        public virtual DbSet<Org> Org { get; set; }
        public virtual DbSet<Page> Page { get; set; }
        public virtual DbSet<PageGroup> PageGroup { get; set; }
        public virtual DbSet<Parent> Parent { get; set; }
        public virtual DbSet<ParentAppForm> ParentAppForm { get; set; }
        public virtual DbSet<Payment> Payment { get; set; }
        public virtual DbSet<ProdCat> ProdCat { get; set; }
        public virtual DbSet<ProdType> ProdType { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<Qualification> Qualification { get; set; }
        public virtual DbSet<Rating> Rating { get; set; }
        public virtual DbSet<RemindLog> RemindLog { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<RoleAccess> RoleAccess { get; set; }
        public virtual DbSet<Room> Room { get; set; }
        public virtual DbSet<SoldTransaction> SoldTransaction { get; set; }
        public virtual DbSet<Staff> Staff { get; set; }
        public virtual DbSet<StaffOrg> StaffOrg { get; set; }
        public virtual DbSet<Stock> Stock { get; set; }
        public virtual DbSet<StockApplication> StockApplication { get; set; }
        public virtual DbSet<StockOrder> StockOrder { get; set; }
        public virtual DbSet<Teacher> Teacher { get; set; }
        public virtual DbSet<TeacherCourse> TeacherCourse { get; set; }
        public virtual DbSet<TeacherLanguage> TeacherLanguage { get; set; }
        public virtual DbSet<TeacherQualificatiion> TeacherQualificatiion { get; set; }
        public virtual DbSet<TeacherTransaction> TeacherTransaction { get; set; }
        public virtual DbSet<TeacherWageRates> TeacherWageRates { get; set; }
        public virtual DbSet<Term> Term { get; set; }
        public virtual DbSet<TodoList> TodoList { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySQL("Server=45.76.123.59;User Id=dbuser;Password=qwer1234;Database=ablemusic");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Amendment>(entity =>
            {
                entity.ToTable("amendment", "ablemusic");

                entity.HasIndex(e => e.CourseInstanceId)
                    .HasName("R_26");

                entity.HasIndex(e => e.CourseScheduleId)
                    .HasName("R_105");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_27");

                entity.HasIndex(e => e.OrgId)
                    .HasName("R_30");

                entity.HasIndex(e => e.RoomId)
                    .HasName("R_28");

                entity.HasIndex(e => e.TeacherId)
                    .HasName("R_111");

                entity.Property(e => e.AmendmentId)
                    .HasColumnName("amendment_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AmendType)
                    .HasColumnName("amend_type")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.BeginDate)
                    .HasColumnName("begin_date")
                    .HasColumnType("date");

                entity.Property(e => e.BeginTime).HasColumnName("begin_time");

                entity.Property(e => e.CourseInstanceId)
                    .HasColumnName("course_instance_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CourseScheduleId)
                    .HasColumnName("course_schedule_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.DayOfWeek)
                    .HasColumnName("day_of_week")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.EndDate)
                    .HasColumnName("end_date")
                    .HasColumnType("date");

                entity.Property(e => e.EndTime).HasColumnName("end_time");

                entity.Property(e => e.IsTemporary)
                    .HasColumnName("is_temporary")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.OrgId)
                    .HasColumnName("org_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Reason)
                    .HasColumnName("reason")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.RoomId)
                    .HasColumnName("room_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.TeacherId)
                    .HasColumnName("teacher_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.CourseInstance)
                    .WithMany(p => p.Amendment)
                    .HasForeignKey(d => d.CourseInstanceId)
                    .HasConstraintName("R_26");

                entity.HasOne(d => d.CourseSchedule)
                    .WithMany(p => p.Amendment)
                    .HasForeignKey(d => d.CourseScheduleId)
                    .HasConstraintName("R_105");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.Amendment)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_27");

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.Amendment)
                    .HasForeignKey(d => d.OrgId)
                    .HasConstraintName("R_30");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Amendment)
                    .HasForeignKey(d => d.RoomId)
                    .HasConstraintName("R_28");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.Amendment)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("R_111");
            });

            modelBuilder.Entity<ApplicationDetails>(entity =>
            {
                entity.HasKey(e => e.DetaillsId);

                entity.ToTable("application_details", "ablemusic");

                entity.HasIndex(e => e.ApplicationId)
                    .HasName("R_133");

                entity.HasIndex(e => e.ProductId)
                    .HasName("R_134");

                entity.Property(e => e.DetaillsId)
                    .HasColumnName("detaills_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ApplicationId)
                    .HasColumnName("application_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AppliedQty)
                    .HasColumnName("applied_qty")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DeliveredQty)
                    .HasColumnName("delivered_qty")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ProductId)
                    .HasColumnName("product_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ReceivedQty)
                    .HasColumnName("received_qty")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Application)
                    .WithMany(p => p.ApplicationDetails)
                    .HasForeignKey(d => d.ApplicationId)
                    .HasConstraintName("R_133");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ApplicationDetails)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("R_134");
            });

            modelBuilder.Entity<AskOff>(entity =>
            {
                entity.HasKey(e => e.AskId);

                entity.ToTable("ask_off", "ablemusic");

                entity.HasIndex(e => e.UserId)
                    .HasName("R_65");

                entity.Property(e => e.AskId)
                    .HasColumnName("ask_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ApplyDate).HasColumnName("apply_date");

                entity.Property(e => e.BeginDate)
                    .HasColumnName("begin_date")
                    .HasColumnType("date");

                entity.Property(e => e.EndDate)
                    .HasColumnName("end_date")
                    .HasColumnType("date");

                entity.Property(e => e.Note)
                    .HasColumnName("note")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ProcessStatus)
                    .HasColumnName("process_status")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.ReasonDesc)
                    .HasColumnName("reason_desc")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ReasonType)
                    .HasColumnName("reason_type")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.UserType)
                    .HasColumnName("user_type")
                    .HasColumnType("bit(1)");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AskOff)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("R_65");
            });

            modelBuilder.Entity<AvailableDays>(entity =>
            {
                entity.ToTable("available_days", "ablemusic");

                entity.HasIndex(e => e.OrgId)
                    .HasName("R_81");

                entity.HasIndex(e => e.TeacherId)
                    .HasName("R_9");

                entity.Property(e => e.AvailableDaysId)
                    .HasColumnName("available_days_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.DayOfWeek)
                    .HasColumnName("day_of_week")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.OrgId)
                    .HasColumnName("org_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.TeacherId)
                    .HasColumnName("teacher_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.AvailableDays)
                    .HasForeignKey(d => d.OrgId)
                    .HasConstraintName("R_81");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.AvailableDays)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("R_9");
            });

            modelBuilder.Entity<ChatGroup>(entity =>
            {
                entity.ToTable("chat_group", "ablemusic");

                entity.HasIndex(e => e.UserId)
                    .HasName("R_121");

                entity.Property(e => e.ChatGroupId)
                    .HasColumnName("chat_group_id")
                    .HasMaxLength(60)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ChatGroup)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("R_121");
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.ToTable("chat_message", "ablemusic");

                entity.HasIndex(e => e.ChatGroupId)
                    .HasName("R_122");

                entity.HasIndex(e => e.ReceiverUserId)
                    .HasName("R_128");

                entity.HasIndex(e => e.SenderUserId)
                    .HasName("R_123");

                entity.Property(e => e.ChatMessageId)
                    .HasColumnName("chat_message_id")
                    .HasColumnType("int(11)")
                    .ValueGeneratedNever();

                entity.Property(e => e.ChatGroupId)
                    .HasColumnName("chat_group_id")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.CreateAt)
                    .HasColumnName("create_at")
                    .HasColumnType("date");

                entity.Property(e => e.MessageBody)
                    .HasColumnName("message_body")
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.ReceiverUserId)
                    .HasColumnName("receiver_user_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.SenderUserId)
                    .HasColumnName("sender_user_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.ChatGroup)
                    .WithMany(p => p.ChatMessage)
                    .HasForeignKey(d => d.ChatGroupId)
                    .HasConstraintName("R_122");

                entity.HasOne(d => d.ReceiverUser)
                    .WithMany(p => p.ChatMessageReceiverUser)
                    .HasForeignKey(d => d.ReceiverUserId)
                    .HasConstraintName("R_128");

                entity.HasOne(d => d.SenderUser)
                    .WithMany(p => p.ChatMessageSenderUser)
                    .HasForeignKey(d => d.SenderUserId)
                    .HasConstraintName("R_123");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("course", "ablemusic");

                entity.HasIndex(e => e.CourseCategoryId)
                    .HasName("R_85");

                entity.Property(e => e.CourseId)
                    .HasColumnName("course_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CourseCategoryId)
                    .HasColumnName("course_category_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.CourseName)
                    .HasColumnName("course_name")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.CourseType)
                    .HasColumnName("course_type")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.Duration)
                    .HasColumnName("duration")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("decimal(6,2)");

                entity.Property(e => e.TeacherLevel)
                    .HasColumnName("teacher_level")
                    .HasColumnType("tinyint(4)");

                entity.HasOne(d => d.CourseCategory)
                    .WithMany(p => p.Course)
                    .HasForeignKey(d => d.CourseCategoryId)
                    .HasConstraintName("R_85");
            });

            modelBuilder.Entity<CourseCategory>(entity =>
            {
                entity.ToTable("course_category", "ablemusic");

                entity.Property(e => e.CourseCategoryId)
                    .HasColumnName("course_category_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.CourseCategoryName)
                    .HasColumnName("course_category_name")
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CourseSchedule>(entity =>
            {
                entity.ToTable("course_schedule", "ablemusic");

                entity.HasIndex(e => e.CourseInstanceId)
                    .HasName("R_90");

                entity.HasIndex(e => e.GroupCourseInstanceId)
                    .HasName("R_91");

                entity.Property(e => e.CourseScheduleId)
                    .HasColumnName("course_schedule_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BeginTime).HasColumnName("begin_time");

                entity.Property(e => e.CourseInstanceId)
                    .HasColumnName("course_instance_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DayOfWeek)
                    .HasColumnName("day_of_week")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.EndTime).HasColumnName("end_time");

                entity.Property(e => e.GroupCourseInstanceId)
                    .HasColumnName("group_course_instance_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.CourseInstance)
                    .WithMany(p => p.CourseSchedule)
                    .HasForeignKey(d => d.CourseInstanceId)
                    .HasConstraintName("R_90");

                entity.HasOne(d => d.GroupCourseInstance)
                    .WithMany(p => p.CourseSchedule)
                    .HasForeignKey(d => d.GroupCourseInstanceId)
                    .HasConstraintName("R_91");
            });

            modelBuilder.Entity<Fund>(entity =>
            {
                entity.HasKey(e => e.LearnerId);

                entity.ToTable("fund", "ablemusic");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)")
                    .ValueGeneratedNever();

                entity.Property(e => e.Balance)
                    .HasColumnName("balance")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<GroupCourseInstance>(entity =>
            {
                entity.ToTable("group_course_instance", "ablemusic");

                entity.HasIndex(e => e.CourseId)
                    .HasName("R_4");

                entity.HasIndex(e => e.OrgId)
                    .HasName("R_63");

                entity.HasIndex(e => e.RoomId)
                    .HasName("R_22");

                entity.HasIndex(e => e.TeacherId)
                    .HasName("R_13");

                entity.Property(e => e.GroupCourseInstanceId)
                    .HasColumnName("group_course_instance_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BeginDate)
                    .HasColumnName("begin_date")
                    .HasColumnType("date");

                entity.Property(e => e.CourseId)
                    .HasColumnName("course_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.EndDate)
                    .HasColumnName("end_date")
                    .HasColumnType("date");

                entity.Property(e => e.InvoiceDate)
                    .HasColumnName("invoice_date")
                    .HasColumnType("date");

                entity.Property(e => e.IsActivate)
                    .HasColumnName("is_activate")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsStarted)
                    .HasColumnName("is_started")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.OrgId)
                    .HasColumnName("org_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.RoomId)
                    .HasColumnName("room_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.TeacherId)
                    .HasColumnName("teacher_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.GroupCourseInstance)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("R_4");

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.GroupCourseInstance)
                    .HasForeignKey(d => d.OrgId)
                    .HasConstraintName("R_63");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.GroupCourseInstance)
                    .HasForeignKey(d => d.RoomId)
                    .HasConstraintName("R_22");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.GroupCourseInstance)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("R_13");
            });

            modelBuilder.Entity<Holiday>(entity =>
            {
                entity.ToTable("holiday", "ablemusic");

                entity.Property(e => e.HolidayId)
                    .HasColumnName("holiday_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.HolidayDate)
                    .HasColumnName("holiday_date")
                    .HasColumnType("date");

                entity.Property(e => e.HolidayName)
                    .HasColumnName("holiday_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("invoice", "ablemusic");

                entity.HasIndex(e => e.CourseInstanceId)
                    .HasName("R_74");

                entity.HasIndex(e => e.GroupCourseInstanceId)
                    .HasName("R_75");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_76");

                entity.HasIndex(e => e.TermId)
                    .HasName("R_71");

                entity.Property(e => e.InvoiceId)
                    .HasColumnName("invoice_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BeginDate)
                    .HasColumnName("begin_date")
                    .HasColumnType("date");

                entity.Property(e => e.ConcertFee)
                    .HasColumnName("concert_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.ConcertFeeName)
                    .HasColumnName("concert_fee_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.CourseInstanceId)
                    .HasColumnName("course_instance_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CourseName)
                    .HasColumnName("course_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.DueDate)
                    .HasColumnName("due_date")
                    .HasColumnType("date");

                entity.Property(e => e.EndDate)
                    .HasColumnName("end_date")
                    .HasColumnType("date");

                entity.Property(e => e.GroupCourseInstanceId)
                    .HasColumnName("group_course_instance_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.InvoiceNum)
                    .HasColumnName("invoice_num")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsPaid)
                    .HasColumnName("is_paid")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LearnerName)
                    .HasColumnName("learner_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.LessonFee)
                    .HasColumnName("lesson_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.LessonNoteFeeName)
                    .HasColumnName("lesson_note_fee_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.LessonQuantity)
                    .HasColumnName("lesson_quantity")
                    .HasColumnType("int(11)");

                entity.Property(e => e.NoteFee)
                    .HasColumnName("note_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.Other1Fee)
                    .HasColumnName("other1_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.Other1FeeName)
                    .HasColumnName("other1_fee_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.Other2Fee)
                    .HasColumnName("other2_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.Other2FeeName)
                    .HasColumnName("other2_fee_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.Other3Fee)
                    .HasColumnName("other3_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.Other3FeeName)
                    .HasColumnName("other3_fee_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.OwingFee)
                    .HasColumnName("owing_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.PaidFee)
                    .HasColumnName("paid_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.TermId)
                    .HasColumnName("term_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.TotalFee)
                    .HasColumnName("total_fee")
                    .HasColumnType("decimal(8,2)");

                entity.HasOne(d => d.CourseInstance)
                    .WithMany(p => p.Invoice)
                    .HasForeignKey(d => d.CourseInstanceId)
                    .HasConstraintName("R_74");

                entity.HasOne(d => d.GroupCourseInstance)
                    .WithMany(p => p.Invoice)
                    .HasForeignKey(d => d.GroupCourseInstanceId)
                    .HasConstraintName("R_75");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.Invoice)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_76");

                entity.HasOne(d => d.Term)
                    .WithMany(p => p.Invoice)
                    .HasForeignKey(d => d.TermId)
                    .HasConstraintName("R_71");
            });

            modelBuilder.Entity<InvoiceWaitingConfirm>(entity =>
            {
                entity.HasKey(e => e.WaitingId);

                entity.ToTable("invoice_waiting_confirm", "ablemusic");

                entity.HasIndex(e => e.CourseInstanceId)
                    .HasName("R_102");

                entity.HasIndex(e => e.GroupCourseInstanceId)
                    .HasName("R_103");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_101");

                entity.HasIndex(e => e.TermId)
                    .HasName("R_104");

                entity.Property(e => e.WaitingId)
                    .HasColumnName("waiting_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BeginDate)
                    .HasColumnName("begin_date")
                    .HasColumnType("date");

                entity.Property(e => e.ConcertFee)
                    .HasColumnName("concert_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.ConcertFeeName)
                    .HasColumnName("concert_fee_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.CourseInstanceId)
                    .HasColumnName("course_instance_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CourseName)
                    .HasColumnName("course_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.DueDate)
                    .HasColumnName("due_date")
                    .HasColumnType("date");

                entity.Property(e => e.EndDate)
                    .HasColumnName("end_date")
                    .HasColumnType("date");

                entity.Property(e => e.GroupCourseInstanceId)
                    .HasColumnName("group_course_instance_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.InvoiceNum)
                    .HasColumnName("invoice_num")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.IsActivate)
                    .HasColumnName("is_activate")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsConfirmed)
                    .HasColumnName("is_confirmed")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsEmailSent)
                    .HasColumnName("is_email_sent")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsPaid)
                    .HasColumnName("is_paid")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LearnerName)
                    .HasColumnName("learner_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.LessonFee)
                    .HasColumnName("lesson_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.LessonNoteFeeName)
                    .HasColumnName("lesson_note_fee_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.LessonQuantity)
                    .HasColumnName("lesson_quantity")
                    .HasColumnType("int(11)");

                entity.Property(e => e.NoteFee)
                    .HasColumnName("note_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.Other1Fee)
                    .HasColumnName("other1_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.Other1FeeName)
                    .HasColumnName("other1_fee_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.Other2Fee)
                    .HasColumnName("other2_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.Other2FeeName)
                    .HasColumnName("other2_fee_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.Other3Fee)
                    .HasColumnName("other3_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.Other3FeeName)
                    .HasColumnName("other3_fee_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.OwingFee)
                    .HasColumnName("owing_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.PaidFee)
                    .HasColumnName("paid_fee")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.TermId)
                    .HasColumnName("term_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.TotalFee)
                    .HasColumnName("total_fee")
                    .HasColumnType("decimal(8,2)");

                entity.HasOne(d => d.CourseInstance)
                    .WithMany(p => p.InvoiceWaitingConfirm)
                    .HasForeignKey(d => d.CourseInstanceId)
                    .HasConstraintName("R_102");

                entity.HasOne(d => d.GroupCourseInstance)
                    .WithMany(p => p.InvoiceWaitingConfirm)
                    .HasForeignKey(d => d.GroupCourseInstanceId)
                    .HasConstraintName("R_103");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.InvoiceWaitingConfirm)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_101");

                entity.HasOne(d => d.Term)
                    .WithMany(p => p.InvoiceWaitingConfirm)
                    .HasForeignKey(d => d.TermId)
                    .HasConstraintName("R_104");
            });

            modelBuilder.Entity<Language>(entity =>
            {
                entity.HasKey(e => e.LangId);

                entity.ToTable("language", "ablemusic");

                entity.Property(e => e.LangId)
                    .HasColumnName("lang_id")
                    .HasColumnType("tinyint(4)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.LangName)
                    .HasColumnName("lang_name")
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Learner>(entity =>
            {
                entity.ToTable("learner", "ablemusic");

                entity.HasIndex(e => e.FirstName)
                    .HasName("idx_learner_firstname");

                entity.HasIndex(e => e.OrgId)
                    .HasName("R_115");

                entity.HasIndex(e => e.UserId)
                    .HasName("R_106");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Address)
                    .HasColumnName("address")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.Comment)
                    .HasColumnName("comment")
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.ContactNum)
                    .HasColumnName("contact_num")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.Dob)
                    .HasColumnName("dob")
                    .HasColumnType("date");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.EnrollDate).HasColumnName("enroll_date");

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.FormUrl)
                    .HasColumnName("form_url")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.G5Certification)
                    .HasColumnName("g5_certification")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Gender)
                    .HasColumnName("gender")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsAbrsmG5)
                    .HasColumnName("is_abrsm_g5")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsUnder18)
                    .HasColumnName("is_under_18")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LearnerLevel)
                    .HasColumnName("learner_level")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.LevelType)
                    .HasColumnName("level_type")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.MiddleName)
                    .HasColumnName("middle_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Note)
                    .HasColumnName("note")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OrgId)
                    .HasColumnName("org_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.OtherfileUrl)
                    .HasColumnName("otherfile_url")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentPeriod)
                    .HasColumnName("payment_period")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.Photo)
                    .HasColumnName("photo")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Referrer)
                    .HasColumnName("referrer")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.ReferrerLearnerId)
                    .HasColumnName("referrer_learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.Learner)
                    .HasForeignKey(d => d.OrgId)
                    .HasConstraintName("R_115");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Learner)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("R_106");
            });

            modelBuilder.Entity<LearnerAppForm>(entity =>
            {
                entity.HasKey(e => e.AppFormId);

                entity.ToTable("learner_app_form", "ablemusic");

                entity.Property(e => e.AppFormId)
                    .HasColumnName("app_form_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Address)
                    .HasColumnName("address")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.ContactNum)
                    .HasColumnName("contact_num")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.Dob)
                    .HasColumnName("dob")
                    .HasColumnType("date");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.EnrollDate).HasColumnName("enroll_date");

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.G5Certification)
                    .HasColumnName("g5_certification")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Gender)
                    .HasColumnName("gender")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsAbrsmG5)
                    .HasColumnName("is_abrsm_g5")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsUnder18)
                    .HasColumnName("is_under_18")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.MiddleName)
                    .HasColumnName("middle_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Note)
                    .HasColumnName("note")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Photo)
                    .HasColumnName("photo")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReferrerLearnerId)
                    .HasColumnName("referrer_learner_id")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<LearnerGroupCourse>(entity =>
            {
                entity.ToTable("learner_group_course", "ablemusic");

                entity.HasIndex(e => e.GroupCourseInstanceId)
                    .HasName("R_87");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_86");

                entity.Property(e => e.LearnerGroupCourseId)
                    .HasColumnName("learner_group_course_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BeginDate)
                    .HasColumnName("begin_date")
                    .HasColumnType("date");

                entity.Property(e => e.Comment)
                    .HasColumnName("comment")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.EndDate)
                    .HasColumnName("end_date")
                    .HasColumnType("date");

                entity.Property(e => e.GroupCourseInstanceId)
                    .HasColumnName("group_course_instance_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.InvoiceDate)
                    .HasColumnName("invoice_date")
                    .HasColumnType("date");

                entity.Property(e => e.IsActivate)
                    .HasColumnName("is_activate")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.GroupCourseInstance)
                    .WithMany(p => p.LearnerGroupCourse)
                    .HasForeignKey(d => d.GroupCourseInstanceId)
                    .HasConstraintName("R_87");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.LearnerGroupCourse)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_86");
            });

            modelBuilder.Entity<LearnerOthers>(entity =>
            {
                entity.ToTable("learner_others", "ablemusic");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_119");

                entity.Property(e => e.LearnerOthersId)
                    .HasColumnName("learner_others_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LearnerLevel)
                    .HasColumnName("learner_level")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.OthersType)
                    .HasColumnName("others_type")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.OthersValue)
                    .HasColumnName("others_value")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.LearnerOthers)
                    .HasForeignKey(d => d.LearnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("R_119");
            });

            modelBuilder.Entity<LearnerTransaction>(entity =>
            {
                entity.HasKey(e => e.TranId);

                entity.ToTable("learner_transaction", "ablemusic");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_41");

                entity.HasIndex(e => e.LessonId)
                    .HasName("R_35");

                entity.Property(e => e.TranId)
                    .HasColumnName("tran_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LessonId)
                    .HasColumnName("lesson_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.LearnerTransaction)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_41");

                entity.HasOne(d => d.Lesson)
                    .WithMany(p => p.LearnerTransaction)
                    .HasForeignKey(d => d.LessonId)
                    .HasConstraintName("R_35");
            });

            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.ToTable("lesson", "ablemusic");

                entity.HasIndex(e => e.CourseInstanceId)
                    .HasName("R_66");

                entity.HasIndex(e => e.GroupCourseInstanceId)
                    .HasName("R_67");

                entity.HasIndex(e => e.InvoiceId)
                    .HasName("R_112");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_31");

                entity.HasIndex(e => e.OrgId)
                    .HasName("R_34");

                entity.HasIndex(e => e.RoomId)
                    .HasName("R_32");

                entity.HasIndex(e => e.TeacherId)
                    .HasName("R_33");

                entity.HasIndex(e => e.TrialCourseId)
                    .HasName("R_127");

                entity.Property(e => e.LessonId)
                    .HasColumnName("lesson_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BeginTime).HasColumnName("begin_time");

                entity.Property(e => e.CourseInstanceId)
                    .HasColumnName("course_instance_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.EndTime).HasColumnName("end_time");

                entity.Property(e => e.GroupCourseInstanceId)
                    .HasColumnName("group_course_instance_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.InvoiceId)
                    .HasColumnName("invoice_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IsCanceled)
                    .HasColumnName("is_canceled")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsChanged)
                    .HasColumnName("is_changed")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsConfirm)
                    .HasColumnName("is_confirm")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.IsTrial)
                    .HasColumnName("is_trial")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.OrgId)
                    .HasColumnName("org_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Reason)
                    .HasColumnName("reason")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.RoomId)
                    .HasColumnName("room_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.TeacherId)
                    .HasColumnName("teacher_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.TrialCourseId)
                    .HasColumnName("trial_course_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.CourseInstance)
                    .WithMany(p => p.Lesson)
                    .HasForeignKey(d => d.CourseInstanceId)
                    .HasConstraintName("R_66");

                entity.HasOne(d => d.GroupCourseInstance)
                    .WithMany(p => p.Lesson)
                    .HasForeignKey(d => d.GroupCourseInstanceId)
                    .HasConstraintName("R_67");

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.Lesson)
                    .HasForeignKey(d => d.InvoiceId)
                    .HasConstraintName("R_112");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.Lesson)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_31");

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.Lesson)
                    .HasForeignKey(d => d.OrgId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("R_34");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Lesson)
                    .HasForeignKey(d => d.RoomId)
                    .HasConstraintName("R_32");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.Lesson)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("R_33");

                entity.HasOne(d => d.TrialCourse)
                    .WithMany(p => p.Lesson)
                    .HasForeignKey(d => d.TrialCourseId)
                    .HasConstraintName("R_127");
            });

            modelBuilder.Entity<LessonRemain>(entity =>
            {
                entity.ToTable("lesson_remain", "ablemusic");

                entity.HasIndex(e => e.CourseInstanceId)
                    .HasName("R_73");

                entity.HasIndex(e => e.GroupCourseInstanceId)
                    .HasName("R_83");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_84");

                entity.HasIndex(e => e.TermId)
                    .HasName("R_72");

                entity.Property(e => e.LessonRemainId)
                    .HasColumnName("lesson_remain_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CourseInstanceId)
                    .HasColumnName("course_instance_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ExpiryDate)
                    .HasColumnName("expiry_date")
                    .HasColumnType("date");

                entity.Property(e => e.GroupCourseInstanceId)
                    .HasColumnName("group_course_instance_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Quantity)
                    .HasColumnName("quantity")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TermId)
                    .HasColumnName("term_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.CourseInstance)
                    .WithMany(p => p.LessonRemain)
                    .HasForeignKey(d => d.CourseInstanceId)
                    .HasConstraintName("R_73");

                entity.HasOne(d => d.GroupCourseInstance)
                    .WithMany(p => p.LessonRemain)
                    .HasForeignKey(d => d.GroupCourseInstanceId)
                    .HasConstraintName("R_83");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.LessonRemain)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_84");

                entity.HasOne(d => d.Term)
                    .WithMany(p => p.LessonRemain)
                    .HasForeignKey(d => d.TermId)
                    .HasConstraintName("R_72");
            });

            modelBuilder.Entity<LoginLog>(entity =>
            {
                entity.ToTable("login_log", "ablemusic");

                entity.HasIndex(e => e.OrgId)
                    .HasName("R_120");

                entity.HasIndex(e => e.UserId)
                    .HasName("R_62");

                entity.Property(e => e.LoginLogId)
                    .HasColumnName("login_log_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.LogType)
                    .HasColumnName("log_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.OrgId)
                    .HasColumnName("org_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.LoginLog)
                    .HasForeignKey(d => d.OrgId)
                    .HasConstraintName("R_120");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.LoginLog)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("R_62");
            });

            modelBuilder.Entity<Lookup>(entity =>
            {
                entity.ToTable("lookup", "ablemusic");

                entity.Property(e => e.LookupId)
                    .HasColumnName("lookup_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.LookupType)
                    .HasColumnName("lookup_type")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.PropName)
                    .HasColumnName("prop_name")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.PropValue)
                    .HasColumnName("prop_value")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<One2oneCourseInstance>(entity =>
            {
                entity.HasKey(e => e.CourseInstanceId);

                entity.ToTable("one2one_course_instance", "ablemusic");

                entity.HasIndex(e => e.CourseId)
                    .HasName("R_16");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_18");

                entity.HasIndex(e => e.OrgId)
                    .HasName("R_29");

                entity.HasIndex(e => e.RoomId)
                    .HasName("R_23");

                entity.HasIndex(e => e.TeacherId)
                    .HasName("R_17");

                entity.Property(e => e.CourseInstanceId)
                    .HasColumnName("course_instance_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BeginDate)
                    .HasColumnName("begin_date")
                    .HasColumnType("date");

                entity.Property(e => e.CourseId)
                    .HasColumnName("course_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.EndDate)
                    .HasColumnName("end_date")
                    .HasColumnType("date");

                entity.Property(e => e.InvoiceDate)
                    .HasColumnName("invoice_date")
                    .HasColumnType("date");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.OrgId)
                    .HasColumnName("org_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.RoomId)
                    .HasColumnName("room_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.TeacherId)
                    .HasColumnName("teacher_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.One2oneCourseInstance)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("R_16");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.One2oneCourseInstance)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_18");

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.One2oneCourseInstance)
                    .HasForeignKey(d => d.OrgId)
                    .HasConstraintName("R_29");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.One2oneCourseInstance)
                    .HasForeignKey(d => d.RoomId)
                    .HasConstraintName("R_23");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.One2oneCourseInstance)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("R_17");
            });

            modelBuilder.Entity<OnlineUser>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("online_user", "ablemusic");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("smallint(6)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.LoginTime).HasColumnName("login_time");

                entity.Property(e => e.LogoutTime).HasColumnName("logout_time");

                entity.Property(e => e.Token)
                    .HasColumnName("token")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithOne(p => p.OnlineUser)
                    .HasForeignKey<OnlineUser>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("R_61");
            });

            modelBuilder.Entity<Org>(entity =>
            {
                entity.ToTable("org", "ablemusic");

                entity.Property(e => e.OrgId)
                    .HasColumnName("org_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Abbr)
                    .HasColumnName("abbr")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Address)
                    .HasColumnName("address")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.IsActivate)
                    .HasColumnName("is_activate")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsHeadoffice)
                    .HasColumnName("is_headoffice")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.LocaltionX)
                    .HasColumnName("localtion_x")
                    .HasColumnType("decimal(10,7)");

                entity.Property(e => e.LocaltionY)
                    .HasColumnName("localtion_y")
                    .HasColumnType("decimal(10,7)");

                entity.Property(e => e.OrgName)
                    .HasColumnName("org_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasColumnName("phone")
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Page>(entity =>
            {
                entity.ToTable("page", "ablemusic");

                entity.HasIndex(e => e.PageGroupId)
                    .HasName("R_60");

                entity.Property(e => e.PageId)
                    .HasColumnName("page_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DisplayOrder)
                    .HasColumnName("display_order")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Icon)
                    .HasColumnName("icon")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.IsActivate)
                    .HasColumnName("is_activate")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.PageGroupId)
                    .HasColumnName("page_group_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.PageName)
                    .HasColumnName("page_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Para)
                    .HasColumnName("para")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ParaFlag)
                    .HasColumnName("para_flag")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.Url)
                    .HasColumnName("url")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.HasOne(d => d.PageGroup)
                    .WithMany(p => p.Page)
                    .HasForeignKey(d => d.PageGroupId)
                    .HasConstraintName("R_60");
            });

            modelBuilder.Entity<PageGroup>(entity =>
            {
                entity.ToTable("page_group", "ablemusic");

                entity.Property(e => e.PageGroupId)
                    .HasColumnName("page_group_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.DisplayOrder)
                    .HasColumnName("display_order")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Icon)
                    .HasColumnName("icon")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.PageGroupName)
                    .HasColumnName("page_group_name")
                    .HasMaxLength(60)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Parent>(entity =>
            {
                entity.ToTable("parent", "ablemusic");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_1");

                entity.Property(e => e.ParentId)
                    .HasColumnName("parent_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ContactNum)
                    .HasColumnName("contact_num")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Relationship)
                    .HasColumnName("relationship")
                    .HasColumnType("tinyint(4)");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.Parent)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_1");
            });

            modelBuilder.Entity<ParentAppForm>(entity =>
            {
                entity.HasKey(e => e.ParentFormId);

                entity.ToTable("parent_app_form", "ablemusic");

                entity.HasIndex(e => e.AppFormId)
                    .HasName("R_82");

                entity.Property(e => e.ParentFormId)
                    .HasColumnName("parent_form_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AppFormId)
                    .HasColumnName("app_form_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ContactNum)
                    .HasColumnName("contact_num")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ParentId)
                    .HasColumnName("parent_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Relationship)
                    .HasColumnName("relationship")
                    .HasColumnType("tinyint(4)");

                entity.HasOne(d => d.AppForm)
                    .WithMany(p => p.ParentAppForm)
                    .HasForeignKey(d => d.AppFormId)
                    .HasConstraintName("R_82");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payment", "ablemusic");

                entity.HasIndex(e => e.InvoiceId)
                    .HasName("R_56");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_38");

                entity.HasIndex(e => e.StaffId)
                    .HasName("R_39");

                entity.Property(e => e.PaymentId)
                    .HasColumnName("payment_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AfterBalance)
                    .HasColumnName("after_balance")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.BeforeBalance)
                    .HasColumnName("before_balance")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.Comment)
                    .HasColumnName("comment")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.InvoiceId)
                    .HasColumnName("invoice_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IsConfirmed)
                    .HasColumnName("is_confirmed")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PaymentMethod)
                    .HasColumnName("payment_method")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.PaymentType)
                    .HasColumnName("payment_type")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.StaffId)
                    .HasColumnName("staff_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.Payment)
                    .HasForeignKey(d => d.InvoiceId)
                    .HasConstraintName("R_56");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.Payment)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_38");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.Payment)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("R_39");
            });

            modelBuilder.Entity<ProdCat>(entity =>
            {
                entity.ToTable("prod_cat", "ablemusic");

                entity.Property(e => e.ProdCatId)
                    .HasColumnName("prod_cat_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.ProdCatName)
                    .HasColumnName("prod_cat_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ProdType>(entity =>
            {
                entity.ToTable("prod_type", "ablemusic");

                entity.HasIndex(e => e.ProdCatId)
                    .HasName("R_88");

                entity.Property(e => e.ProdTypeId)
                    .HasColumnName("prod_type_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.ProdCatId)
                    .HasColumnName("prod_cat_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.ProdTypeName)
                    .HasColumnName("prod_type_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.HasOne(d => d.ProdCat)
                    .WithMany(p => p.ProdType)
                    .HasForeignKey(d => d.ProdCatId)
                    .HasConstraintName("R_88");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("product", "ablemusic");

                entity.HasIndex(e => e.ProdTypeId)
                    .HasName("R_69");

                entity.Property(e => e.ProductId)
                    .HasColumnName("product_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Brand)
                    .HasColumnName("brand")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Model)
                    .HasColumnName("model")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ProdTypeId)
                    .HasColumnName("prod_type_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.ProductName)
                    .HasColumnName("product_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SellPrice)
                    .HasColumnName("sell_price")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.WholesalePrice)
                    .HasColumnName("wholesale_price")
                    .HasColumnType("decimal(8,2)");

                entity.HasOne(d => d.ProdType)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.ProdTypeId)
                    .HasConstraintName("R_69");
            });

            modelBuilder.Entity<Qualification>(entity =>
            {
                entity.HasKey(e => e.QualiId);

                entity.ToTable("qualification", "ablemusic");

                entity.Property(e => e.QualiId)
                    .HasColumnName("quali_id")
                    .HasColumnType("tinyint(4)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.QualiName)
                    .HasColumnName("quali_name")
                    .HasMaxLength(60)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.ToTable("rating", "ablemusic");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_124");

                entity.HasIndex(e => e.TeacherId)
                    .HasName("R_1251");

                entity.Property(e => e.RatingId)
                    .HasColumnName("rating_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Comment)
                    .HasColumnName("comment")
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.CreateAt).HasColumnName("create_at");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.RateType)
                    .HasColumnName("rate_type")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.TeacherId)
                    .HasColumnName("teacher_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.Rating)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_124");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.Rating)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("R_1251");
            });

            modelBuilder.Entity<RemindLog>(entity =>
            {
                entity.HasKey(e => e.RemindId);

                entity.ToTable("remind_log", "ablemusic");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_77");

                entity.HasIndex(e => e.LessonId)
                    .HasName("R_107");

                entity.HasIndex(e => e.TeacherId)
                    .HasName("R_78");

                entity.Property(e => e.RemindId)
                    .HasColumnName("remind_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.EmailAt).HasColumnName("email_at");

                entity.Property(e => e.IsLearner)
                    .HasColumnName("is_learner")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LessonId)
                    .HasColumnName("lesson_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ProcessFlag)
                    .HasColumnName("process_flag")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.ReceivedFlag)
                    .HasColumnName("received_flag")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.RemindContent)
                    .HasColumnName("remind_content")
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.RemindTitle)
                    .HasColumnName("remind_title")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.RemindType)
                    .HasColumnName("remind_type")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.TeacherId)
                    .HasColumnName("teacher_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.RemindLog)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_77");

                entity.HasOne(d => d.Lesson)
                    .WithMany(p => p.RemindLog)
                    .HasForeignKey(d => d.LessonId)
                    .HasConstraintName("R_107");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.RemindLog)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("R_78");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("role", "ablemusic");

                entity.Property(e => e.RoleId)
                    .HasColumnName("role_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.RoleName)
                    .HasColumnName("role_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RoleAccess>(entity =>
            {
                entity.ToTable("role_access", "ablemusic");

                entity.HasIndex(e => e.PageId)
                    .HasName("R_59");

                entity.HasIndex(e => e.RoleId)
                    .HasName("R_58");

                entity.Property(e => e.RoleAccessId)
                    .HasColumnName("role_access_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.IsMobile)
                    .HasColumnName("is_mobile")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.PageId)
                    .HasColumnName("page_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.RoleId)
                    .HasColumnName("role_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Page)
                    .WithMany(p => p.RoleAccess)
                    .HasForeignKey(d => d.PageId)
                    .HasConstraintName("R_59");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RoleAccess)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("R_58");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("room", "ablemusic");

                entity.HasIndex(e => e.OrgId)
                    .HasName("R_21");

                entity.Property(e => e.RoomId)
                    .HasColumnName("room_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.IsActivate)
                    .HasColumnName("is_activate")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.OrgId)
                    .HasColumnName("org_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.RoomName)
                    .HasColumnName("room_name")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.Room)
                    .HasForeignKey(d => d.OrgId)
                    .HasConstraintName("R_21");
            });

            modelBuilder.Entity<SoldTransaction>(entity =>
            {
                entity.HasKey(e => e.TranId);

                entity.ToTable("sold_transaction", "ablemusic");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_48");

                entity.HasIndex(e => e.PaymentId)
                    .HasName("R_94");

                entity.HasIndex(e => e.ProductId)
                    .HasName("R_47");

                entity.HasIndex(e => e.StockId)
                    .HasName("R_68");

                entity.Property(e => e.TranId)
                    .HasColumnName("tran_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AflterQuantity)
                    .HasColumnName("aflter_quantity")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.Balance)
                    .HasColumnName("balance")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.BeforeQuantity)
                    .HasColumnName("before_quantity")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.DiscountAmount)
                    .HasColumnName("discount_amount")
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.DiscountRate)
                    .HasColumnName("discount_rate")
                    .HasColumnType("decimal(4,2)");

                entity.Property(e => e.DiscountedAmount)
                    .HasColumnName("discounted_amount")
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Note)
                    .HasColumnName("note")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentId)
                    .HasColumnName("payment_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ProductId)
                    .HasColumnName("product_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.SoldQuantity)
                    .HasColumnName("sold_quantity")
                    .HasColumnType("int(11)");

                entity.Property(e => e.StockId)
                    .HasColumnName("stock_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.SoldTransaction)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_44");

                entity.HasOne(d => d.LearnerNavigation)
                    .WithMany(p => p.SoldTransaction)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_48");

                entity.HasOne(d => d.Payment)
                    .WithMany(p => p.SoldTransaction)
                    .HasForeignKey(d => d.PaymentId)
                    .HasConstraintName("R_94");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.SoldTransaction)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("R_47");

                entity.HasOne(d => d.Stock)
                    .WithMany(p => p.SoldTransaction)
                    .HasForeignKey(d => d.StockId)
                    .HasConstraintName("R_68");
            });

            modelBuilder.Entity<Staff>(entity =>
            {
                entity.ToTable("staff", "ablemusic");

                entity.HasIndex(e => e.UserId)
                    .HasName("R_79");

                entity.Property(e => e.StaffId)
                    .HasColumnName("staff_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Dob)
                    .HasColumnName("dob")
                    .HasColumnType("date");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ExpiryDate)
                    .HasColumnName("expiry_date")
                    .HasColumnType("date");

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Gender)
                    .HasColumnName("gender")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.HomePhone)
                    .HasColumnName("home_phone")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.IdNumber)
                    .HasColumnName("id_number")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.IdPhoto)
                    .HasColumnName("id_photo")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.IdType)
                    .HasColumnName("id_type")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IrdNumber)
                    .HasColumnName("ird_number")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.IsActivate)
                    .HasColumnName("is_activate")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.MobilePhone)
                    .HasColumnName("mobile_phone")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Photo)
                    .HasColumnName("photo")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Visa)
                    .HasColumnName("visa")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Staff)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("R_79");
            });

            modelBuilder.Entity<StaffOrg>(entity =>
            {
                entity.ToTable("staff_org", "ablemusic");

                entity.HasIndex(e => e.OrgId)
                    .HasName("R_93");

                entity.HasIndex(e => e.StaffId)
                    .HasName("R_92");

                entity.Property(e => e.StaffOrgId)
                    .HasColumnName("staff_org_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.IsActivate)
                    .HasColumnName("is_activate")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.OrgId)
                    .HasColumnName("org_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.StaffId)
                    .HasColumnName("staff_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.StaffOrg)
                    .HasForeignKey(d => d.OrgId)
                    .HasConstraintName("R_93");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.StaffOrg)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("R_92");
            });

            modelBuilder.Entity<Stock>(entity =>
            {
                entity.ToTable("stock", "ablemusic");

                entity.HasIndex(e => e.OrgId)
                    .HasName("R_45");

                entity.HasIndex(e => e.ProductId)
                    .HasName("R_46");

                entity.Property(e => e.StockId)
                    .HasColumnName("stock_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.OrgId)
                    .HasColumnName("org_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.ProductId)
                    .HasColumnName("product_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Quantity)
                    .HasColumnName("quantity")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.Stock)
                    .HasForeignKey(d => d.OrgId)
                    .HasConstraintName("R_45");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Stock)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("R_46");
            });

            modelBuilder.Entity<StockApplication>(entity =>
            {
                entity.HasKey(e => e.ApplicationId);

                entity.ToTable("stock_application", "ablemusic");

                entity.HasIndex(e => e.ApplyStaffId)
                    .HasName("R_132");

                entity.HasIndex(e => e.OrgId)
                    .HasName("R_131");

                entity.Property(e => e.ApplicationId)
                    .HasColumnName("application_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ApplyAt).HasColumnName("apply_at");

                entity.Property(e => e.ApplyReason)
                    .HasColumnName("apply_reason")
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.ApplyStaffId)
                    .HasColumnName("apply_staff_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.DeliverAt).HasColumnName("deliver_at");

                entity.Property(e => e.IsDisputed)
                    .HasColumnName("is_disputed")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.OrgId)
                    .HasColumnName("org_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.ProcessStatus)
                    .HasColumnName("process_status")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.RecieveAt).HasColumnName("recieve_at");

                entity.Property(e => e.ReplyAt).HasColumnName("reply_at");

                entity.Property(e => e.ReplyContent)
                    .HasColumnName("reply_content")
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.HasOne(d => d.ApplyStaff)
                    .WithMany(p => p.StockApplication)
                    .HasForeignKey(d => d.ApplyStaffId)
                    .HasConstraintName("R_132");

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.StockApplication)
                    .HasForeignKey(d => d.OrgId)
                    .HasConstraintName("R_131");
            });

            modelBuilder.Entity<StockOrder>(entity =>
            {
                entity.HasKey(e => e.OrderId);

                entity.ToTable("stock_order", "ablemusic");

                entity.HasIndex(e => e.OrgId)
                    .HasName("R_51");

                entity.HasIndex(e => e.ProductId)
                    .HasName("R_50");

                entity.HasIndex(e => e.StaffId)
                    .HasName("R_52");

                entity.HasIndex(e => e.StockId)
                    .HasName("R_49");

                entity.Property(e => e.OrderId)
                    .HasColumnName("order_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BuyingPrice)
                    .HasColumnName("buying_price")
                    .HasColumnType("decimal(8,2)");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.OrderType)
                    .HasColumnName("order_type")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.OrgId)
                    .HasColumnName("org_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.ProductId)
                    .HasColumnName("product_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Quantity)
                    .HasColumnName("quantity")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ReceiptImg)
                    .HasColumnName("receipt_img")
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.StaffId)
                    .HasColumnName("staff_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.StockId)
                    .HasColumnName("stock_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Org)
                    .WithMany(p => p.StockOrder)
                    .HasForeignKey(d => d.OrgId)
                    .HasConstraintName("R_51");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.StockOrder)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("R_50");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.StockOrder)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("R_52");

                entity.HasOne(d => d.Stock)
                    .WithMany(p => p.StockOrder)
                    .HasForeignKey(d => d.StockId)
                    .HasConstraintName("R_49");
            });

            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.ToTable("teacher", "ablemusic");

                entity.HasIndex(e => e.FirstName)
                    .HasName("idx_teacher_firstname");

                entity.HasIndex(e => e.UserId)
                    .HasName("R_12");

                entity.Property(e => e.TeacherId)
                    .HasColumnName("teacher_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Ability)
                    .HasColumnName("ability")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Comment)
                    .HasColumnName("comment")
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.CvUrl)
                    .HasColumnName("cv_url")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Dob)
                    .HasColumnName("dob")
                    .HasColumnType("date");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.ExpiryDate)
                    .HasColumnName("expiry_date")
                    .HasColumnType("date");

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.FormUrl)
                    .HasColumnName("form_url")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Gender)
                    .HasColumnName("gender")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.HomePhone)
                    .HasColumnName("home_phone")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.IdNumber)
                    .HasColumnName("id_number")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.IdPhoto)
                    .HasColumnName("id_photo")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.IdType)
                    .HasColumnName("id_type")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.InvoiceTemplate)
                    .HasColumnName("invoice_template")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.IrdNumber)
                    .HasColumnName("ird_number")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.IsActivate)
                    .HasColumnName("is_activate")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsContract)
                    .HasColumnName("is_contract")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsLeft)
                    .HasColumnName("is_left")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.MinimumHours)
                    .HasColumnName("minimum_hours")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.MobilePhone)
                    .HasColumnName("mobile_phone")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.OtherfileUrl)
                    .HasColumnName("otherfile_url")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Photo)
                    .HasColumnName("photo")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Visa)
                    .HasColumnName("visa")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Teacher)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("R_12");
            });

            modelBuilder.Entity<TeacherCourse>(entity =>
            {
                entity.ToTable("teacher_course", "ablemusic");

                entity.HasIndex(e => e.CourseId)
                    .HasName("R_24");

                entity.HasIndex(e => e.TeacherId)
                    .HasName("R_25");

                entity.Property(e => e.TeacherCourseId)
                    .HasColumnName("teacher_course_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CourseId)
                    .HasColumnName("course_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.HourlyWage)
                    .HasColumnName("hourly_wage")
                    .HasColumnType("decimal(6,2)");

                entity.Property(e => e.TeacherId)
                    .HasColumnName("teacher_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.TeacherCourse)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("R_24");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.TeacherCourse)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("R_25");
            });

            modelBuilder.Entity<TeacherLanguage>(entity =>
            {
                entity.HasKey(e => e.TeacherLangId);

                entity.ToTable("teacher_language", "ablemusic");

                entity.HasIndex(e => e.LangId)
                    .HasName("R_7");

                entity.HasIndex(e => e.TeacherId)
                    .HasName("R_6");

                entity.Property(e => e.TeacherLangId)
                    .HasColumnName("teacher_lang_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.LangId)
                    .HasColumnName("lang_id")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.TeacherId)
                    .HasColumnName("teacher_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Lang)
                    .WithMany(p => p.TeacherLanguage)
                    .HasForeignKey(d => d.LangId)
                    .HasConstraintName("R_7");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.TeacherLanguage)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("R_6");
            });

            modelBuilder.Entity<TeacherQualificatiion>(entity =>
            {
                entity.HasKey(e => e.TeacherQualiId);

                entity.ToTable("teacher_qualificatiion", "ablemusic");

                entity.HasIndex(e => e.QualiId)
                    .HasName("R_8");

                entity.HasIndex(e => e.TeacherId)
                    .HasName("R_5");

                entity.Property(e => e.TeacherQualiId)
                    .HasColumnName("teacher_quali_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.QualiId)
                    .HasColumnName("quali_id")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.TeacherId)
                    .HasColumnName("teacher_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Quali)
                    .WithMany(p => p.TeacherQualificatiion)
                    .HasForeignKey(d => d.QualiId)
                    .HasConstraintName("R_8");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.TeacherQualificatiion)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("R_5");
            });

            modelBuilder.Entity<TeacherTransaction>(entity =>
            {
                entity.HasKey(e => e.TranId);

                entity.ToTable("teacher_transaction", "ablemusic");

                entity.HasIndex(e => e.LessonId)
                    .HasName("R_42");

                entity.HasIndex(e => e.TeacherId)
                    .HasName("R_125");

                entity.Property(e => e.TranId)
                    .HasColumnName("tran_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.LessonId)
                    .HasColumnName("lesson_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TeacherId)
                    .HasColumnName("teacher_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.WageAmount)
                    .HasColumnName("wage_amount")
                    .HasColumnType("decimal(6,2)");

                entity.HasOne(d => d.Lesson)
                    .WithMany(p => p.TeacherTransaction)
                    .HasForeignKey(d => d.LessonId)
                    .HasConstraintName("R_42");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.TeacherTransaction)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("R_125");
            });

            modelBuilder.Entity<TeacherWageRates>(entity =>
            {
                entity.HasKey(e => e.RatesId);

                entity.ToTable("teacher_wage_rates", "ablemusic");

                entity.HasIndex(e => e.TeacherId)
                    .HasName("R_126");

                entity.Property(e => e.RatesId)
                    .HasColumnName("rates_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreateAt).HasColumnName("create_at");

                entity.Property(e => e.GroupRates)
                    .HasColumnName("group_rates")
                    .HasColumnType("decimal(6,2)");

                entity.Property(e => e.IsActivate)
                    .HasColumnName("is_activate")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.OthersRates)
                    .HasColumnName("others_rates")
                    .HasColumnType("decimal(6,2)");

                entity.Property(e => e.PianoRates)
                    .HasColumnName("piano_rates")
                    .HasColumnType("decimal(6,2)");

                entity.Property(e => e.TeacherId)
                    .HasColumnName("teacher_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.TheoryRates)
                    .HasColumnName("theory_rates")
                    .HasColumnType("decimal(6,2)");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.TeacherWageRates)
                    .HasForeignKey(d => d.TeacherId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("R_126");
            });

            modelBuilder.Entity<Term>(entity =>
            {
                entity.ToTable("term", "ablemusic");

                entity.Property(e => e.TermId)
                    .HasColumnName("term_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.BeginDate)
                    .HasColumnName("begin_date")
                    .HasColumnType("date");

                entity.Property(e => e.EndDate)
                    .HasColumnName("end_date")
                    .HasColumnType("date");

                entity.Property(e => e.TermName)
                    .HasColumnName("term_name")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.TermType)
                    .HasColumnName("term_type")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.WeekQuantity)
                    .HasColumnName("week_quantity")
                    .HasColumnType("smallint(6)");
            });

            modelBuilder.Entity<TodoList>(entity =>
            {
                entity.HasKey(e => e.ListId);

                entity.ToTable("todo_list", "ablemusic");

                entity.HasIndex(e => e.LearnerId)
                    .HasName("R_109");

                entity.HasIndex(e => e.LessonId)
                    .HasName("R_108");

                entity.HasIndex(e => e.TeacherId)
                    .HasName("R_110");

                entity.HasIndex(e => e.UserId)
                    .HasName("R_64");

                entity.Property(e => e.ListId)
                    .HasColumnName("list_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.LearnerId)
                    .HasColumnName("learner_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LessonId)
                    .HasColumnName("lesson_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ListContent)
                    .HasColumnName("list_content")
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.ListName)
                    .HasColumnName("list_name")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.ProcessFlag)
                    .HasColumnName("process_flag")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");

                entity.Property(e => e.TeacherId)
                    .HasColumnName("teacher_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.TodoDate)
                    .HasColumnName("todo_date")
                    .HasColumnType("date");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("smallint(6)");

                entity.HasOne(d => d.Learner)
                    .WithMany(p => p.TodoList)
                    .HasForeignKey(d => d.LearnerId)
                    .HasConstraintName("R_109");

                entity.HasOne(d => d.Lesson)
                    .WithMany(p => p.TodoList)
                    .HasForeignKey(d => d.LessonId)
                    .HasConstraintName("R_108");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.TodoList)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("R_110");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TodoList)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("R_64");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user", "ablemusic");

                entity.HasIndex(e => e.RoleId)
                    .HasName("R_57");

                entity.HasIndex(e => e.UserName)
                    .HasName("idx_username");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.IsActivate)
                    .HasColumnName("is_activate")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsOnline)
                    .HasColumnName("is_online")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.RoleId)
                    .HasColumnName("role_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Signature)
                    .HasColumnName("signature")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.UnreadMessageId)
                    .HasColumnName("unread_message_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.UserName)
                    .HasColumnName("user_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("R_57");
            });
        }
    }
}
