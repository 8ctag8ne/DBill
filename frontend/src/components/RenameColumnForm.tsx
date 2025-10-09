// src/components/RenameColumnForm.tsx
import { useState } from "react";
import { Edit3 } from "lucide-react";
import type { Column } from "../api/types";
import { TableApi } from "../api/tableApi";

export function RenameColumnForm({ tableName, columns, onClose, onSuccess }: any) {
  const [oldName, setOldName] = useState('');
  const [newName, setNewName] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!oldName || !newName) {
      setError('Both fields are required');
      return;
    }

    const response = await TableApi.renameColumn(tableName, oldName, newName);
    if (response.error) {
      setError(response.error);
    } else {
      onSuccess();
    }
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow mb-6">
      <h3 className="text-xl font-semibold mb-4">Rename Column</h3>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 p-4 rounded mb-4">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-2">Select Column</label>
          <select
            value={oldName}
            onChange={(e) => setOldName(e.target.value)}
            className="w-full px-4 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">-- Select Column --</option>
            {columns.map((col: Column) => (
              <option key={col.name} value={col.name}>
                {col.name}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium mb-2">New Name</label>
          <input
            type="text"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            className="w-full px-4 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div className="flex gap-2">
          <button
            type="submit"
            className="flex-1 bg-blue-500 text-gray-900 px-6 py-2 rounded hover:bg-blue-600"
          >
            Rename
          </button>
          <button
            type="button"
            onClick={onClose}
            className="flex-1 bg-gray-500 text-gray-900 px-6 py-2 rounded hover:bg-gray-600"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}