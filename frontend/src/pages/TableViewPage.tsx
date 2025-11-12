// src/pages/TableViewPage.tsx
import { Plus, Edit, Trash2, ChevronLeft, Menu } from "lucide-react";
import { useState, useEffect } from "react";
import { DataType, stringToDataType, type Column } from "../api/types";
import { TableApi } from "../api/tableApi";
import { CellRenderer } from "../components/CellRenderer";
import { RenameColumnForm } from "../components/RenameColumnForm";
import { ReorderColumnsForm } from "../components/ReorderColumnsForm";
import { RowForm } from "../components/RowForm";

export function TableViewPage({ tableName, onBack }: any) {
  const [columns, setColumns] = useState<Column[]>([]);
  const [rows, setRows] = useState<any[]>([]);
  const [showAddForm, setShowAddForm] = useState(false);
  const [showEditForm, setShowEditForm] = useState(false);
  const [editingRowIndex, setEditingRowIndex] = useState<number | null>(null);
  const [showRenameForm, setShowRenameForm] = useState(false);
  const [showReorderForm, setShowReorderForm] = useState(false);
  const [message, setMessage] = useState('');

  const loadTableData = async () => {
    try {
      const colResponse: any = await TableApi.getColumns(tableName);
      if (colResponse.columns) {
        setColumns(colResponse.columns);
      } else {
        setMessage(colResponse.error || 'Failed to load columns');
      }

      const rowsResponse: any = await TableApi.getAllRows(tableName);
      if (rowsResponse.rows) {
        setRows(rowsResponse.rows);
      } else {
        setMessage(rowsResponse.error || 'Failed to load rows');
      }
    } catch (error) {
      setMessage('Error loading table data');
      console.error('Error loading table data:', error);
    }
  };

  useEffect(() => {
    loadTableData();
  }, [tableName]);

  const handleDeleteRow = async (index: number) => {
    if (!confirm('Delete this row?')) return;
    
    const response = await TableApi.deleteRow(tableName, index);
    setMessage(response.message || response.error || 'Row deleted');
    if (!response.error) {
      loadTableData();
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <button
            onClick={onBack}
            className="text-blue-500 hover:text-blue-700 mb-2 flex items-center gap-2"
          >
            <ChevronLeft size={18} />
            Back to Tables
          </button>
          <h2 className="text-3xl font-bold text-gray-800">{tableName}</h2>
        </div>
        <div className="flex gap-2">
          <button
            onClick={() => setShowAddForm(true)}
            className="bg-green-500 text-black px-4 py-2 rounded hover:bg-green-600 flex items-center gap-2 font-medium"
          >
            <Plus size={18} />
            Add Row
          </button>
          <button
            onClick={() => setShowRenameForm(true)}
            className="bg-yellow-500 text-black px-4 py-2 rounded hover:bg-yellow-600 flex items-center gap-2 font-medium"
          >
            <Edit size={18} />
            Rename Column
          </button>
          <button
            onClick={() => setShowReorderForm(true)}
            className="bg-purple-500 text-black px-4 py-2 rounded hover:bg-purple-600 flex items-center gap-2 font-medium"
          >
            <Menu size={18} />
            Reorder Columns
          </button>
        </div>
      </div>

      {message && (
        <div className={`p-4 rounded ${
          message.includes('Error') || message.includes('Failed') 
            ? 'bg-red-50 border border-red-200 text-red-700' 
            : 'bg-blue-50 border border-blue-200 text-blue-700'
        }`}>
          {message}
        </div>
      )}

      {showAddForm && (
        <RowForm
          columns={columns}
          tableName={tableName}
          onClose={() => setShowAddForm(false)}
          onSuccess={() => {
            setShowAddForm(false);
            loadTableData();
            setMessage('Row added successfully');
          }}
        />
      )}

      {showEditForm && editingRowIndex !== null && (
        <RowForm
          columns={columns}
          tableName={tableName}
          editingRowIndex={editingRowIndex}
          initialData={rows[editingRowIndex]}
          onClose={() => {
            setShowEditForm(false);
            setEditingRowIndex(null);
          }}
          onSuccess={() => {
            setShowEditForm(false);
            setEditingRowIndex(null);
            loadTableData();
            setMessage('Row updated successfully');
          }}
        />
      )}

      {showRenameForm && (
        <RenameColumnForm
          tableName={tableName}
          columns={columns}
          onClose={() => setShowRenameForm(false)}
          onSuccess={() => {
            setShowRenameForm(false);
            loadTableData();
            setMessage('Column renamed successfully');
          }}
        />
      )}

      {showReorderForm && (
        <ReorderColumnsForm
          tableName={tableName}
          columns={columns}
          onClose={() => setShowReorderForm(false)}
          onSuccess={() => {
            setShowReorderForm(false);
            loadTableData();
            setMessage('Columns reordered successfully');
          }}
        />
      )}

      <div className="bg-white rounded-lg shadow overflow-x-auto">
        <table className="w-full">
          <thead className="bg-gray-100">
            <tr>
              <th className="px-4 py-3 text-left font-semibold text-gray-900">#</th>
              {columns.map((col) => (
                <th key={col.name} className="px-4 py-3 text-left font-semibold text-gray-900">
                  {col.name}
                  <span className="text-xs text-gray-500 ml-2">
                    ({col.type})
                  </span>
                </th>
              ))}
              <th className="px-4 py-3 text-left font-semibold text-gray-900">Actions</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row, index) => (
              <tr key={index} className="border-t hover:bg-gray-50">
                <td className="px-4 py-3 text-gray-700 font-medium">{index}</td>
                {columns.map((col) => (
                  <td key={col.name} className="px-4 py-3 text-gray-700">
                    <CellRenderer value={row[col.name]} type={col.type} />
                  </td>
                ))}
                <td className="px-4 py-3">
                  <div className="flex gap-2">
                    <button
                      onClick={() => {
                        setEditingRowIndex(index);
                        setShowEditForm(true);
                      }}
                      className="text-blue-600 hover:text-blue-800 p-1 rounded hover:bg-blue-50"
                      title="Edit row"
                    >
                      <Edit size={18} />
                    </button>
                    <button
                      onClick={() => handleDeleteRow(index)}
                      className="text-red-600 hover:text-red-800 p-1 rounded hover:bg-red-50"
                      title="Delete row"
                    >
                      <Trash2 size={18} />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {rows.length === 0 && (
          <div className="p-12 text-center text-gray-500">
            No rows yet. Click "Add Row" to insert data.
          </div>
        )}
      </div>
    </div>
  );
}

