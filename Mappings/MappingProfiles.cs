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
            CreateMap<Parents,Parent>();
        }
    }
}
