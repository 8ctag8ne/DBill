//src/api/databaseApi.ts
import { API_BASE_URL, type ApiResponse, getHeaders, getSessionId } from "./types";

export class DatabaseApi {
  static async create(name: string): Promise<ApiResponse> {
    const response = await fetch(`${API_BASE_URL}/database/create`, {
      method: 'POST',
      headers: getHeaders(),
      body: JSON.stringify({ name })
    });
    return response.json();
  }

  static async load(file: File): Promise<ApiResponse> {
    const formData = new FormData();
    formData.append('file', file);
    
    const headers = {
      'X-Session-Id': getSessionId(),
    };
    
    const response = await fetch(`${API_BASE_URL}/database/load`, {
      method: 'POST',
      headers: headers,
      body: formData
    });
    return response.json();
  }

  static async download(): Promise<Response> {
    const response = await fetch(`${API_BASE_URL}/database/download`, {
      method: 'POST',
      headers: {
        'X-Session-Id': getHeaders()['X-Session-Id'],
      },
    });

    if (!response.ok) {
      // Якщо сервер повернув помилку у JSON
      if (response.headers.get('content-type')?.includes('application/json')) {
        const error = await response.json();
        throw new Error(error.error || 'Failed to download database');
      } else {
        throw new Error('Failed to download database');
      }
    }

    return response;
  }

  static async save(filePath: string): Promise<ApiResponse> {
    const response = await fetch(`${API_BASE_URL}/database/save`, {
      method: 'POST',
      headers: getHeaders(),
      body: JSON.stringify({ filePath })
    });
    return response.json();
  }

  static async getInfo(): Promise<ApiResponse> {
    const response = await fetch(`${API_BASE_URL}/database/info`, {
      headers: getHeaders()
    });
    return response.json();
  }

  static async getTables(): Promise<ApiResponse<{ tables: string[] }>> {
    const response = await fetch(`${API_BASE_URL}/database/tables`,{
      headers: getHeaders()
    });
    return response.json();
  }

  static async getStatistics(): Promise<ApiResponse> {
    const response = await fetch(`${API_BASE_URL}/database/statistics`, {
      headers: getHeaders()
    });
    return response.json();
  }

  static async validate(): Promise<ApiResponse<{ isValid: boolean; errors: string[] }>> {
    const response = await fetch(`${API_BASE_URL}/database/validate`, {
      headers: getHeaders()
    });
    return response.json();
  }

  static async close(): Promise<ApiResponse> {
    const response = await fetch(`${API_BASE_URL}/database/close`, {
      method: 'POST',
      headers: getHeaders(),
    });
    return response.json();
  }
}