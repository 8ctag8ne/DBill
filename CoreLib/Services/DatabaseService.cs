using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreLib.Models;

namespace CoreLib.Services
{
    public class DatabaseService
    {
        private readonly IDatabaseStorageService _storageService;
        private Database? _currentDatabase;

        public Database? CurrentDatabase => _currentDatabase;

        public DatabaseService(IDatabaseStorageService storageService)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        public Database CreateDatabase(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Database name cannot be null or empty", nameof(name));

            _currentDatabase = new Database(name);
            return _currentDatabase;
        }

        public async Task<Database> LoadDatabaseAsync(string filePath)
        {
            _currentDatabase = await _storageService.LoadDatabaseAsync(filePath);
            
            var validation = _currentDatabase.Validate();
            if (!validation.IsValid)
                throw new InvalidOperationException($"Database validation failed: {string.Join(", ", validation.Errors)}");

            return _currentDatabase;
        }

        public async Task SaveDatabaseAsync(string filePath)
        {
            if (_currentDatabase == null)
                throw new InvalidOperationException("No database is currently loaded");

            var validation = _currentDatabase.Validate();
            if (!validation.IsValid)
                throw new InvalidOperationException($"Cannot save invalid database: {string.Join(", ", validation.Errors)}");

            await _storageService.SaveDatabaseAsync(_currentDatabase, filePath);
        }

        public List<string> GetTableNames()
        {
            return _currentDatabase?.GetTableNames() ?? new List<string>();
        }

        public Table? GetTable(string tableName)
        {
            return _currentDatabase?.GetTable(tableName);
        }

        public bool TableExists(string tableName)
        {
            return _currentDatabase?.TableExists(tableName) ?? false;
        }

        public void CreateTable(Table table)
        {
            if (_currentDatabase == null)
                throw new InvalidOperationException("No database is currently loaded");

            _currentDatabase.AddTable(table);
        }

        public bool DeleteTable(string tableName)
        {
            if (_currentDatabase == null)
                throw new InvalidOperationException("No database is currently loaded");

            return _currentDatabase.RemoveTable(tableName);
        }

        public DatabaseStatistics GetStatistics()
        {
            if (_currentDatabase == null)
                throw new InvalidOperationException("No database is currently loaded");

            return _currentDatabase.GetStatistics();
        }

        public ValidationResult ValidateCurrentDatabase()
        {
            if (_currentDatabase == null)
                return ValidationResult.Failure("No database is currently loaded");

            return _currentDatabase.Validate();
        }
    }
}