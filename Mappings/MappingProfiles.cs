using AutoMapper;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;

namespace Pegasus_backend.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<TeachersRegister, Teacher>();
            CreateMap<StudentRegister, Learner>();
            CreateMap<Models.Parent,pegasusContext.Parent>();
            CreateMap<InvoicePay,Payment>();
            CreateMap<ProdCatModel, ProdCat>();
            CreateMap<ProdTypeModel, ProdType>();
            CreateMap<ProductModel, Product>();
            CreateMap<PaymentTranModel, Payment>();
            //CreateMap<PaymentTranModel, SoldTransaction>();
            CreateMap<SoldTransactionModel, SoldTransaction>();
            CreateMap<CourseViewModel, Course>();
            CreateMap<TeachersUpdate, Teacher>();
            CreateMap<InvoiceWaitingConfirmViewModel, InvoiceWaitingConfirm>();
            CreateMap<GroupCourseInstanceModel, GroupCourseInstance>();
            CreateMap<OnetoOneCourseInstanceModel, One2oneCourseInstance>();
            CreateMap<LessonViewModel, Lesson>();
            CreateMap<TeacherCourseModel, TeacherCourse>();
            CreateMap<LoginLogModel, LoginLog>();
            CreateMap<Invoice, WaitingOrInvoice>();
            CreateMap<InvoiceWaitingConfirm, WaitingOrInvoice>();
            CreateMap<StaffModel, Staff>();
        }
    }
}
