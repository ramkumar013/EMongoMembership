using ExtendedMongoMembership.Entities;
using ExtendedMongoMembership.Helpers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;


namespace ExtendedMongoMembership
{

    /*internal*/
    public class MongoSession : IDisposable
    {
        private MongoClient _client;
        private MongoServer _server;
        private MongoDatabase _provider;

        private MongoUrl _mongoUrl;
        static MongoSession()
        {
            CreateBsonClassMap();
        }
        public MongoSession()
        {
            var connectionString = SecurityHelper.Decrypt(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString, true);
            _mongoUrl = new MongoUrl(connectionString);
        }

        private static void CreateBsonClassMap()
        {
            BsonClassMap.RegisterClassMap<MembershipAccountBase>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.UserId));
                cm.IdMemberMap.SetIdGenerator(NullIdChecker.Instance).SetSerializer(new GuidSerializer());
            });

            BsonClassMap.RegisterClassMap<MembershipAccount>(cm =>
            {
                cm.AutoMap();
                cm.MapProperty(x => x.ConfirmationToken);
                cm.MapProperty(x => x.CreateDate);
                cm.MapProperty(x => x.IsConfirmed);
                cm.MapProperty(x => x.LastPasswordFailureDate);
                cm.MapProperty(x => x.PasswordFailuresSinceLastSuccess);
                cm.MapProperty(x => x.Password);
                cm.MapProperty(x => x.PasswordChangedDate);
                cm.MapProperty(x => x.PasswordSalt);
                cm.MapProperty(x => x.PasswordVerificationToken);
                cm.MapProperty(x => x.PasswordVerificationTokenExpirationDate);
                cm.MapProperty(x => x.LastLoginDate);
                cm.MapProperty(x => x.Roles);
                cm.MapProperty(x => x.OAuthData);
            });

            BsonClassMap.RegisterClassMap<MembershipRole>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.RoleId));
                cm.IdMemberMap.SetIdGenerator(NullIdChecker.Instance).SetSerializer(new GuidSerializer());
            });
        }

        public MongoSession(string connectionString)
        {
            _mongoUrl = new MongoUrl(SecurityHelper.Decrypt(connectionString, true));
            _client = new MongoClient(_mongoUrl);
            _server = _client.GetServer();
            _provider = _server.GetDatabase(_mongoUrl.DatabaseName, WriteConcern.Acknowledged);
        }

        public MongoDatabase MongoDatabase { get { return this._provider; } }


        public IQueryable<MembershipAccount> Users
        {
            get { return _provider.GetCollection<MembershipAccount>("Users").AsQueryable(); }
        }

        public IQueryable<MembershipRole> Roles
        {
            get { return _provider.GetCollection<MembershipRole>("Roles").AsQueryable(); }
        }

        public IQueryable<OAuthToken> OAuthTokens
        {
            get { return _provider.GetCollection<OAuthToken>("OAuthTokens").AsQueryable(); }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _server.Disconnect();
        }

        #endregion

        public void Add<T>(T item) where T : class
        {
            Type t = typeof(T);
            if (t == typeof(MembershipAccount))
            {
                var acc = item as MembershipAccount;
                //acc.UserId = GetNextSequence("user_id");
                _provider.GetCollection<MembershipAccount>(GetCollectionName<MembershipAccount>()).Insert(acc);
            }
            else
            {
                _provider.GetCollection<T>(GetCollectionName<T>()).Insert(item);
            }
        }

        public void Add<T>(IEnumerable<T> items) where T : class
        {
            _provider.GetCollection<T>(GetCollectionName<T>()).Insert(items);
        }

        public void Save<T>(T item) where T : class
        {
            _provider.GetCollection<T>(GetCollectionName<T>()).Save(item);
        }

        public void Update<T>(T item) where T : class
        {
            Save(item);
        }

        public void DeleteByQuery<T>(IMongoQuery query) where T : class
        {
            _provider.GetCollection<T>(GetCollectionName<T>()).Remove(query);
        }

        public void DeleteById<T>(object id) where T : class
        {
            IMongoQuery query = Query.EQ("_id", id.ToString());
            _provider.GetCollection<T>(GetCollectionName<T>()).Remove(query);
        }

        public void DeleteById(object id, string collectionName)
        {
            IMongoQuery query = Query.EQ("_id", id.ToString());
            _provider.GetCollection(collectionName).Remove(query);
        }

        public void Drop<T>()
        {

            var col = _provider.GetCollection<T>(GetCollectionName<T>());
            _provider.DropCollection(typeof(T).Name);
        }

        private string GetCollectionName<T>()
        {
            string result = string.Empty;
            Type t = typeof(T);

            if (t == typeof(MembershipAccount))
            {
                result = "Users";
            }
            else if (t == typeof(MembershipRole))
            {
                result = "Roles";
            }
            else if (t == typeof(OAuthToken))
            {
                result = "OAuthTokens";
            }

            return result;
        }

        public int GetNextSequence(string name)
        {
            var collection = _provider.GetCollection("Counters");
            IMongoQuery query = Query.EQ("_id", name);
            var sortBy = SortBy.Descending("_id");
            IMongoUpdate update = MongoDB.Driver.Builders.Update.Inc("seq", 1);
            var result = collection.FindAndModify(query, sortBy, update, true, true);

            return int.Parse(result.ModifiedDocument[1].ToString());
        }

        public Guid GetUserId(string userTableName, string userNameColumn, string userName)
        {
            var collection = _provider.GetCollection(userTableName);
            IMongoQuery query = Query.EQ(userNameColumn, userName);
            var result = collection.FindOne(query);
            if (result == null)
                return Guid.Empty;

            return result["_id"].AsGuid;
        }

        public bool CreateUserRow(string userName, IDictionary<string, object> values)
        {
            string userTableName = "Users";
            List<BsonElement> elements = new List<BsonElement>();
            elements.Add(new BsonElement("UserName", userName));
            elements.Add(new BsonElement("_id", Guid.NewGuid()));
            if (values != null)
            {
                foreach (var item in values)
                {
                    elements.Add(new BsonElement(item.Key, item.Value as BsonValue));
                }
            }

            var collection = _provider.GetCollection(userTableName);
            var result = collection.Insert(new BsonDocument(elements.ToArray()));
            return result.LastErrorMessage == null;
        }
    }

}
