// src/components/FileManager.tsx
import React, { useState } from 'react';
import { FileApi } from '../api/fileApi';

export const FileManager: React.FC = () => {
  const [uploading, setUploading] = useState(false);

  const handleFileUpload = async (file: File) => {
    try {
      setUploading(true);
      const response = await FileApi.upload(file);
      if (response.message) {
        alert('File uploaded successfully!');
      }
    } catch (error) {
      console.error('Failed to upload file:', error);
    } finally {
      setUploading(false);
    }
  };

  const handleCleanup = async () => {
    if (window.confirm('Are you sure you want to cleanup all files?')) {
      try {
        await FileApi.cleanup();
        alert('All files cleaned up successfully!');
      } catch (error) {
        console.error('Failed to cleanup files:', error);
      }
    }
  };

  return (
    <div className="space-y-6">
      <div className="bg-white shadow rounded-lg">
        <div className="px-4 py-5 sm:p-6">
          <h2 className="text-lg font-medium text-gray-900 mb-4">File Management</h2>
          
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">Upload File</label>
              <input
                type="file"
                onChange={(e) => e.target.files?.[0] && handleFileUpload(e.target.files[0])}
                disabled={uploading}
                className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
              />
            </div>

            <div>
              <button
                onClick={handleCleanup}
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-gray-900 bg-red-600 hover:bg-red-700"
              >
                Cleanup All Files
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};