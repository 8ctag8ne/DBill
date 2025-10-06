// Tests/TableOperationstests.cs
using CoreLib.Models;
using CoreLib.Services;

namespace CoreLib.Tests
{
    [TestFixture]
    public class TableOperationsTests
    {
        private DatabaseService _databaseService;
        private InMemoryStorageService _storageService;
        private InMemoryStorageService _tempStorageService;
        private string _testTableName;

        [SetUp]
        public void Setup()
        {
            _storageService = new InMemoryStorageService();
            _tempStorageService = new InMemoryStorageService();
            
            // Створюємо mock FileService (просту реалізацію, яка нічого не робить)
            var fileService = CreateMockFileService();
            var tempFileService = CreateMockFileService();
            
            _databaseService = new DatabaseService(_storageService, _tempStorageService, fileService, tempFileService);
            _databaseService.CreateDatabase("TestDB");
            
            _testTableName = "TestTable";
            
            // Створюємо тестову таблицю через DatabaseService
            var columns = new List<Column>
            {
                new Column("ID", DataType.Integer),
                new Column("Name", DataType.String),
                new Column("Age", DataType.Integer),
                new Column("Salary", DataType.Real)
            };
            
            var table = new Table(_testTableName, columns);
            _databaseService.CreateTable(table);
        }

        [Test]
        public void RenameColumn_ShouldChangeColumnName()
        {
            // Arrange
            var table = _databaseService.GetTable(_testTableName);
            var oldName = "Age";
            var newName = "UserAge";

            // Act
            var result = table?.RenameColumn(oldName, newName);
            var columnNames = table?.GetColumnNames();

            // Assert
            Assert.That(result, Is.True);
            Assert.That(columnNames?.Contains("UserAge"), Is.True);
            Assert.That(columnNames.Contains("Age"), Is.False);
        }

        [Test]
        public void ReorderColumns_ShouldChangeColumnOrder()
        {
            // Arrange
            var table = _databaseService.GetTable(_testTableName);
            var newOrder = new List<string> { "Salary", "Name", "Age", "ID" };

            // Act
            var result = table?.ReorderColumns(newOrder);
            var columnNames = table?.GetColumnNames();

            // Assert
            Assert.That(result, Is.True);
            Assert.That(newOrder, Is.EqualTo(columnNames));
        }

        [TearDown]
        public void TearDown()
        {
            _storageService.Clear();
            _tempStorageService.Clear();
        }


        private FileService CreateMockFileService()
        {
            var mockFileStorage = new MockFileStorage();
            return new FileService(mockFileStorage);
        }
    }
}