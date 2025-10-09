// src/components/ReorderColumnsForm.tsx
import { useState } from "react";
import { GripVertical, ArrowUp, ArrowDown } from "lucide-react";
import type { Column } from "../api/types";
import { TableApi } from "../api/tableApi";

export function ReorderColumnsForm({ tableName, columns, onClose, onSuccess }: any) {
  const [columnOrder, setColumnOrder] = useState<string[]>(columns.map((c: Column) => c.name));
  const [error, setError] = useState('');

  const moveColumn = (index: number, direction: 'up' | 'down') => {
    const newOrder = [...columnOrder];
    const targetIndex = direction === 'up' ? index - 1 : index + 1;
    
    if (targetIndex < 0 || targetIndex >= newOrder.length) return;
    
    [newOrder[index], newOrder[targetIndex]] = [newOrder[targetIndex], newOrder[index]];
    setColumnOrder(newOrder);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    const response = await TableApi.reorderColumns(tableName, columnOrder);
    if (response.error || response.errors) {
      setError(response.error || response.errors?.join(', ') || 'Failed to reorder');
    } else {
      onSuccess();
    }
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow mb-6">
      <h3 className="text-xl font-semibold mb-4">Reorder Columns</h3>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 p-4 rounded mb-4">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="space-y-2">
          {columnOrder.map((colName, index) => (
            <div key={colName} className="flex items-center gap-2 bg-gray-50 p-3 rounded">
              <span className="flex-1 font-medium">{colName}</span>
              <button
                type="button"
                onClick={() => moveColumn(index, 'up')}
                disabled={index === 0}
                className="px-3 py-1 bg-blue-500 text-gray-900 rounded hover:bg-blue-600 disabled:bg-gray-300 disabled:cursor-not-allowed"
              >
                ↑
              </button>
              <button
                type="button"
                onClick={() => moveColumn(index, 'down')}
                disabled={index === columnOrder.length - 1}
                className="px-3 py-1 bg-blue-500 text-gray-900 rounded hover:bg-blue-600 disabled:bg-gray-300 disabled:cursor-not-allowed"
              >
                ↓
              </button>
            </div>
          ))}
        </div>

        <div className="flex gap-2">
          <button
            type="submit"
            className="flex-1 bg-blue-500 text-gray-900 px-6 py-2 rounded hover:bg-blue-600"
          >
            Apply Reorder
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