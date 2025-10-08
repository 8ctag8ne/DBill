import { API_BASE_URL, ApiResponse } from "./types";

export class DatabaseApi {
  static async create(name: string): Promise<ApiResponse> {
    const response = await fetch(`${API_BASE_URL}/database/create`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name })
    });
    return response.json();
  }

  static async load(file: File): Promise<ApiResponse> {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await fetch(`${API_BASE_URL}/database/load`, {
      method: 'POST',
      body: formData
    });
    return response.json();
  }

  static async save(filePath: string): Promise<ApiResponse> {
    const response = await fetch(`${API_BASE_URL}/database/save`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ filePath })
    });
    return response.json();
  }

  static async getInfo(): Promise<ApiResponse> {
    const response = await fetch(`${API_BASE_URL}/database/info`);
    return response.json();
  }

  static async getTables(): Promise<ApiResponse<{ tables: string[] }>> {
    const response = await fetch(`${API_BASE_URL}/database/tables`);
    return response.json();
  }

  static async getStatistics(): Promise<ApiResponse> {
    const response = await fetch(`${API_BASE_URL}/database/statistics`);
    return response.json();
  }

  static async validate(): Promise<ApiResponse<{ isValid: boolean; errors: string[] }>> {
    const response = await fetch(`${API_BASE_URL}/database/validate`);
    return response.json();
  }

  static async close(): Promise<ApiResponse> {
    const response = await fetch(`${API_BASE_URL}/database/close`, {
      method: 'POST'
    });
    return response.json();
  }
}