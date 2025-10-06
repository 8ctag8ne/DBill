namespace CoreLib.Tests;

using CoreLib.Models;
using CoreLib.Services;

[TestFixture]
public class DatabaseOperationsTests
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

        var fileService = CreateMockFileService();
        var tempFileService = CreateMockFileService();

        _databaseService = new DatabaseService(_storageService, _tempStorageService, fileService, tempFileService);
        _databaseService.CreateDatabase("TestDB");

        _testTableName = "TestTable";

        var columns = new List<Column>
            {
                new Column("ID", DataType.Integer),
                new Column("Name", DataType.String),
                new Column("Score", DataType.Real),
                new Column("Active", DataType.Char)
            };

        var table = new Table(_testTableName, columns);
        _databaseService.CreateTable(table);
    }

    [Test]
    public void AddRow_WithValidData_ShouldIncreaseRowCount()
    {
        // Arrange
        var table = _databaseService.GetTable(_testTableName);
        var initialCount = table?.GetRowCount();
        var rowData = new Dictionary<string, object?>
        {
            ["ID"] = 1,
            ["Name"] = "Test User",
            ["Score"] = 95.5,
            ["Active"] = 'Y'
        };

        // Act
        table?.AddRow(rowData);
        var finalCount = table?.GetRowCount();

        // Assert
        Assert.That(initialCount + 1, Is.EqualTo(finalCount));
    }

    [Test]
    public void UpdateRow_WithValidData_ShouldModifyRow()
    {
        // Arrange
        var table = _databaseService.GetTable(_testTableName);
        var initialRowData = new Dictionary<string, object?>
        {
            ["ID"] = 1,
            ["Name"] = "Old Name",
            ["Score"] = 80.0,
            ["Active"] = 'N'
        };
        table?.AddRow(initialRowData);

        var updatedRowData = new Dictionary<string, object?>
        {
            ["ID"] = 1,
            ["Name"] = "New Name",
            ["Score"] = 90.0,
            ["Active"] = 'Y'
        };

        // Act
        table?.UpdateRow(0, updatedRowData);
        var updatedRow = table?.GetRow(0);

        // Assert
        Assert.That(updatedRow?["Name"], Is.EqualTo("New Name"));
        Assert.That(updatedRow["Score"], Is.EqualTo(90.0));
        Assert.That(updatedRow["Active"], Is.EqualTo('Y'));
    }

    [Test]
    public void DeleteRow_ShouldDecreaseRowCount()
    {
        // Arrange
        var table = _databaseService.GetTable(_testTableName);
        var rowData = new Dictionary<string, object?>
        {
            ["ID"] = 1,
            ["Name"] = "Test User",
            ["Score"] = 95.5,
            ["Active"] = 'Y'
        };
        table?.AddRow(rowData);
        var countBeforeDelete = table?.GetRowCount();

        // Act
        table?.DeleteRow(0);
        var countAfterDelete = table?.GetRowCount();

        // Assert
        Assert.That(countBeforeDelete - 1, Is.EqualTo(countAfterDelete));
    }

    [Test]
    public void CreateTable_ShouldAddTableToDatabase()
    {
        // Arrange
        var newTableName = "NewTable";
        var columns = new List<Column>
            {
                new Column("Col1", DataType.Integer),
                new Column("Col2", DataType.String)
            };
        var newTable = new Table(newTableName, columns);

        // Act
        _databaseService.CreateTable(newTable);
        var tableNames = _databaseService.GetTableNames();

        // Assert
        Assert.That(tableNames, Does.Contain(newTableName));
    }

    [Test]
    public void DeleteTable_ShouldRemoveTableFromDatabase()
    {
        // Act
        var result = _databaseService.DeleteTable(_testTableName);
        var tableNames = _databaseService.GetTableNames();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(tableNames.Contains(_testTableName), Is.False);
    }

    [TearDown]
    public void TearDown()
    {
        _storageService.Clear();
        _tempStorageService.Clear();
    }

    private FileService CreateMockFileService()
    {
        // Проста mock-реалізація FileService для тестів
        var mockFileStorage = new MockFileStorage();
        return new FileService(mockFileStorage);
    }
}