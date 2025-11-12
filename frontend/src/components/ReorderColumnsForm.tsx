// src/components/ReorderColumnsForm.tsx
import { useState } from "react";
import { GripVertical, X } from "lucide-react";
import type { Column } from "../api/types";
import { TableApi } from "../api/tableApi";

export function ReorderColumnsForm({ tableName, columns, onClose, onSuccess }: any) {
  const [columnOrder, setColumnOrder] = useState<string[]>(columns.map((c: Column) => c.name));
  const [error, setError] = useState('');
  const [draggedItem, setDraggedItem] = useState<number | null>(null);

  const handleDragStart = (e: React.DragEvent<HTMLDivElement>, index: number) => {
    setDraggedItem(index);
    e.dataTransfer.effectAllowed = "move";
    e.dataTransfer.setData("text/html", index.toString());
  };

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>, index: number) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = "move";
  };

  const handleDrop = (e: React.DragEvent<HTMLDivElement>, targetIndex: number) => {
    e.preventDefault();
    
    if (draggedItem === null) return;

    const newOrder = [...columnOrder];
    const [movedItem] = newOrder.splice(draggedItem, 1);
    newOrder.splice(targetIndex, 0, movedItem);
    
    setColumnOrder(newOrder);
    setDraggedItem(null);
  };

  const handleDragEnd = () => {
    setDraggedItem(null);
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

  const resetOrder = () => {
    setColumnOrder(columns.map((c: Column) => c.name));
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow-lg border border-gray-200 mb-6">
      <div className="flex justify-between items-start mb-6">
        <div>
          <h3 className="text-2xl font-bold text-gray-800">Reorder Columns</h3>
          <p className="text-gray-600 mt-1">
            Drag and drop columns to change their order in the table
          </p>
        </div>
        <button
          onClick={onClose}
          className="text-gray-500 hover:text-gray-700 transition-colors p-1 rounded-full hover:bg-gray-100"
          title="Close"
        >
          <X size={24} />
        </button>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-4">
          {error}
        </div>
      )}

      {/* Instructions */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
        <p className="text-sm text-blue-700 font-medium flex items-center gap-2">
          <span className="text-lg">💡</span>
          Drag the <GripVertical size={16} className="inline text-blue-500" /> handle to reorder columns
        </p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="space-y-3">
          {columnOrder.map((colName, index) => {
            const column = columns.find((c: Column) => c.name === colName);
            return (
              <div
                key={colName}
                draggable
                onDragStart={(e) => handleDragStart(e, index)}
                onDragOver={(e) => handleDragOver(e, index)}
                onDrop={(e) => handleDrop(e, index)}
                onDragEnd={handleDragEnd}
                className={`
                  flex items-center gap-4 p-4 rounded-lg border-2 transition-all duration-200 cursor-move
                  ${draggedItem === index 
                    ? 'bg-purple-50 border-purple-300 shadow-md scale-[1.02]' 
                    : 'bg-white border-gray-200 hover:bg-gray-50 hover:border-gray-300 shadow-sm'
                  }
                `}
              >
                <GripVertical 
                  size={20} 
                  className="text-gray-400 flex-shrink-0 cursor-grab active:cursor-grabbing hover:text-gray-600" 
                />
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-gray-900 text-lg">{colName}</p>
                  {column && (
                    <p className="text-sm text-gray-500 mt-1">
                      {typeof column.type === 'string' ? column.type : column.type.toString()}
                    </p>
                  )}
                </div>
                <div className="flex items-center gap-3 flex-shrink-0">
                  <span className="text-sm font-medium text-gray-700 bg-gray-100 px-3 py-1 rounded-full">
                    Position: {index + 1}
                  </span>
                </div>
              </div>
            );
          })}
        </div>

        {/* Buttons */}
        <div className="flex gap-3 pt-6 border-t border-gray-200">
          <button
            type="submit"
            className="flex-1 bg-green-500 text-gray-900 px-6 py-3 rounded-lg hover:bg-green-600 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2 transition-colors font-medium text-lg"
          >
            Apply New Column Order
          </button>
          <button
            type="button"
            onClick={resetOrder}
            className="px-6 py-3 bg-gray-500 text-gray-900 rounded-lg hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors font-medium"
          >
            Reset to Original
          </button>
          <button
            type="button"
            onClick={onClose}
            className="px-6 py-3 bg-red-500 text-gray-900 rounded-lg hover:bg-red-600 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 transition-colors font-medium"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}