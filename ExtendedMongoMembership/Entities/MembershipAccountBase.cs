using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ExtendedMongoMembership.Entities
{
    [BsonIgnoreExtraElements]
    public class MembershipAccountBase
    {
        public System.Guid UserId { get; set; }
        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }
        public string UserName { get; set; }
    }
}
