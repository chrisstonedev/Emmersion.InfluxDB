using System;
using NUnit.Framework;

namespace EL.InfluxDB.UnitTests
{
    internal class AssembleLineProtocolTests : With_an_automocked<AssembleLineProtocol>
    {
        [Test]
        public void should_comma_separate_fields()
        {
            var timestamp = DateTimeOffset.UtcNow;
            var point = new InfluxPoint("test_measurement", new[] {new Field("field1", value: 1), new Field("field2", value: 2)}, timestamp);
            var result = AssembleLineProtocol.Assemble(point);

            var expectedTimestamp = timestamp.ToUnixTimeNanoseconds().ToString();
            Assert.That(result, Is.EqualTo($"test_measurement field1=1,field2=2 {expectedTimestamp}"));
        }

        [Test]
        public void should_escape_commas_spaces_equals_in_field_key()
        {
            var timestamp = DateTimeOffset.UtcNow;
            var point = new InfluxPoint("test_measurement", new[] {new Field("f,i e=ld1", value: 1)}, timestamp);
            var result = AssembleLineProtocol.Assemble(point);

            var expectedTimestamp = timestamp.ToUnixTimeNanoseconds().ToString();
            Assert.That(result, Is.EqualTo($@"test_measurement f\,i\ e\=ld1=1 {expectedTimestamp}"));
        }

        [Test]
        public void should_escape_commas_spaces_equals_in_tag_keys_and_tag_values()
        {
            var timestamp = DateTimeOffset.UtcNow;
            var point = new InfluxPoint("test_measurement", new[] {new Field("field1", value: 1)}, new[] {new Tag("ta,g 1=", "app,l e=s")}, timestamp);
            var result = AssembleLineProtocol.Assemble(point);

            var expectedTimestamp = timestamp.ToUnixTimeNanoseconds().ToString();
            Assert.That(result, Is.EqualTo($@"test_measurement,ta\,g\ 1\==app\,l\ e\=s field1=1 {expectedTimestamp}"));
        }

        [Test]
        public void should_escape_commas_spaces_in_measurement_names()
        {
            var timestamp = DateTimeOffset.UtcNow;
            var point = new InfluxPoint("test measure,ment", new[] {new Field("field1", value: 1)}, tags: null, timestamp);
            var result = AssembleLineProtocol.Assemble(point);

            var expectedTimestamp = timestamp.ToUnixTimeNanoseconds().ToString();
            Assert.That(result, Is.EqualTo($@"test\ measure\,ment field1=1 {expectedTimestamp}"));
        }

        [Test]
        public void should_escape_doublequotes_in_field_value()
        {
            var timestamp = DateTimeOffset.UtcNow;
            var point = new InfluxPoint("test_measurement", new[] {new Field("field1", @"some ""quoted"" text")}, timestamp);
            var result = AssembleLineProtocol.Assemble(point);

            var expectedTimestamp = timestamp.ToUnixTimeNanoseconds().ToString();
            Assert.That(result, Is.EqualTo($@"test_measurement field1=""some \""quoted\"" text"" {expectedTimestamp}"));
        }

        [Test]
        public void should_format_measurement_name_then_fields_then_timestamp()
        {
            var timestamp = DateTimeOffset.UtcNow;
            var point = new InfluxPoint("test_measurement", new[] {new Field("field1", value: 1)}, timestamp);
            var result = AssembleLineProtocol.Assemble(point);

            var expectedTimestamp = timestamp.ToUnixTimeNanoseconds().ToString();
            Assert.That(result, Is.EqualTo($"test_measurement field1=1 {expectedTimestamp}"));
        }

        [Test]
        public void should_format_tags_after_measurement_name()
        {
            var timestamp = DateTimeOffset.UtcNow;
            var fields = new[] {new Field("field1", value: 1)};
            var tags = new[] {new Tag("tag1", "apples"), new Tag("tag2", "oranges")};
            var point = new InfluxPoint("test_measurement", fields, tags, timestamp);
            var result = AssembleLineProtocol.Assemble(point);

            var expectedTimestamp = timestamp.ToUnixTimeNanoseconds().ToString();
            Assert.That(result, Is.EqualTo($"test_measurement,tag1=apples,tag2=oranges field1=1 {expectedTimestamp}"));
        }

        [Test]
        public void should_not_quote_field_values_if_they_are_booleans()
        {
            var timestamp = DateTimeOffset.UtcNow;
            var point = new InfluxPoint("test_measurement", new[] {new Field("field1", value: true), new Field("field2", value: false)}, timestamp);

            var result = AssembleLineProtocol.Assemble(point);

            var expectedTimestamp = timestamp.ToUnixTimeNanoseconds().ToString();
            Assert.That(result, Is.EqualTo($@"test_measurement field1=True,field2=False {expectedTimestamp}"));
        }

        [Test]
        public void should_not_quote_field_values_if_they_are_numbers()
        {
            var timestamp = DateTimeOffset.UtcNow;
            var point = new InfluxPoint("test_measurement", new[] {new Field("field1", value: 1L), new Field("field2", value: 32.05m)}, timestamp);

            var result = AssembleLineProtocol.Assemble(point);

            var expectedTimestamp = timestamp.ToUnixTimeNanoseconds().ToString();
            Assert.That(result, Is.EqualTo($@"test_measurement field1=1,field2=32.05 {expectedTimestamp}"));
        }

        [Test]
        public void should_quote_field_values_if_they_are_strings()
        {
            var timestamp = DateTimeOffset.UtcNow;
            var point = new InfluxPoint("test_measurement", new[] {new Field("field1", value: 1), new Field("field2", "foo")}, timestamp);
            var result = AssembleLineProtocol.Assemble(point);

            var expectedTimestamp = timestamp.ToUnixTimeNanoseconds().ToString();
            Assert.That(result, Is.EqualTo($@"test_measurement field1=1,field2=""foo"" {expectedTimestamp}"));
        }

        [Test]
        public void should_treat_all_other_data_types_as_strings()
        {
            var timestamp = DateTimeOffset.UtcNow;
            var point = new InfluxPoint("test_measurement",
                new[] {new Field("field1", new DateTime(year: 2018, month: 12, day: 30)), new Field("field2", Guid.Empty), new Field("field3", new object())},
                timestamp);

            var result = AssembleLineProtocol.Assemble(point);

            var expectedTimestamp = timestamp.ToUnixTimeNanoseconds().ToString();
            Assert.That(result, Is.EqualTo($@"test_measurement field1=""{new DateTime(year: 2018, month: 12, day: 30)}"",field2=""{Guid.Empty}"",field3=""{new object()}"" {expectedTimestamp}"));
        }

        [Test]
        public void should_use_default_timestamp_when_not_provided()
        {
            var before = DateTimeOffset.UtcNow;
            var point = new InfluxPoint("test_measurement", new[] {new Field("field1", value: 1)});
            var result = AssembleLineProtocol.Assemble(point);
            var after = DateTimeOffset.UtcNow;

            Assert.That(point.Timestamp, Is.GreaterThanOrEqualTo(before));
            Assert.That(point.Timestamp, Is.LessThanOrEqualTo(after));

            var expectedTimestamp = point.Timestamp.ToUnixTimeNanoseconds().ToString();
            Assert.That(result, Is.EqualTo($"test_measurement field1=1 {expectedTimestamp}"));
        }
    }
}