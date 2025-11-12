// src/components/RowForm.tsx
import { useState, useEffect } from "react";
import { type Column, DataType, type IntegerIntervalFromApi, type IntegerIntervalToApi, type TextFileFromApi } from "../api/types";
import { TableApi } from "../api/tableApi";
import { FileUploader } from "./FileUploader";
import { AlertCircle } from "lucide-react";

// Функція для конвертації рядка в DataType
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

const getDataTypeName = (type: DataType): string => {
  const typeNames = {
    [DataType.Integer]: 'integer',
    [DataType.Real]: 'real number', 
    [DataType.Char]: 'character',
    [DataType.String]: 'text',
    [DataType.TextFile]: 'file',
    [DataType.IntegerInterval]: 'integer interval'
  };
  return typeNames[type] || 'value';
};

interface RowFormProps {
  columns: Column[];
  tableName: string;
  editingRowIndex?: number | null;
  initialData?: Record<string, any>;
  onClose: () => void;
  onSuccess: () => void;
}

export function RowForm({ 
  columns, 
  tableName, 
  editingRowIndex, 
  initialData, 
  onClose, 
  onSuccess 
}: RowFormProps) {
  const [formData, setFormData] = useState<Record<string, any>>({});
  const [files, setFiles] = useState<Record<string, File>>({});
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    const initialFormData: Record<string, any> = {};
    
    console.log('RowForm: initializing with columns:', columns);
    console.log('RowForm: initialData:', initialData);
    
    columns.forEach((column: Column) => {
      const columnType = typeof column.type === 'string' 
        ? stringToDataType(column.type) 
        : column.type;
      
      // Обробка різних типів даних - ТОЧНО як у WPF
      if (columnType === DataType.IntegerInterval) {
        if (initialData && initialData[column.name] !== undefined && initialData[column.name] !== null) {
          const intervalData = initialData[column.name];
          if (typeof intervalData === 'object' && intervalData !== null) {
            // Беремо значення як є - не конвертуємо в числа
            initialFormData[column.name] = {
              min: intervalData.min?.toString() || intervalData.Min?.toString() || '',
              max: intervalData.max?.toString() || intervalData.Max?.toString() || ''
            };
            console.log(`Column ${column.name} (IntegerInterval): converted from data:`, initialFormData[column.name]);
          } else {
            initialFormData[column.name] = { min: '', max: '' };
          }
        } else {
          // Новий інтервал - пусті рядки як у WPF
          initialFormData[column.name] = { min: '', max: '' };
          console.log(`Column ${column.name} (IntegerInterval): set default empty strings`);
        }
      } else if (columnType === DataType.TextFile) {
        if (initialData && initialData[column.name] !== undefined && initialData[column.name] !== null) {
          initialFormData[column.name] = initialData[column.name];
          console.log(`Column ${column.name} (TextFile): got value from initialData:`, initialData[column.name]);
        } else {
          initialFormData[column.name] = null;
          console.log(`Column ${column.name} (TextFile): set default (null)`);
        }
      } else {
        // Решта типів
        if (initialData && initialData[column.name] !== undefined) {
          initialFormData[column.name] = initialData[column.name]?.toString() || '';
          console.log(`Column ${column.name} (${DataType[columnType]}): got value from initialData:`, initialData[column.name]);
        } else {
          initialFormData[column.name] = '';
          console.log(`Column ${column.name} (${DataType[columnType]}): set default empty string`);
        }
      }
    });
    
    console.log('RowForm: final initialFormData:', initialFormData);
    setFormData(initialFormData);
  }, [initialData, columns]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      console.log('RowForm: handleSubmit started');
      console.log('RowForm: formData:', formData);
      console.log('RowForm: files:', files);

      // Підготовка даних для відправки - ТОЧНО як у WPF
      const dataToSend: Record<string, any> = {};
      
      columns.forEach((column: Column) => {
        const columnType = typeof column.type === 'string' 
          ? stringToDataType(column.type) 
          : column.type;
        const value = formData[column.name];
        
        console.log(`Processing column ${column.name} (${DataType[columnType]}): value=`, value);
        
        // Якщо значення undefined - не передаємо його
        if (value === undefined || value === null) {
          if (columnType !== DataType.TextFile) {
            dataToSend[column.name] = value;
          }
          console.log(`  -> value is null/undefined, skipping for TextFile`);
          return;
        }

        switch (columnType) {
          case DataType.IntegerInterval:
            // IntegerInterval: передаємо як об'єкт з РЯДКАМИ - ТОЧНО як у WPF
            if (typeof value === 'object' && value !== null && ('min' in value || 'max' in value)) {
              const toSend = {
                min: Number(value.min) || "", // РЯДКИ, не числа!
                max: Number(value.max) || ""  // РЯДКИ, не числа!
              };
              
              // Перевіряємо, що хоча б одне значення заповнене
              if (toSend.min !== "" || toSend.max !== "") {
                dataToSend[column.name] = toSend;
                console.log(`  -> IntegerInterval sent as strings:`, toSend);
              } else {
                console.log(`  -> IntegerInterval: both values empty, skipping`);
              }
            }
            break;
          
          case DataType.TextFile:
            // TextFile: передаємо через files (не в dataToSend)
            if (!files[column.name]) {
              // Якщо файл не змінювався, передаємо поточний об'єкт як є
              if (value && typeof value === 'object' && 'fileName' in value) {
                dataToSend[column.name] = value;
                console.log(`  -> TextFile (existing):`, value);
              } else {
                console.log(`  -> TextFile: no file selected and no existing file`);
              }
            } else {
              console.log(`  -> TextFile: new file selected, will be sent via FormData`);
            }
            break;
          
          case DataType.Real:
            // Замінюємо кому на крапку, якщо потрібно
            const normalizedValue = value.replace('.', ',');
            dataToSend[column.name] = normalizedValue;
            console.log(`  -> Real: ${value} -> ${dataToSend[column.name]}`);
            break;
          
          default:
            // Решта типів: передаємо як є
            dataToSend[column.name] = value;
            console.log(`  -> ${DataType[columnType]}: sent as-is`);
        }
      });

      console.log('RowForm: final dataToSend:', dataToSend);
      console.log('RowForm: files to upload:', files);

      let response;
      if (editingRowIndex !== null && editingRowIndex !== undefined) {
        console.log(`Calling updateRow for index ${editingRowIndex}`);
        response = await TableApi.updateRow(tableName, editingRowIndex, dataToSend, files);
      } else {
        console.log('Calling addRow');
        response = await TableApi.addRow(tableName, dataToSend, files);
      }

      console.log('RowForm: response from API:', response);

      if (response.error || response.errors) {
        setError(response.error || (Array.isArray(response.errors) ? response.errors.join(', ') : 'Operation failed'));
      } else {
        onSuccess();
      }
    } catch (err) {
      console.error('RowForm: catch error:', err);
      setError('An error occurred while saving the row');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleInputChange = (columnName: string, value: any) => {
    setFormData(prev => ({
      ...prev,
      [columnName]: value
    }));
  };

  const handleFileChange = (columnName: string, file: File | null) => {
    if (file) {
      setFiles(prev => ({ ...prev, [columnName]: file }));
    } else {
      setFiles(prev => {
        const newFiles = { ...prev };
        delete newFiles[columnName];
        return newFiles;
      });
    }
  };

  const handleIntervalChange = (columnName: string, field: 'min' | 'max', value: string) => {
    setFormData(prev => ({
      ...prev,
      [columnName]: {
        ...(prev[columnName] || { min: '', max: '' }),
        [field]: value
      }
    }));
  };

  const getInputType = (column: Column): string => {
    const columnType = typeof column.type === 'string' 
      ? stringToDataType(column.type) 
      : column.type;

    switch (columnType) {
      case DataType.Integer:
        return "number";
      case DataType.Real:
        return "any";
      case DataType.Char:
        return "text";
      case DataType.String:
        return "text";
      default:
        return "text";
    }
  };

  const getInputStep = (column: Column): string | undefined => {
    const columnType = typeof column.type === 'string' 
      ? stringToDataType(column.type) 
      : column.type;

    switch (columnType) {
      case DataType.Integer:
        return "1";
      case DataType.Real:
        return "any";
      default:
        return undefined;
    }
  };

  const renderInput = (column: Column) => {
    const columnType = typeof column.type === 'string' 
      ? stringToDataType(column.type) 
      : column.type;
    const value = formData[column.name];
    
    console.log(`renderInput for ${column.name} (${DataType[columnType]}): value=`, value);

    switch (columnType) {
      case DataType.TextFile:
        console.log(`  -> Rendering TextFile uploader`);
        const currentFile = value && typeof value === 'object' && 'fileName' in value ? value as TextFileFromApi : null;
        return (
          <FileUploader
            onFileChange={(file) => handleFileChange(column.name, file)}
            currentFile={currentFile}
            accept=".txt,.text,text/plain,text/*"
          />
        );

      case DataType.IntegerInterval:
        console.log(`  -> Rendering IntegerInterval input`);
        const intervalValue = value || { min: '', max: '' };
        console.log(`  -> intervalValue:`, intervalValue);
        return (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Minimum Value
                </label>
                <input
                  type="number"
                  step="1"
                  placeholder="e.g., 20"
                  value={intervalValue.min || ''}
                  onChange={(e) => handleIntervalChange(column.name, 'min', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Maximum Value
                </label>
                <input
                  type="number"
                  step="1"
                  placeholder="e.g., 100"
                  value={intervalValue.max || ''}
                  onChange={(e) => handleIntervalChange(column.name, 'max', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>
          </div>
        );

      default:
        console.log(`  -> Rendering default input (${DataType[columnType]})`);
        return (
          <input
            type={getInputType(column)}
            step={getInputStep(column)}
            maxLength={columnType === DataType.Char ? 1 : undefined}
            value={value || ''}
            onChange={(e) => handleInputChange(column.name, e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            placeholder={`Enter ${getDataTypeName(columnType)}...`}
          />
        );
    }
  };

  const isEditing = editingRowIndex !== null && editingRowIndex !== undefined;

  return (
    <div className="fixed inset-0 backdrop-opacity-50 backdrop-blur-sm flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-xl shadow-2xl max-w-2xl w-full max-h-[85vh] overflow-hidden flex flex-col">
        {/* Header */}
        <div className="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-6 text-gray-900">
          <div className="flex justify-between items-start gap-4">
            <div>
              <h3 className="text-2xl font-bold text-white">
                {isEditing ? 'Edit Row' : 'Add New Row'}
              </h3>
              <p className="text-blue-100 mt-1 text-sm">
                {tableName} • {columns.length} columns
              </p>
            </div>
            <button
              onClick={onClose}
              className="text-gray-900 hover:text-blue-200 transition-colors p-1 rounded-full hover:bg-blue-800"
              title="Close"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>

        {/* Error Message */}
        {error && (
          <div className="mx-6 mt-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg flex items-start gap-3">
            <AlertCircle size={20} className="flex-shrink-0 mt-0.5" />
            <span className="font-medium">{error}</span>
          </div>
        )}

        {/* Form */}
        <div className="flex-1 overflow-y-auto p-6">
          <form onSubmit={handleSubmit} className="space-y-5">
            {columns.map((column: Column) => {
              const columnType = typeof column.type === 'string' 
                ? stringToDataType(column.type) 
                : column.type;
              
              return (
                <div key={column.name} className="bg-gray-50 rounded-lg p-4 border border-gray-200 hover:border-gray-300 transition-colors">
                  <label className="block text-sm font-semibold text-gray-800 mb-3">
                    {column.name}
                    <span className="ml-2 text-xs font-normal text-gray-600 bg-white px-2 py-1 rounded border border-gray-200">
                      {DataType[columnType]}
                    </span>
                  </label>
                  {renderInput(column)}
                </div>
              );
            })}

            {/* Buttons */}
            <div className="flex gap-3 pt-4 border-t border-gray-200">
              <button
                type="submit"
                disabled={isSubmitting}
                className="flex-1 bg-blue-600 text-gray-900 px-6 py-3 rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-medium"
              >
                {isSubmitting ? (
                  <span className="flex items-center justify-center gap-2">
                    <svg className="animate-spin h-4 w-4 text-gray-900" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                    </svg>
                    Saving...
                  </span>
                ) : (
                  isEditing ? 'Update Row' : 'Add Row'
                )}
              </button>
              <button
                type="button"
                onClick={onClose}
                disabled={isSubmitting}
                className="flex-1 bg-gray-500 text-gray-900 px-6 py-3 rounded-lg hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 disabled:opacity-50 transition-colors font-medium"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}