using System;
using System.Threading.Tasks;
using NUnit.Framework;
using BasicCloudApi;

namespace BasicCloudTests
{
    [TestFixture]
    public class HelperTests{
        [TestCase("homes/test/something", "homes/test", TestName = "Test_Valid")]
        [TestCase("shared", "", TestName = "Test_Root")]
        public void TestGetParentDir(string testDirectory, string expectedResult)
        {
            string actualResult = Helpers.GetParentDir(testDirectory);
            Assert.That(expectedResult == actualResult);
        }
    }
    [TestFixture]
    public class CommunicationTests
    {
        private string apiUrl;
        private Utils.UserCreds validUserCreds;
        private Communication apiCommunication;
        [SetUp]
        public void Setup()
        {
            apiUrl = TestContext.Parameters["TEST_API_URL"];
            string username = TestContext.Parameters["TEST_USERNAME"];
            string password = TestContext.Parameters["TEST_PASSWORD"];
            string token = TestContext.Parameters["TEST_TOKEN"];

            if(string.IsNullOrEmpty(apiUrl))
            {
                throw new Exception("missing 'TEST_API_URL' parameter");
            }
            else if (string.IsNullOrEmpty(username))
            {
                throw new Exception("missing 'TEST_USERNAME' parameter");
            }
            else if (string.IsNullOrEmpty(password))
            {
                throw new Exception("missing 'TEST_PASSWORD' parameter");
            }
            else if (string.IsNullOrEmpty(token))
            {
                throw new Exception("missing 'TEST_TOKEN' parameter");
            }

            validUserCreds = new(
                username,
                password,
                new BasicCloudApi.Types.Token(token, "bearer")
            );

            apiCommunication = new(apiUrl, validUserCreds.Token);
        }
        [Test]
        public void TestGetVersion()
        {
            Assert.DoesNotThrowAsync(async () => await apiCommunication.GetApiVersion());
        }
        [Test]
        public void TestPostCreateAccount()
        {
            string username = "somerandomuser";
            Assert.DoesNotThrowAsync(async () => await apiCommunication.PostCreateAccount(username, username));
        }
        [Test]
        public void TestPostLogin()
        {
            Assert.DoesNotThrowAsync(async () => await apiCommunication.PostLoginToken(
                validUserCreds.Username, validUserCreds.Password, false
            ));
        }
        [Test]
        public async Task TestGetRoots()
        {
            var roots = await apiCommunication.GetDirectoryRoots();
            Assert.That(roots.shared == "shared");
            Assert.That(roots.home == "homes/" + validUserCreds.Username);
        }
    }
}
