// src/components/CellRenderer.tsx
import { DataType } from "../api/types";
import { FileApi } from "../api/fileApi";
import { Download } from "lucide-react";

interface CellRendererProps {
  value: any;
  type: any; // Змінилося з DataType на string
}

interface IntegerInterval {
  min: number;
  max: number;
}

interface TextFile {
  fileName?: string;
  storagePath?: string;
  name?: string;
  path?: string;
}

// Додайте функцію для конвертації рядка в DataType
const stringToDataType = (typeString: string): DataType => {
  const typeMap: { [key: string]: DataType } = {
    'String': DataType.String,
    'Integer': DataType.Integer,
    'Real': DataType.Real,
    'Char': DataType.Char,
    'TextFile': DataType.TextFile,
    'IntegerInterval': DataType.IntegerInterval
  };
  
  return typeMap[typeString] || DataType.String;
};

export function CellRenderer({ value, type }: CellRendererProps) {
  const dataType = stringToDataType(type);
  console.log(`CellRenderer: type=${type}, converted=${DataType[dataType]}, value=`, value);
  
  // Обробка TextFile
  if (dataType === DataType.TextFile) {
    console.log('TextFile rendering, value:', value);
    
    if (!value) {
      return <span className="text-gray-400">No file</span>;
    }
    
    const fileObj = value as TextFile;
    const fileName = fileObj.fileName || fileObj.name;
    const storagePath = fileObj.storagePath || fileObj.path;
    
    console.log('File object details:', { fileName, storagePath });

    if (storagePath && fileName) {
      const downloadUrl = FileApi.getDownloadUrl(storagePath);
      
      return (
        <div className="flex items-center gap-2">
          <span className="text-sm text-blue-600 font-medium truncate max-w-xs">
            {fileName}
          </span>
          <a
            href={downloadUrl}
            download={fileName}
            className="text-blue-600 hover:text-blue-800 p-1 rounded hover:bg-blue-50 flex-shrink-0"
            title="Download file"
            onClick={(e) => e.stopPropagation()}
          >
            <Download size={16} />
          </a>
        </div>
      );
    }
    
    if (fileName) {
      return <span className="text-sm text-gray-600">{fileName}</span>;
    }
    
    return <span className="text-sm text-gray-600">{String(value)}</span>;
  }

  // Обробка IntegerInterval
  if (dataType === DataType.IntegerInterval) {
    console.log('IntegerInterval rendering, value:', value);
    
    if (!value) {
      return <span className="text-gray-400">No interval</span>;
    }

    const interval = value as IntegerInterval;
    
    if (interval && typeof interval === 'object' && ('min' in interval || 'max' in interval)) {
      const { min, max } = interval;
      
      console.log('Interval object:', { min, max });
      
      if (min !== undefined && max !== undefined) {
        return (
          <div className="flex items-center gap-2">
            <span className="px-3 py-1 bg-blue-100 text-blue-800 text-sm rounded font-medium">
              {min} → {max}
            </span>
          </div>
        );
      } else if (min !== undefined) {
        return (
          <span className="px-3 py-1 bg-blue-100 text-blue-800 text-sm rounded font-medium">
            From {min}
          </span>
        );
      } else if (max !== undefined) {
        return (
          <span className="px-3 py-1 bg-blue-100 text-blue-800 text-sm rounded font-medium">
            To {max}
          </span>
        );
      }
    }
    
    console.log('Invalid interval structure:', value);
    return <span className="text-gray-400">Invalid interval</span>;
  }

  // Обробка решти типів
  if (value === null || value === undefined) {
    return <span className="text-gray-400">—</span>;
  }

  // Форматування великих чисел для Real
  if (dataType === DataType.Real && typeof value === 'number') {
    if (value === 0) return <span className="text-gray-900">0</span>;
    if (Math.abs(value) > 1e10 || (Math.abs(value) < 1e-6 && value !== 0)) {
      return <span className="text-gray-900 font-mono text-sm">{value.toExponential(3)}</span>;
    }
    return <span className="text-gray-900">{value.toLocaleString()}</span>;
  }

  // Для інших типів
  return <span className="text-gray-900">{String(value)}</span>;
}