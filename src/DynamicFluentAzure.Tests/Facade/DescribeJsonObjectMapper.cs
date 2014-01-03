using System;
using DynamicFluentAzure.Tests.Helpers;
using FluentAssertions;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DynamicFluentAzure.Tests.Facade
{
    [TestFixture]
    public class DescribeJsonObjectMapper
    {
        [Test]
        public void ItCanMapFromDynamicTableEntity()
        {
            // g
            const string valueString = "something";
            var aTimestamp = DateTimeOffset.UtcNow;
            var ce = new DynamicTableEntity {ETag = valueString, Timestamp = aTimestamp};
            ce.Properties.Add("id", new EntityProperty(valueString));
            ce.Properties.Add("name", new EntityProperty(valueString));

            var expected = JsonObjectFactory.CreateJsonObjectWithTimestamp(aTimestamp);

            // w
            var json = ce.ToJsonObject();

            // t
            json.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void ItCanMapFromJsonObject()
        {
            // g 
            var json = JsonObjectFactory.CreateJsonObjectForPost();

            // w
            var actual = json.ToDynamicEntity();

            // t
            actual.PartitionKey.Should().Be(json["PartitionKey"].ToString());
        }

        [Test]
        public void ItComplains_WhenMappingFromJsonObject_GivenInvalidJsonObject()
        {
            // g 

            // w
            Action act = (() => JsonObjectMapper.ToDynamicEntity(null));

            // t
            act.ShouldThrow<ArgumentNullException>();
        }


        [Test]
        public void ItShouldFlattenArrays()
        {
            // g 
            var expected = JsonConvert.SerializeObject(new object[] {"1", "2", "3"});
            var json = JsonObjectFactory.CreateJsonObjectForPostWithArray();

            // w
            var actual = json.ToDynamicEntity();

            // t
            actual.Properties["dragon_ids"].StringValue.Should().Be(expected);
        }

        [Test]
        public void ItShouldInflateArrays()
        {
            // g
            var expected = new object[] {"1", "2", "3"};
            var expectedString = JsonConvert.SerializeObject(expected);

            var ce = new DynamicTableEntity();
            ce.Properties.Add("dragon_ids", new EntityProperty(expectedString));

            // w
            var json = ce.ToJsonObject();

            // t
            json["dragon_ids"].ShouldBeEquivalentTo(expected);
        }
    }
}