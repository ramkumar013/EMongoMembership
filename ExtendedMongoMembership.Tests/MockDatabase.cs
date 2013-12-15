using System.Collections.Generic;
using System.Linq;

namespace ExtendedMongoMembership.Tests
{
    public abstract class MockDatabase : ISession
    {
        public abstract void Add<T>(IEnumerable<T> items) where T : class;

        public abstract void Add<T>(T item) where T : class;

        public abstract bool CreateUserRow(string userName, string UserNameColumn, string userTableName, IDictionary<string, object> values);

        public abstract void DeleteById(object id, string collectionName);

        public abstract void DeleteById<T>(object id) where T : class;

        public abstract void DeleteByQuery<T>(MongoDB.Driver.IMongoQuery query) where T : class;

        public abstract int GetUserId(string userTableName, string userNameColumn, string userName);

        public abstract IQueryable<OAuthToken> OAuthTokens { get; }

        public abstract IQueryable<MembershipRole> Roles { get; }

        public abstract void Save<T>(T item) where T : class;

        public abstract void Update<T>(T item) where T : class;

        public abstract IQueryable<MembershipAccount> Users { get; }
        
        public abstract void Dispose();
    }
}
