using System;
using System.Linq;
using Xunit;
using Dook.Tests.Models;
using System.Data;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dook.Tests
{
    public class QueryProviderTests
    {
        /// <summary>
        /// DeleteWhere tests
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> GetDeleteWhereTestsData()
        {
            Expression<Func<TestModel,bool>> e1 = t => t.StringProperty.Contains("Test");
            yield return new object[]
            {
                e1,
                "DELETE FROM TestModels as t WHERE t.[StringProperty] LIKE @P0;"
            };
            Expression<Func<TestModel,bool>> e2 = t => t.Id > 2 && t.CreatedOn < new DateTime(2019,05,05);
            yield return new object[]
            {
                e2,
                "DELETE FROM TestModels as t WHERE ((t.[Id] > @P0) AND (t.[CreatedOn] < @P1));"
            };
        }
        [Theory, MemberData("GetDeleteWhereTestsData")]
        public void GetDeleteWhereTest(Expression<Func<TestModel,bool>> expression, string expectedResult)
        {
            MySQLTranslator translator = new MySQLTranslator();
            QueryProvider provider = new QueryProvider(new DbProvider(DbType.Sql, "Server=127.0.0.1;Database=fakedb;User Id=FakeUser;Password=fake.password;"));
            IDbCommand cmd = provider.GetDeleteWhereCommand<TestModel>(expression, "TestModels", Mapper.GetTableMapping<TestModel>());
            Assert.Equal(expectedResult, cmd.CommandText);
        } 

        /// <summary>
        /// DeleteAll tests
        /// </summary>
        [Fact]
        public void GetDeleteAllTest()
        {
            MySQLTranslator translator = new MySQLTranslator();
            QueryProvider provider = new QueryProvider(new DbProvider(DbType.Sql, "Server=127.0.0.1;Database=fakedb;User Id=FakeUser;Password=fake.password;"));
            IDbCommand cmd = provider.GetDeleteAllCommand("TestModels", Mapper.GetTableMapping<TestModel>());
            Assert.Equal("DELETE FROM TestModels;", cmd.CommandText);
        } 

        /// <summary>
        /// GetDeleteCommand Test 
        /// </summary>
        /// <param name="model"></param>
        [Fact]
        public void GetDeleteCommandTest()
        {
            TestModel model = new TestModel{ Id = 1 };
            MySQLTranslator translator = new MySQLTranslator();
            QueryProvider provider = new QueryProvider(new DbProvider(DbType.Sql, "Server=127.0.0.1;Database=fakedb;User Id=FakeUser;Password=fake.password;"));
            IDbCommand cmd = provider.GetDeleteCommand(model.Id, "TestModels", Mapper.GetTableMapping<TestModel>());
            Assert.Equal("DELETE FROM TestModels WHERE Id = @id;", cmd.CommandText);
        }

        public static IEnumerable<object[]> GetUpdateCommandTestsData()
        {
            List<string> properties = new List<string>();
            Dictionary<string, string> TableMapping = Mapper.GetTableMapping<TestModel>();
            foreach (string attributeName in TableMapping.Keys)
            {
                if (attributeName == "Id" || attributeName == "CreatedOn") continue;
                properties.Add($"{TableMapping[attributeName]} = @{attributeName}");
            }
            yield return new object[]
            {
                new TestModel{
                    CreatedOn = new DateTime(2019,05,07),
                    UpdatedOn = new DateTime(2019,05,07),
                    Id = 1,
                    BoolProperty = true,
                    DateTimeProperty = new DateTime(2019,05,07),
                    StringProperty = "test",
                    EnumProperty = TestEnum.One
                },
                $"UPDATE TestModels SET {String.Join(", ", properties)} WHERE Id = @id;"
            };
            yield return new object[]
            {
                new TestModel{
                    CreatedOn = new DateTime(2019,05,07),
                    UpdatedOn = new DateTime(2019,05,07),
                    Id = 0,
                    BoolProperty = true,
                    DateTimeProperty = new DateTime(2019,05,07),
                    StringProperty = "test",
                    EnumProperty = TestEnum.One
                },
                "Id property must be a positive integer."
            };
        }
        [Theory, MemberData("GetUpdateCommandTestsData")]
        public void GetUpdateCommandTests(TestModel model, string expectedResult)
        {
            MySQLTranslator translator = new MySQLTranslator();
            QueryProvider provider = new QueryProvider(new DbProvider(DbType.Sql, "Server=127.0.0.1;Database=fakedb;User Id=FakeUser;Password=fake.password;"));
            string result;
            try
            {
                IDbCommand cmd = provider.GetUpdateCommand(model, "TestModels", Mapper.GetTableMapping<TestModel>());
                result = cmd.CommandText;
            }
            catch (Exception e)
            {
                result = e.Message;
            }
            Assert.Equal(expectedResult, result);
        }
    }
}
