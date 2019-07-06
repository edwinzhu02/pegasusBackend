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
            CreateMap<LessonRemain,CourseRemain>();
            //CreateMap<CourseRemain, LessonRemainWithUnfonfirmLessons>();
            //CreateMap<PaymentTranModel, SoldTransaction>();
            CreateMap<SoldTransactionModel, SoldTransaction>();
            CreateMap<CourseViewModel, Course>();
            CreateMap<TeachersUpdate, Teacher>();
            CreateMap<InvoiceWaitingConfirmViewModel, InvoiceWaitingConfirm>();
            CreateMap<GroupCourseInstanceModel, GroupCourseInstance>();
            CreateMap<OnetoOneCourseInstanceModel, One2oneCourseInstance>();
            CreateMap<LessonViewModel, Lesson>();
            CreateMap<LoginLogModel, LoginLog>();
            CreateMap<Invoice, WaitingOrInvoice>();
            CreateMap<InvoiceWaitingConfirm, Invoice>();
            CreateMap<InvoiceWaitingConfirm, WaitingOrInvoice>();
            CreateMap<StaffModel, Staff>();
            CreateMap<One2oneCourseInstance, OneOrGroupCourse>();
            CreateMap<GroupCourseInstance, OneOrGroupCourse>();
            CreateMap<PageModel, Page>();
            
            //Start: Get Learner Model
            CreateMap<GetLearnerModel, Learner>();
            CreateMap<GetLearnerParentModel, pegasusContext.Parent>();
            CreateMap<GetLearnerLearnerOthers, LearnerOthers>();
            CreateMap<GetLearnerOnetoOneCourseInstance, One2oneCourseInstance>();
            CreateMap<GetLearnerOnetoOneCourseInstanceOrg, pegasusContext.Org>();
            CreateMap<GetLearnerOnetoOneCourseInstanceCourse, Course>();
            CreateMap<GetLearnerOnetoOneCourseInstanceRoom, Room>();
            CreateMap<GetLearnerOnetoOneCourseInstanceCourseSchedule,CourseSchedule>();
            CreateMap<GetLearnerOnetoOneCourseInstanceCourseTeacher, Teacher>();
            CreateMap<GetLearnerLearnerGroupCourse, LearnerGroupCourse>();
            CreateMap<GetLearnerOnetoOneCourseInstanceCourseAmendment, Amendment>();
            CreateMap<GetLearnerLearnerGroupCourseGroupCourseInstance,GroupCourseInstance>();
            CreateMap<GetLearnerLearnerGroupCourseGroupCourseInstanceTeacher,Teacher>();
            CreateMap<GetLearnerLearnerGroupCourseGroupCourseInstanceCourseSchedule, CourseSchedule>();
            CreateMap<GetLearnerLearnerGroupCourseGroupCourseInstanceCourse, Course>();
            CreateMap<GetLearnerLearnerGroupCourseGroupCourseInstanceRoom, Room>();
            //End
            //Payment
            CreateMap<GetPaymentModel, Payment>();
            CreateMap<PaymentInvoiceModel, Invoice>();
            CreateMap<PaymentLearnerModel, Learner>();            
            CreateMap<PaymentSoldTransaction, SoldTransaction>();            
            CreateMap<PaymentProductModel, Product>();  
            CreateMap<PaymentStaffModel, Staff>(); 

            CreateMap<ChatMessageModel, ChatMessage>();
        }
    }
}
