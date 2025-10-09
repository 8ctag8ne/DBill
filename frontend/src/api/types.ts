// src/api/types.ts
export const API_BASE_URL = 'http://localhost:5222/api';

export interface Column {
  name: string;
  type: DataType;
}

export enum DataType {
  Integer = 0,
  Real = 1,
  Char = 2,
  String = 3,
  TextFile = 4,
  IntegerInterval = 5
}

// ========== Типи для отримання з API (як вони приходять) ==========

/** Значення IntegerInterval як повертає API */
export interface IntegerIntervalFromApi {
  min: number;
  max: number;
}

/** Значення TextFile як повертає API */
export interface TextFileFromApi {
  fileName: string;
  size: number;
  mimeType: string;
  storagePath: string;
}

// ========== Типи для відправки на API (як ми передаємо) ==========

/** IntegerInterval для відправки на API */
export interface IntegerIntervalToApi {
  Min: number;
  Max: number;
}

/** TextFile для відправки на API (просто передаємо File через FormData) */
export type TextFileToApi = File;

// ========== Допоміжні типи ==========

export interface FileObject {
  fileName: string;
  size: number;
  mimeType: string;
  storagePath: string;
}

export interface ApiResponse<T = any> {
  data?: T;
  error?: string;
  errors?: string[];
  message?: string;
}

export const getSessionId = (): string => {
  let sessionId = localStorage.getItem('sessionId');
  if (!sessionId) {
    sessionId = crypto.randomUUID();
    localStorage.setItem('sessionId', sessionId);
  }
  return sessionId;
};

export const getHeaders = (additionalHeaders: Record<string, string> = {}) => ({
  'Content-Type': 'application/json',
  'X-Session-Id': getSessionId(),
  ...additionalHeaders,
});

export const stringToDataType = (typeString: string): DataType => {
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

export interface IntegerIntervalPascalCase {
  Min: number | null;
  Max: number | null;
}