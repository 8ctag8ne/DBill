/* eslint-disable no-restricted-globals */
//src/pages/TablesPage.tsx
import { Plus, Trash2, TableIcon } from "lucide-react";
import { useState } from "react";
import { TableApi } from "../api/tableApi";
import { DataType, type Column } from "../api/types";

export function TablesPage({ tables, onTablesChange, onSelectTable }: any) {
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [tableName, setTableName] = useState('');
  const [columns, setColumns] = useState<Column[]>([{ name: '', type: DataType.String }]);
  const [message, setMessage] = useState('');

  const handleAddColumn = () => {
    setColumns([...columns, { name: '', type: DataType.String }]);
  };

  const handleRemoveColumn = (index: number) => {
    setColumns(columns.filter((_, i) => i !== index));
  };

  const handleColumnChange = (index: number, field: 'name' | 'type', value: any) => {
    const newColumns = [...columns];
    newColumns[index] = { ...newColumns[index], [field]: value };
    setColumns(newColumns);
  };

  const handleCreateTable = async () => {
    if (!tableName.trim()) {
      setMessage('Please enter table name');
      return;
    }
    if (columns.some(c => !c.name.trim())) {
      setMessage('All columns must have names');
      return;
    }

    const response = await TableApi.create(tableName, columns);
    setMessage(response.message || response.error || response.errors?.join(', ') || 'Table created');
    
    if (!response.error && !response.errors) {
      setTableName('');
      setColumns([{ name: '', type: DataType.String }]);
      setShowCreateForm(false);
      onTablesChange();
    }
  };

  const handleDeleteTable = async (table: string) => {
    if (!confirm(`Delete table "${table}"?`)) return;
    
    const response = await TableApi.delete(table);
    setMessage(response.message || response.error || 'Table deleted');
    if (!response.error) {
      onTablesChange();
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-3xl font-bold text-gray-800">Tables</h2>
        <button
          onClick={() => setShowCreateForm(!showCreateForm)}
          className="bg-green-500 text-gray-900 px-4 py-2 rounded hover:bg-green-600 flex items-center gap-2"
        >
          <Plus size={18} />
          Create Table
        </button>
      </div>

      {message && (
        <div className="bg-blue-50 border border-blue-200 p-4 rounded">
          {message}
        </div>
      )}

      {showCreateForm && (
        <div className="bg-white p-6 rounded-lg shadow">
          <h3 className="text-xl font-semibold mb-4">Create New Table</h3>
          
          <div className="mb-4">
            <label className="block text-sm font-medium mb-2">Table Name</label>
            <input
              type="text"
              value={tableName}
              onChange={(e) => setTableName(e.target.value)}
              className="w-full px-4 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div className="mb-4">
            <label className="block text-sm font-medium mb-2">Columns</label>
            {columns.map((col, index) => (
              <div key={index} className="flex gap-2 mb-2">
                <input
                  type="text"
                  value={col.name}
                  onChange={(e) => handleColumnChange(index, 'name', e.target.value)}
                  placeholder="Column name"
                  className="flex-1 px-4 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <select
                  value={col.type}
                  onChange={(e) => handleColumnChange(index, 'type', parseInt(e.target.value))}
                  className="px-4 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value={DataType.Integer}>Integer</option>
                  <option value={DataType.Real}>Real</option>
                  <option value={DataType.Char}>Char</option>
                  <option value={DataType.String}>String</option>
                  <option value={DataType.TextFile}>TextFile</option>
                  <option value={DataType.IntegerInterval}>IntegerInterval</option>
                </select>
                {columns.length > 1 && (
                  <button
                    onClick={() => handleRemoveColumn(index)}
                    className="bg-red-500 text-gray-900 px-3 py-2 rounded hover:bg-red-600"
                  >
                    <Trash2 size={18} />
                  </button>
                )}
              </div>
            ))}
            <button
              onClick={handleAddColumn}
              className="mt-2 bg-blue-500 text-gray-900 px-4 py-2 rounded hover:bg-blue-600 flex items-center gap-2"
            >
              <Plus size={18} />
              Add Column
            </button>
          </div>

          <div className="flex gap-2">
            <button
              onClick={handleCreateTable}
              className="bg-green-500 text-gray-900 px-6 py-2 rounded hover:bg-green-600"
            >
              Create
            </button>
            <button
              onClick={() => setShowCreateForm(false)}
              className="bg-gray-500 text-gray-900 px-6 py-2 rounded hover:bg-gray-600"
            >
              Cancel
            </button>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {tables.map((table: string) => (
          <div key={table} className="bg-white p-6 rounded-lg shadow hover:shadow-lg transition">
            <h3 className="text-xl font-semibold mb-4 flex items-center gap-2">
              <TableIcon size={20} />
              {table}
            </h3>
            <div className="flex gap-2">
              <button
                onClick={() => onSelectTable(table)}
                className="flex-1 bg-blue-500 text-gray-900 px-4 py-2 rounded hover:bg-blue-600"
              >
                Open
              </button>
              <button
                onClick={() => handleDeleteTable(table)}
                className="bg-red-500 text-gray-900 px-4 py-2 rounded hover:bg-red-600"
              >
                <Trash2 size={18} />
              </button>
            </div>
          </div>
        ))}
      </div>

      {tables.length === 0 && !showCreateForm && (
        <div className="bg-gray-50 border-2 border-dashed border-gray-300 p-12 rounded-lg text-center">
          <TableIcon size={48} className="mx-auto text-gray-400 mb-4" />
          <p className="text-gray-600 mb-4">No tables yet. Create your first table!</p>
          <button
            onClick={() => setShowCreateForm(true)}
            className="bg-blue-500 text-gray-900 px-6 py-2 rounded hover:bg-blue-600"
          >
            Create Table
          </button>
        </div>
      )}
    </div>
  );
}