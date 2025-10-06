// Tests/DataValidationTests.cs
using CoreLib.Models;
namespace CoreLib.Tests;
[TestFixture]
    public class DataValidationTests
    {
        private Table _testTable;

        [SetUp]
        public void Setup()
        {
            _testTable = new Table("ValidationTest");
            _testTable.AddColumn(new Column("IntCol", DataType.Integer));
            _testTable.AddColumn(new Column("RealCol", DataType.Real));
            _testTable.AddColumn(new Column("CharCol", DataType.Char));
            _testTable.AddColumn(new Column("StringCol", DataType.String));
            _testTable.AddColumn(new Column("IntervalCol", DataType.IntegerInterval));
        }

        [Test]
        public void ValidateInteger_WithValidValue_ShouldPass()
        {
            // Arrange
            var rowData = new Dictionary<string, object?>
            {
                ["IntCol"] = 42,
                ["RealCol"] = 3.14,
                ["CharCol"] = 'A',
                ["StringCol"] = "test",
                ["IntervalCol"] = new IntegerInterval(1, 10)
            };

            // Act
            var result = _testTable.ValidateRow(rowData);

            // Assert
            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void ValidateInteger_WithInvalidValue_ShouldFail()
        {
            // Arrange
            var rowData = new Dictionary<string, object?>
            {
                ["IntCol"] = "not_an_integer",
                ["RealCol"] = 3.14,
                ["CharCol"] = 'A',
                ["StringCol"] = "test",
                ["IntervalCol"] = new IntegerInterval(1, 10)
            };

            // Act
            var result = _testTable.ValidateRow(rowData);

            // Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Count > 0, Is.True);
        }

        [Test]
        public void ValidateChar_WithMultipleCharacters_ShouldFail()
        {
            // Arrange
            var rowData = new Dictionary<string, object?>
            {
                ["IntCol"] = 42,
                ["RealCol"] = 3.14,
                ["CharCol"] = "AB",
                ["StringCol"] = "test",
                ["IntervalCol"] = new IntegerInterval(1, 10)
            };

            // Act
            var result = _testTable.ValidateRow(rowData);

            // Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Count > 0, Is.True);
        }
    }