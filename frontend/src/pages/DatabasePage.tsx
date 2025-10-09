// src/pages/DatabasePage.tsx
import { Plus, Upload, Download, Trash2, Database, FolderOpen } from "lucide-react";
import { useState, useRef } from "react";
import { DatabaseApi } from "../api/databaseApi";
import { FileUploader } from "../components/FileUploader";

export function DatabasePage({ onDatabaseChange }: any) {
  const [dbName, setDbName] = useState('');
  const [saveFilePath, setSaveFilePath] = useState('');
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [message, setMessage] = useState('');

  const handleCreate = async () => {
    if (!dbName.trim()) {
      setMessage('Please enter a database name');
      return;
    }
    const response = await DatabaseApi.create(dbName);
    setMessage(response.message || response.error || 'Database created');
    if (!response.error) {
      onDatabaseChange();
      setDbName('');
    }
  };

  const handleLoad = async () => {
    if (!selectedFile) {
      setMessage('Please select a database file first');
      return;
    }
    
    const response = await DatabaseApi.load(selectedFile);
    setMessage(response.message || response.error || 'Database loaded');
    if (!response.error) {
      onDatabaseChange();
      setSelectedFile(null);
    }
  };

  const handleSave = async () => {
    if (!saveFilePath.trim()) {
      setMessage('Please select a file location');
      return;
    }
    const response = await DatabaseApi.save(saveFilePath);
    setMessage(response.message || response.error || 'Database saved');
    if (!response.error) {
      setSaveFilePath('');
    }
  };

  const handleClose = async () => {
    const response = await DatabaseApi.close();
    setMessage(response.message || response.error || 'Database closed');
    if (!response.error) {
      onDatabaseChange();
    }
  };

  const handleSelectSaveLocation = () => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.json';
    input.style.display = 'none';
    
    input.addEventListener('change', (e) => {
      const target = e.target as HTMLInputElement;
      const file = target.files?.[0];
      if (file) {
        setSaveFilePath(file.name);
      }
    });
    
    document.body.appendChild(input);
    input.click();
    document.body.removeChild(input);
  };

  const handleDownload = async () => {
    try {
      const response = await DatabaseApi.download();
      
      if (response.ok) {
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.style.display = 'none';
        a.href = url;
        
        const contentDisposition = response.headers.get('content-disposition');
        let filename = 'database.json';
        if (contentDisposition) {
          const filenameMatch = contentDisposition.match(/filename="?(.+)"?/);
          if (filenameMatch) {
            filename = filenameMatch[1];
          }
        }
        
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
        setMessage('Database downloaded successfully');
      }
    } catch (err: any) {
      setMessage(err.message || 'An error occurred while downloading the database');
    }
  };

  const handleFileChange = (file: File | null) => {
    setSelectedFile(file);
  };

  return (
    <div className="space-y-6">
      <h2 className="text-3xl font-bold text-gray-800 flex items-center gap-2">
        <Database size={32} />
        Database Management
      </h2>

      {message && (
        <div className={`p-4 rounded ${
          message.includes('error') || message.includes('Failed') 
            ? 'bg-red-50 border border-red-200 text-red-700' 
            : 'bg-blue-50 border border-blue-200 text-blue-700'
        }`}>
          {message}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Create Database */}
        <div className="bg-white p-6 rounded-lg shadow">
          <h3 className="text-xl font-semibold mb-4 flex items-center gap-2">
            <Plus size={20} className="text-green-600" />
            Create New Database
          </h3>
          <div className="space-y-3">
            <input
              type="text"
              value={dbName}
              onChange={(e) => setDbName(e.target.value)}
              placeholder="Enter database name..."
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              onKeyPress={(e) => e.key === 'Enter' && handleCreate()}
            />
            <button
              onClick={handleCreate}
              className="w-full bg-green-500 text-gray-900 px-4 py-2 rounded-lg hover:bg-green-600 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2 transition-colors font-medium"
            >
              Create Database
            </button>
          </div>
        </div>

        {/* Load Database */}
        <div className="bg-white p-6 rounded-lg shadow">
          <h3 className="text-xl font-semibold mb-4 flex items-center gap-2">
            <Upload size={20} className="text-blue-600" />
            Load Database
          </h3>
          <div className="space-y-4">
            <FileUploader
              onFileChange={handleFileChange}
              accept=".json"
            />
            <button
              onClick={handleLoad}
              disabled={!selectedFile}
              className="w-full bg-blue-500 text-gray-900 px-4 py-2 rounded-lg hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-medium"
            >
              Load Database
            </button>
          </div>
        </div>

        {/* Download & Close Database - в одному рядку */}
        <div className="bg-white p-6 rounded-lg shadow">
            {/* Download Database */}
            <div>
              <h3 className="text-xl font-semibold mb-4 flex items-center gap-2">
                <Download size={20} className="text-orange-600" />
                Download Database
              </h3>
              <div className="space-y-4">
                <p className="text-sm text-gray-600">
                  Download the current database as a JSON file
                </p>
                <button
                  onClick={handleDownload}
                  className="w-full bg-orange-500 text-gray-900 px-4 py-2 rounded-lg hover:bg-orange-600 focus:outline-none focus:ring-2 focus:ring-orange-500 focus:ring-offset-2 transition-colors font-medium"
                >
                  Download Database File
                </button>
              </div>
            </div>
          </div>

            {/* Close Database */}
            <div className="bg-white p-6 rounded-lg shadow">
            <div>
              <h3 className="text-xl font-semibold mb-4 flex items-center gap-2">
                <Trash2 size={20} className="text-red-600" />
                Close Database
              </h3>
              <div className="space-y-4">
                <p className="text-sm text-gray-600">
                  Close the current database. You'll need to load or create a new one to continue working.
                </p>
                <button
                  onClick={handleClose}
                  className="w-full bg-red-500 text-gray-900 px-4 py-2 rounded-lg hover:bg-red-600 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 transition-colors font-medium"
                >
                  Close Current Database
                </button>
              </div>
            </div>
          </div>
      </div>

      {/* Database Information */}
      <div className="bg-gray-50 p-4 rounded-lg border border-gray-200">
        <h3 className="font-semibold text-gray-800 mb-2">Database Information</h3>
        <div className="text-sm text-gray-600 space-y-1">
          <p>• Create a new empty database with your preferred name</p>
          <p>• Load an existing database from a JSON file (select file and click Load)</p>
          <p>• Save the current database to a specific location</p>
          <p>• Download the database file directly to your downloads folder</p>
          <p>• Close the current database when finished working</p>
        </div>
      </div>
    </div>
  );
}