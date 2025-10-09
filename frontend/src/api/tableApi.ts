import { API_BASE_URL, type ApiResponse, type Column, getHeaders } from "./types";

export class TableApi {
  static async getTable(tableName: string): Promise<ApiResponse> {
    const response = await fetch(`${API_BASE_URL}/table/${encodeURIComponent(tableName)}`, {
      headers: getHeaders(),
    });
    return response.json();
  }

  static async create(tableName: string, columns: Column[]): Promise<ApiResponse> {
    const response = await fetch(`${API_BASE_URL}/table/create`, {
      method: 'POST',
      headers: getHeaders(),
      body: JSON.stringify({ tableName, columns })
    });
    return response.json();
  }

  static async delete(tableName: string): Promise<ApiResponse> {
    const response = await fetch(`${API_BASE_URL}/table/${encodeURIComponent(tableName)}`, {
      method: 'DELETE',
      headers: getHeaders(),
    });
    return response.json();
  }

  static async getAllRows(tableName: string): Promise<ApiResponse<{ rows: any[] }>> {
    const response = await fetch(`${API_BASE_URL}/table/${encodeURIComponent(tableName)}/rows`,{
      headers: getHeaders(),
    });
    return response.json();
  }

  static async getRow(tableName: string, rowIndex: number): Promise<ApiResponse> {
    const response = await fetch(
      `${API_BASE_URL}/table/${encodeURIComponent(tableName)}/rows/${rowIndex}`, {
        headers: getHeaders(),
      }
    );
    return response.json();
  }

  static async addRow(
    tableName: string, 
    data: Record<string, any>, 
    files?: Record<string, File>
  ): Promise<ApiResponse> {
    const formData = new FormData();
    
    // Додаємо дані як JSON
    formData.append('Data', JSON.stringify(data));

    // Додаємо файли
    if (files) {
      for (const [columnName, file] of Object.entries(files)) {
        formData.append(columnName, file);
      }
    }

    const response = await fetch(
      `${API_BASE_URL}/table/${encodeURIComponent(tableName)}/rows`,
      {
        method: 'POST',
        headers: {
          'X-Session-Id': getHeaders()['X-Session-Id'],
        },
        body: formData
      }
    );
    return response.json();
  }

  static async updateRow(
    tableName: string, 
    rowIndex: number,
    data: Record<string, any>, 
    files?: Record<string, File>
  ): Promise<ApiResponse> {
    const formData = new FormData();
    formData.append('Data', JSON.stringify(data));

    if (files) {
      for (const [columnName, file] of Object.entries(files)) {
        formData.append(columnName, file);
      }
    }

    const response = await fetch(
      `${API_BASE_URL}/table/${encodeURIComponent(tableName)}/rows/${rowIndex}`,
      {
        method: 'PUT',
        headers: {
          'X-Session-Id': getHeaders()['X-Session-Id'],
        },
        body: formData
      }
    );
    return response.json();
  }

  static async deleteRow(tableName: string, rowIndex: number): Promise<ApiResponse> {
    const response = await fetch(
      `${API_BASE_URL}/table/${encodeURIComponent(tableName)}/rows/${rowIndex}`,
      {
        method: 'DELETE',
        headers: getHeaders(),
      }
    );
    return response.json();
  }

  static async getColumns(tableName: string): Promise<ApiResponse<{ columns: Column[] }>> {
    const response = await fetch(
      `${API_BASE_URL}/table/${encodeURIComponent(tableName)}/columns`, {
        headers: getHeaders(),
      }
    );
    return response.json();
  }

  static async renameColumn(
    tableName: string, 
    oldName: string, 
    newName: string
  ): Promise<ApiResponse> {
    const response = await fetch(
      `${API_BASE_URL}/table/${encodeURIComponent(tableName)}/columns/rename`,
      {
        method: 'PUT',
        headers: getHeaders(),
        body: JSON.stringify({ oldName, newName })
      }
    );
    return response.json();
  }

  static async reorderColumns(
    tableName: string, 
    newOrder: string[]
  ): Promise<ApiResponse> {
    const response = await fetch(
      `${API_BASE_URL}/table/${encodeURIComponent(tableName)}/columns/reorder`,
      {
        method: 'PUT',
        headers: getHeaders(),
        body: JSON.stringify({ newOrder })
      }
    );
    return response.json();
  }
}