using ExtendedMongoMembership.Entities;

namespace ExtendedMongoMembership.Sample.Models
{
    public class SampleUserProfile : MembershipAccountBase
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
    }
}