using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

using FluentAssertions;

using Xunit;

namespace Cosmos.Abstracts.Tests;

public class ObjectIdTests
{
    private static readonly DateTime __unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(0x01020304, 0x0000000506070809, 0x0a0b0c, 0x01020304, 0x05060708, 0x090a0b0c)]
    [InlineData(0xf1f2f3f4, 0x000000f5f6f7f8f9, 0xfafbfc, 0xf1f2f3f4, 0xf5f6f7f8, 0xf9fafbfc)]
    public void Create_should_generate_expected_a_b_c(uint timestamp, long random, uint increment, uint expectedA, uint expectedB, uint expectedC)
    {
        var objectId = ObjectIdReflector.Create((int)timestamp, random, (int)increment);
        objectId._a().Should().Be((int)expectedA);
        objectId._b().Should().Be((int)expectedB);
        objectId._c().Should().Be((int)expectedC);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0xffffffffff, 0xffffff)]
    public void Create_should_not_throw_when_arguments_are_valid(long random, int increment)
    {
        var _ = ObjectIdReflector.Create(1, random, increment);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0x10000000000)]
    public void Create_should_throw_when_random_is_out_of_range(long random)
    {
        var exception = Record.Exception(() => ObjectIdReflector.Create(1, random, 1));
        var e = exception.Should().BeOfType<ArgumentOutOfRangeException>().Subject;
        e.ParamName.Should().Be("random");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0x1000000)]
    public void Create_should_throw_when_increment_is_out_of_range(int increment)
    {
        var exception = Record.Exception(() => ObjectIdReflector.Create(1, 1, increment));
        var e = exception.Should().BeOfType<ArgumentOutOfRangeException>().Subject;
        e.ParamName.Should().Be("increment");
    }

    [Theory]
    [InlineData(0x00000000, "1970-01-01T00:00:00Z")]
    [InlineData(0x7FFFFFFF, "2038-01-19T03:14:07Z")]
    [InlineData(0x80000000, "2038-01-19T03:14:08Z")]
    [InlineData(0xFFFFFFFF, "2106-02-07T06:28:15Z")]
    public void Ensure_that_timestamp_is_interpreted_as_unsigned_int(uint timestamp, string expectedDateString)
    {
        var objectId = ObjectId.GenerateNewId((int)timestamp);
        var expectedDate = DateTime.Parse(expectedDateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        objectId.CreationTime.Should().Be(expectedDate);
    }

    [Fact]
    public void TestByteArrayConstructor()
    {
        byte[] bytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var objectId = new ObjectId(bytes);
        Assert.Equal(0x01020304, objectId.Timestamp);
        Assert.Equal(0x01020304, objectId._a());
        Assert.Equal(0x05060708, objectId._b());
        Assert.Equal(0x090a0b0c, objectId._c());
        Assert.Equal(__unixEpoch.AddSeconds(0x01020304), objectId.CreationTime);
        Assert.Equal("0102030405060708090a0b0c", objectId.ToString());
        Assert.True(bytes.SequenceEqual(objectId.ToByteArray()));
    }


    [Fact]
    public void TestStringConstructor()
    {
        byte[] bytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var objectId = new ObjectId("0102030405060708090a0b0c");
        Assert.Equal(0x01020304, objectId.Timestamp);
        Assert.Equal(0x01020304, objectId._a());
        Assert.Equal(0x05060708, objectId._b());
        Assert.Equal(0x090a0b0c, objectId._c());
        Assert.Equal(__unixEpoch.AddSeconds(0x01020304), objectId.CreationTime);
        Assert.Equal("0102030405060708090a0b0c", objectId.ToString());
        Assert.True(bytes.SequenceEqual(objectId.ToByteArray()));
    }

    [Fact]
    public void TestGenerateNewId()
    {
        // compare against two timestamps in case seconds since epoch changes in middle of test
        var timestamp1 = (int)Math.Floor((DateTime.UtcNow - __unixEpoch).TotalSeconds);
        var objectId = ObjectId.GenerateNewId();
        var timestamp2 = (int)Math.Floor((DateTime.UtcNow - __unixEpoch).TotalSeconds);
        Assert.True(objectId.Timestamp == timestamp1 || objectId.Timestamp == timestamp2);
        Assert.NotEqual(0, objectId._a());
        Assert.NotEqual(0, objectId._b());
        Assert.NotEqual(0, objectId._c());
    }

    [Fact]
    public void TestGenerateNewIdWithDateTime()
    {
        var timestamp = new DateTime(2011, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var objectId = ObjectId.GenerateNewId(timestamp);
        Assert.True(objectId.CreationTime == timestamp);
        Assert.NotEqual(0, objectId._a());
        Assert.NotEqual(0, objectId._b());
        Assert.NotEqual(0, objectId._c());
    }

    [Fact]
    public void TestGenerateNewIdWithTimestamp()
    {
        var timestamp = 0x01020304;
        var objectId = ObjectId.GenerateNewId(timestamp);
        Assert.True(objectId.Timestamp == timestamp);
        Assert.NotEqual(0, objectId._a());
        Assert.NotEqual(0, objectId._b());
        Assert.NotEqual(0, objectId._c());
    }

    [Fact]
    public void TestIComparable()
    {
        var objectId1 = ObjectId.GenerateNewId();
        var objectId2 = ObjectId.GenerateNewId();
        Assert.Equal(0, objectId1.CompareTo(objectId1));
        Assert.Equal(-1, objectId1.CompareTo(objectId2));
        Assert.Equal(1, objectId2.CompareTo(objectId1));
        Assert.Equal(0, objectId2.CompareTo(objectId2));
    }

    [Fact]
    public void TestCompareEqualGeneratedIds()
    {
        var objectId1 = ObjectId.GenerateNewId();
        var objectId2 = objectId1;
        Assert.False(objectId1 < objectId2);
        Assert.True(objectId1 <= objectId2);
        Assert.False(objectId1 != objectId2);
        Assert.True(objectId1 == objectId2);
        Assert.False(objectId1 > objectId2);
        Assert.True(objectId1 >= objectId2);
    }

    [Fact]
    public void TestCompareSmallerTimestamp()
    {
        var objectId1 = new ObjectId("0102030405060708090a0b0c");
        var objectId2 = new ObjectId("0102030505060708090a0b0c");
        Assert.True(objectId1 < objectId2);
        Assert.True(objectId1 <= objectId2);
        Assert.True(objectId1 != objectId2);
        Assert.False(objectId1 == objectId2);
        Assert.False(objectId1 > objectId2);
        Assert.False(objectId1 >= objectId2);
    }

    [Fact]
    public void TestCompareSmallerMachine()
    {
        var objectId1 = new ObjectId("0102030405060708090a0b0c");
        var objectId2 = new ObjectId("0102030405060808090a0b0c");
        Assert.True(objectId1 < objectId2);
        Assert.True(objectId1 <= objectId2);
        Assert.True(objectId1 != objectId2);
        Assert.False(objectId1 == objectId2);
        Assert.False(objectId1 > objectId2);
        Assert.False(objectId1 >= objectId2);
    }

    [Fact]
    public void TestCompareSmallerPid()
    {
        var objectId1 = new ObjectId("0102030405060708090a0b0c");
        var objectId2 = new ObjectId("01020304050607080a0a0b0c");
        Assert.True(objectId1 < objectId2);
        Assert.True(objectId1 <= objectId2);
        Assert.True(objectId1 != objectId2);
        Assert.False(objectId1 == objectId2);
        Assert.False(objectId1 > objectId2);
        Assert.False(objectId1 >= objectId2);
    }

    [Fact]
    public void TestCompareSmallerIncrement()
    {
        var objectId1 = new ObjectId("0102030405060708090a0b0c");
        var objectId2 = new ObjectId("0102030405060708090a0b0d");
        Assert.True(objectId1 < objectId2);
        Assert.True(objectId1 <= objectId2);
        Assert.True(objectId1 != objectId2);
        Assert.False(objectId1 == objectId2);
        Assert.False(objectId1 > objectId2);
        Assert.False(objectId1 >= objectId2);
    }

    [Fact]
    public void TestCompareSmallerGeneratedId()
    {
        var objectId1 = ObjectId.GenerateNewId();
        var objectId2 = ObjectId.GenerateNewId();
        Assert.True(objectId1 < objectId2);
        Assert.True(objectId1 <= objectId2);
        Assert.True(objectId1 != objectId2);
        Assert.False(objectId1 == objectId2);
        Assert.False(objectId1 > objectId2);
        Assert.False(objectId1 >= objectId2);
    }

    [Fact]
    public void TestCompareLargerTimestamp()
    {
        var objectId1 = new ObjectId("0102030405060708090a0b0c");
        var objectId2 = new ObjectId("0102030305060708090a0b0c");
        Assert.False(objectId1 < objectId2);
        Assert.False(objectId1 <= objectId2);
        Assert.True(objectId1 != objectId2);
        Assert.False(objectId1 == objectId2);
        Assert.True(objectId1 > objectId2);
        Assert.True(objectId1 >= objectId2);
    }

    [Fact]
    public void TestCompareLargerMachine()
    {
        var objectId1 = new ObjectId("0102030405060808090a0b0c");
        var objectId2 = new ObjectId("0102030405060708090a0b0c");
        Assert.False(objectId1 < objectId2);
        Assert.False(objectId1 <= objectId2);
        Assert.True(objectId1 != objectId2);
        Assert.False(objectId1 == objectId2);
        Assert.True(objectId1 > objectId2);
        Assert.True(objectId1 >= objectId2);
    }

    [Fact]
    public void TestCompareLargerPid()
    {
        var objectId1 = new ObjectId("01020304050607080a0a0b0c");
        var objectId2 = new ObjectId("0102030405060708090a0b0c");
        Assert.False(objectId1 < objectId2);
        Assert.False(objectId1 <= objectId2);
        Assert.True(objectId1 != objectId2);
        Assert.False(objectId1 == objectId2);
        Assert.True(objectId1 > objectId2);
        Assert.True(objectId1 >= objectId2);
    }

    [Fact]
    public void TestCompareLargerIncrement()
    {
        var objectId1 = new ObjectId("0102030405060708090a0b0d");
        var objectId2 = new ObjectId("0102030405060708090a0b0c");
        Assert.False(objectId1 < objectId2);
        Assert.False(objectId1 <= objectId2);
        Assert.True(objectId1 != objectId2);
        Assert.False(objectId1 == objectId2);
        Assert.True(objectId1 > objectId2);
        Assert.True(objectId1 >= objectId2);
    }

    [Fact]
    public void TestCompareLargerGeneratedId()
    {
        var objectId2 = ObjectId.GenerateNewId(); // generate before objectId2
        var objectId1 = ObjectId.GenerateNewId();
        Assert.False(objectId1 < objectId2);
        Assert.False(objectId1 <= objectId2);
        Assert.True(objectId1 != objectId2);
        Assert.False(objectId1 == objectId2);
        Assert.True(objectId1 > objectId2);
        Assert.True(objectId1 >= objectId2);
    }

    [Fact]
    public void TestParse()
    {
        var objectId1 = ObjectId.Parse("0102030405060708090a0b0c"); // lower case
        var objectId2 = ObjectId.Parse("0102030405060708090A0B0C"); // upper case
        Assert.True(objectId1.ToByteArray().SequenceEqual(objectId2.ToByteArray()));
        Assert.True(objectId1.ToString() == "0102030405060708090a0b0c"); // ToString returns lower case
        Assert.True(objectId1.ToString() == objectId2.ToString());
        Assert.Throws<FormatException>(() => ObjectId.Parse("102030405060708090a0b0c")); // too short
        Assert.Throws<FormatException>(() => ObjectId.Parse("x102030405060708090a0b0c")); // invalid character
        Assert.Throws<FormatException>(() => ObjectId.Parse("00102030405060708090a0b0c")); // too long
    }

    [Fact]
    public void TestTryParse()
    {
        ObjectId objectId1, objectId2;
        Assert.True(ObjectId.TryParse("0102030405060708090a0b0c", out objectId1)); // lower case
        Assert.True(ObjectId.TryParse("0102030405060708090A0B0C", out objectId2)); // upper case
        Assert.True(objectId1.ToByteArray().SequenceEqual(objectId2.ToByteArray()));
        Assert.True(objectId1.ToString() == "0102030405060708090a0b0c"); // ToString returns lower case
        Assert.True(objectId1.ToString() == objectId2.ToString());
        Assert.False(ObjectId.TryParse("102030405060708090a0b0c", out objectId1)); // too short
        Assert.False(ObjectId.TryParse("x102030405060708090a0b0c", out objectId1)); // invalid character
        Assert.False(ObjectId.TryParse("00102030405060708090a0b0c", out objectId1)); // too long
        Assert.False(ObjectId.TryParse(null, out objectId1)); // should return false not throw ArgumentNullException
    }

    [Fact]
    public void TestConvertObjectIdToObjectId()
    {
        var oid = ObjectId.GenerateNewId();

        var oidConverted = Convert.ChangeType(oid, typeof(ObjectId));

        Assert.Equal(oid, oidConverted);
    }
}

internal static class ObjectIdReflector
{
    public static int _a(this ObjectId obj)
    {
        return (int)GetFieldValue(obj, nameof(_a));
    }

    public static int _b(this ObjectId obj)
    {
        return (int)GetFieldValue(obj, nameof(_b));
    }

    public static int _c(this ObjectId obj)
    {
        return (int)GetFieldValue(obj, nameof(_c));
    }

    public static ObjectId Create(int timestamp, long random, int increment)
    {
        return (ObjectId)InvokeStatic(typeof(ObjectId), nameof(Create), timestamp, random, increment);
    }

    public static void __staticIncrement(int value)
    {
        SetStaticFieldValue(typeof(ObjectId), nameof(__staticIncrement), value);
    }


    private static void SetStaticFieldValue(Type type, string name, int value)
    {
        var fieldInfo = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
        fieldInfo.SetValue(null, value);
    }

    private static object InvokeStatic<T1, T2, T3>(Type type, string name, T1 arg1, T2 arg2, T3 arg3)
    {
        var parameterTypes = new[] { typeof(T1), typeof(T2), typeof(T3) };

        var methodInfo = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(m => m.Name == name && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes))
            .Single();

        try
        {
            return methodInfo.Invoke(null, new object[] { arg1, arg2, arg3 });
        }
        catch (TargetInvocationException exception)
        {
            throw exception.InnerException;
        }
    }

    private static object GetFieldValue(ObjectId obj, string name)
    {
        var field = obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        return field.GetValue(obj);
    }
}
