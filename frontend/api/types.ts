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

export interface IntegerIntervalValue {
  min: string;
  max: string;
}

export interface ApiResponse<T = any> {
  data?: T;
  error?: string;
  errors?: string[];
  message?: string;
}