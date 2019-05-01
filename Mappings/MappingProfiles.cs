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
            CreateMap<ProdCat, ProdCat>();
            CreateMap<ProdTypeModel, ProdType>();
            CreateMap<ProductModel, Product>();
        }
    }
}
