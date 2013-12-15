using Moq;
using System.Linq;
using Xunit;

namespace ExtendedMongoMembership.Tests
{
    public class SimpleMongoProviderTests
    {
        private TestSimpleMembershipProvider _simpleMembershipProvider;
        private Mock<MockDatabase> _database;

        public SimpleMongoProviderTests()
        {
            _database = new Mock<MockDatabase>(MockBehavior.Strict);

            _database.Setup(x => x.Dispose()).Callback(() => { });
            _simpleMembershipProvider = new TestSimpleMembershipProvider(_database.Object);

        }

        [Fact]
        public void ConfirmAccountReturnsFalseIfNoRecordExistsForToken()
        {
            // Arrange
            _database.Setup(d => d.Users)
                .Returns(Enumerable.Empty<MembershipAccount>().AsQueryable());
            _database.Setup(x => x.Update<MembershipAccount>(null))
    .Callback(() => { });
            _database.Setup(x => x.Save<MembershipAccount>(null))
                .Callback(() => { });

            // Act
            bool result = _simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConfirmAccountReturnsFalseIfConfirmationTokenDoesNotMatchInCase()
        {
            // Arrange
            MembershipAccount record = new MembershipAccount
            {
                UserId = 98,
                ConfirmationToken = "Foo"
            };

            _database.Setup(d => d.Users)
                .Returns(new MembershipAccount[] { record }.AsQueryable());
            _database.Setup(x => x.Update<MembershipAccount>(null))
    .Callback(() => { });

            // Act
            bool result = _simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConfirmAccountReturnsFalseIfNoConfirmationTokenFromMultipleListMatchesInCase()
        {
            // Arrange
            MembershipAccount recordA = new MembershipAccount
            {
                UserId = 98,
                ConfirmationToken = "Foo"
            };
            MembershipAccount recordB = new MembershipAccount
            {
                UserId = 99,
                ConfirmationToken = "fOo"
            };

            _database.Setup(d => d.Users)
                .Returns(new MembershipAccount[] { recordA, recordB }.AsQueryable());
            _database.Setup(x => x.Update<MembershipAccount>(null))
    .Callback(() => { });

            // Act
            bool result = _simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConfirmAccountUpdatesIsConfirmedFieldIfConfirmationTokenMatches()
        {
            // Arrange
            MembershipAccount record = new MembershipAccount
            {
                UserId = 98,
                ConfirmationToken = "foo"
            };
            _database.Setup(d => d.Users)
                .Returns(new MembershipAccount[] { record }.AsQueryable())
                .Verifiable();
            _database.Setup(x => x.Update<MembershipAccount>(record))
            .Callback(() => { })
            .Verifiable();

            // Act
            bool result = _simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.True(result);
            _database.Verify();
        }

        [Fact]
        public void ConfirmAccountUpdatesIsConfirmedFieldIfAnyOneOfReturnRecordConfirmationTokenMatches()
        {
            // Arrange
            MembershipAccount recordA = new MembershipAccount
            {
                UserId = 100,
                ConfirmationToken = "Foo"
            };
            MembershipAccount recordB = new MembershipAccount
            {
                UserId = 101,
                ConfirmationToken = "foo"
            };
            MembershipAccount recordC = new MembershipAccount
            {
                UserId = 102,
                ConfirmationToken = "fOo"
            };

            _database.Setup(d => d.Users)
                .Returns(new MembershipAccount[] { recordA, recordB, recordC }.AsQueryable())
                .Verifiable();
            _database.Setup(x => x.Update<MembershipAccount>(recordB))
            .Callback(() => { })
            .Verifiable();

            // Act
            bool result = _simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.True(result);
            _database.Verify();
        }

        [Fact]
        public void ConfirmAccountWithUserNameReturnsFalseIfNoRecordExistsForToken()
        {
            // Arrange
            _database.Setup(d => d.Users)
                .Returns(Enumerable.Empty<MembershipAccount>().AsQueryable());

            // Act
            bool result = _simpleMembershipProvider.ConfirmAccount("user12", "foo");

            // Assert
            Assert.False(result);
        }

        //[Fact]
        //public void GenerateTokenHtmlEncodesValues()
        //{
        //    // Arrange
        //    var generator = new Mock<RandomNumberGenerator>(MockBehavior.Strict);
        //    var generatedBytes = Encoding.Default.GetBytes("|aÿx§#½oÿ↨îA8Eµ");
        //    generator.Setup(g => g.GetBytes(It.IsAny<byte[]>())).Callback((byte[] array) => Array.Copy(generatedBytes, array, generatedBytes.Length));

        //    // Act
        //    var result = TestSimpleMembershipProvider.GenerateToken(generator.Object);

        //    // Assert
        //    Assert.Equal("fGH/eKcjvW//P+5BOEW1", Convert.ToBase64String(generatedBytes));
        //    Assert.Equal("fGH_eKcjvW__P-5BOEW1AA2", result);
        //}

        private class TestSimpleMembershipProvider : MongoMembershipProvider
        {
            private readonly ISession _database;

            public TestSimpleMembershipProvider(ISession database)
            {
                _database = database;
            }

            protected override ISession ConnectToDatabase(string connectionString)
            {
                return _database;
            }

            protected override void VerifyInitialized()
            {
            }
        }
    }
}
