using AutoMapper;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;

namespace jupiterCore.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<ProdCatModel, ProdCat>();
            CreateMap<ProdTypeModel, ProdType>();
        }
    }
}