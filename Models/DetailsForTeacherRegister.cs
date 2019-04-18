using System.Collections.Generic;
using Pegasus_backend.pegasusContext;
namespace Pegasus_backend.Models
{
    public class DetailsForTeacherRegister
    {
        public List<Qualification> qualifications;
        public List<Language> Languages;
        public List<pegasusContext.Org> Orgs;
    }
}