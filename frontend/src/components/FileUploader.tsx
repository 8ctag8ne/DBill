//src/components/FileUploader.tsx
import { useState, useRef } from "react";
import { Upload, X, File as FileIcon } from "lucide-react";
import { type TextFileFromApi } from "../api/types";

interface FileUploaderProps {
  onFileChange: (file: File | null) => void;
  currentFile?: TextFileFromApi | null;
  accept?: string;
}

export function FileUploader({ onFileChange, currentFile, accept }: FileUploaderProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] || null;
    setSelectedFile(file);
    onFileChange(file);
  };

  const handleRemoveFile = () => {
    setSelectedFile(null);
    onFileChange(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-3">
        <input
          ref={fileInputRef}
          type="file"
          onChange={handleFileSelect}
          accept={accept}
          className="hidden"
        />
        
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          className="flex items-center gap-2 bg-blue-500 text-gray-900 px-4 py-2 rounded hover:bg-blue-600 transition-colors font-medium"
        >
          <Upload size={16} />
          Choose File
        </button>

        {(selectedFile || currentFile) && (
          <button
            type="button"
            onClick={handleRemoveFile}
            className="flex items-center gap-2 bg-red-500 text-gray-900 px-3 py-2 rounded hover:bg-red-600 transition-colors font-medium"
          >
            <X size={16} />
            Remove
          </button>
        )}
      </div>

      {selectedFile && (
        <div className="bg-blue-50 border border-blue-200 rounded p-3">
          <div className="flex items-center gap-3">
            <FileIcon size={20} className="text-blue-600 flex-shrink-0" />
            <div className="flex-1 min-w-0">
              <p className="font-medium text-blue-900 truncate">{selectedFile.name}</p>
              <p className="text-sm text-blue-700">
                {formatFileSize(selectedFile.size)}
              </p>
            </div>
            <span className="text-xs bg-blue-200 text-blue-800 px-2 py-1 rounded font-medium flex-shrink-0">
              New
            </span>
          </div>
        </div>
      )}

      {currentFile && !selectedFile && (
        <div className="bg-gray-50 border border-gray-200 rounded p-3">
          <div className="flex items-center gap-3">
            <FileIcon size={20} className="text-gray-600 flex-shrink-0" />
            <div className="flex-1 min-w-0">
              <p className="font-medium text-gray-900 truncate">{currentFile.fileName}</p>
              <p className="text-sm text-gray-700">
                {formatFileSize(currentFile.size)}
              </p>
            </div>
            <span className="text-xs bg-gray-300 text-gray-800 px-2 py-1 rounded font-medium flex-shrink-0">
              Current
            </span>
          </div>
        </div>
      )}

      {!selectedFile && !currentFile && (
        <div className="text-gray-500 text-sm italic p-2 bg-gray-50 rounded border border-dashed border-gray-300">
          No file selected
        </div>
      )}
    </div>
  );
}