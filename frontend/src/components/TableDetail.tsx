// src/components/TableDetail.tsx
import React, { useState, useEffect } from 'react';
import { TableApi } from '../api/tableApi';
import { FileApi } from '../api/fileApi';

interface TableDetailProps {
  tableName: string;
  onBack: () => void;
  onTableUpdate: () => void;
}

interface Column {
  name: string;
  type: string;
}

interface Row {
  [key: string]: any;
}

export const TableDetail: React.FC<TableDetailProps> = ({ 
  tableName, 
  onBack, 
  onTableUpdate 
}) => {
  const [tableInfo, setTableInfo] = useState<{ columns: Column[] } | null>(null);
  const [rows, setRows] = useState<Row[]>([]);
  const [loading, setLoading] = useState(false);
  const [showAddRowModal, setShowAddRowModal] = useState(false);
  const [newRow, setNewRow] = useState<Record<string, any>>({});
  const [files, setFiles] = useState<Record<string, File>>({});

  const loadTableData = async () => {
    try {
      setLoading(true);
      const [tableResponse, rowsResponse] = await Promise.all([
        TableApi.getTable(tableName),
        TableApi.getAllRows(tableName)
      ]);
      
      if (tableResponse.data) setTableInfo(tableResponse.data);
      if (rowsResponse.data) setRows(rowsResponse.data.rows);
    } catch (error) {
      console.error('Failed to load table data:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadTableData();
  }, [tableName]);

  const handleAddRow = async () => {
    try {
      await TableApi.addRow(tableName, newRow, files);
      setShowAddRowModal(false);
      setNewRow({});
      setFiles({});
      loadTableData();
      onTableUpdate();
    } catch (error) {
      console.error('Failed to add row:', error);
    }
  };

  const handleDeleteRow = async (rowIndex: number) => {
    if (window.confirm('Are you sure you want to delete this row?')) {
      try {
        await TableApi.deleteRow(tableName, rowIndex);
        loadTableData();
        onTableUpdate();
      } catch (error) {
        console.error('Failed to delete row:', error);
      }
    }
  };

  const handleFileChange = (columnName: string, file: File | null) => {
    if (file) {
      setFiles(prev => ({ ...prev, [columnName]: file }));
    } else {
      const newFiles = { ...files };
      delete newFiles[columnName];
      setFiles(newFiles);
    }
  };

  const downloadFile = async (storagePath: string, fileName: string) => {
    try {
      const blob = await FileApi.download(storagePath);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (error) {
      console.error('Failed to download file:', error);
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <button
            onClick={onBack}
            className="inline-flex items-center px-3 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
          >
            ← Back
          </button>
          <h1 className="text-2xl font-bold text-gray-900">{tableName}</h1>
        </div>
        <button
          onClick={() => setShowAddRowModal(true)}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-gray-900 bg-blue-600 hover:bg-blue-700"
        >
          Add Row
        </button>
      </div>

      <div className="bg-white shadow overflow-hidden sm:rounded-lg">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                {tableInfo?.columns.map((column) => (
                  <th
                    key={column.name}
                    className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                  >
                    {column.name} ({column.type})
                  </th>
                ))}
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {rows.map((row, rowIndex) => (
                <tr key={rowIndex}>
                  {tableInfo?.columns.map((column) => (
                    <td key={column.name} className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {column.type === 'TextFile' && row[column.name] ? (
                        <button
                          onClick={() => downloadFile(row[column.name].storagePath, row[column.name].fileName)}
                          className="text-blue-600 hover:text-blue-800 underline"
                        >
                          {row[column.name].fileName}
                        </button>
                      ) : column.type === 'IntegerInterval' && row[column.name] ? (
                        `${row[column.name].min} - ${row[column.name].max}`
                      ) : (
                        String(row[column.name] || '')
                      )}
                    </td>
                  ))}
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button
                      onClick={() => handleDeleteRow(rowIndex)}
                      className="text-red-600 hover:text-red-800"
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {rows.length === 0 && (
          <div className="text-center py-12">
            <p className="text-sm text-gray-500">No rows in this table</p>
          </div>
        )}
      </div>

      {/* Add Row Modal */}
      {showAddRowModal && tableInfo && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="mt-3">
              <h3 className="text-lg font-medium leading-6 text-gray-900">Add New Row</h3>
              
              <div className="mt-4 space-y-4">
                {tableInfo.columns.map((column) => (
                  <div key={column.name}>
                    <label className="block text-sm font-medium text-gray-700">
                      {column.name} ({column.type})
                    </label>
                    {column.type === 'TextFile' ? (
                      <input
                        type="file"
                        onChange={(e) => handleFileChange(column.name, e.target.files?.[0] || null)}
                        className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
                      />
                    ) : column.type === 'IntegerInterval' ? (
                      <div className="flex space-x-2 mt-1">
                        <input
                          type="number"
                          placeholder="Min"
                          onChange={(e) => setNewRow(prev => ({
                            ...prev,
                            [column.name]: { ...prev[column.name], min: e.target.value }
                          }))}
                          className="flex-1 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
                        />
                        <input
                          type="number"
                          placeholder="Max"
                          onChange={(e) => setNewRow(prev => ({
                            ...prev,
                            [column.name]: { ...prev[column.name], max: e.target.value }
                          }))}
                          className="flex-1 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
                        />
                      </div>
                    ) : (
                      <input
                        type={column.type === 'Integer' || column.type === 'Real' ? 'number' : 'text'}
                        onChange={(e) => setNewRow(prev => ({ ...prev, [column.name]: e.target.value }))}
                        className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
                      />
                    )}
                  </div>
                ))}
              </div>

              <div className="flex justify-end space-x-3 mt-6">
                <button
                  onClick={() => setShowAddRowModal(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 hover:text-gray-500"
                >
                  Cancel
                </button>
                <button
                  onClick={handleAddRow}
                  className="px-4 py-2 text-sm font-medium text-gray-900 bg-blue-600 rounded-md hover:bg-blue-700"
                >
                  Add Row
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};