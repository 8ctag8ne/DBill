// src/types/models.ts
export interface Database {
  name: string;
  tableCount: number;
  tables: Table[];
}

export interface Table {
  name: string;
  columnCount: number;
  rowCount: number;
}

export interface Column {
  name: string;
  type: string;
}