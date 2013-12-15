using ExtendedMongoMembership.Sample.Models;
using ExtendedMongoMembership.Services;

namespace ExtendedMongoMembership.Sample.Services
{
    public class DefaultUserProfileService : UserProfileServiceBase<SampleUserProfile>
    {
        public DefaultUserProfileService(string connectionString)
            : base(connectionString)
        {

        }
    }
}