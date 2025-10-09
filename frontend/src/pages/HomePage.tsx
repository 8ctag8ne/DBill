//src/pages/HomePage.tsx
import { Database, Download, FileText, Home, Plus, TableIcon, Trash2, Upload } from "lucide-react";
import type { useState } from "react";
import type { DatabaseApi } from "../api/databaseApi";

export function HomePage({ databaseInfo, onRefresh, onNavigate }: any) {
  return (
    <div className="space-y-6">
      <h2 className="text-3xl font-bold text-gray-800">Welcome to Database Manager</h2>
      
      {databaseInfo ? (
        <div className="bg-white p-6 rounded-lg shadow">
          <h3 className="text-xl font-semibold mb-4">Current Database</h3>
          <div className="space-y-2">
            <p><strong>Name:</strong> {databaseInfo.name}</p>
            <p><strong>Tables:</strong> {databaseInfo.tableCount}</p>
            <div className="mt-4">
              <h4 className="font-semibold mb-2">Tables List:</h4>
              <ul className="list-disc list-inside">
                {databaseInfo.tables?.map((table: any) => (
                  <li key={table.name}>
                    {table.name} ({table.columnCount} columns, {table.rowCount} rows)
                  </li>
                ))}
              </ul>
            </div>
          </div>
          <button
            onClick={onRefresh}
            className="mt-4 bg-blue-500 text-gray-900 px-4 py-2 rounded hover:bg-blue-600"
          >
            Refresh
          </button>
        </div>
      ) : (
        <div className="bg-yellow-50 border border-yellow-200 p-6 rounded-lg">
          <p className="text-yellow-800">No database is currently loaded.</p>
          <button
            onClick={() => onNavigate('database')}
            className="mt-4 bg-blue-500 text-gray-900 px-4 py-2 rounded hover:bg-blue-600"
          >
            Go to Database Management
          </button>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-8">
        <div className="bg-white p-6 rounded-lg shadow hover:shadow-lg transition cursor-pointer" onClick={() => onNavigate('database')}>
          <Database size={48} className="text-blue-500 mb-4" />
          <h3 className="text-xl font-semibold mb-2">Manage Database</h3>
          <p className="text-gray-600">Create, load, or save databases</p>
        </div>
        <div className="bg-white p-6 rounded-lg shadow hover:shadow-lg transition cursor-pointer" onClick={() => onNavigate('tables')}>
          <TableIcon size={48} className="text-green-500 mb-4" />
          <h3 className="text-xl font-semibold mb-2">Manage Tables</h3>
          <p className="text-gray-600">Create, view, and modify tables</p>
        </div>
        <div className="bg-white p-6 rounded-lg shadow hover:shadow-lg transition">
          <FileText size={48} className="text-purple-500 mb-4" />
          <h3 className="text-xl font-semibold mb-2">Data Operations</h3>
          <p className="text-gray-600">Add, edit, and delete rows</p>
        </div>
      </div>
    </div>
  );
}