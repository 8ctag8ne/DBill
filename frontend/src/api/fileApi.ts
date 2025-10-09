import { API_BASE_URL, type ApiResponse, getHeaders } from "./types";

export class FileApi {
  static async upload(file: File): Promise<ApiResponse<{ storagePath: string }>> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch(`${API_BASE_URL}/file/upload`, {
      method: 'POST',
      headers: getHeaders(),
      body: formData
    });
    return response.json();
  }

  static async download(storagePath: string): Promise<Blob> {
    const response = await fetch(
      `${API_BASE_URL}/file/download?storagePath=${encodeURIComponent(storagePath)}`,{
        headers: getHeaders(),
      }
    );
    return response.blob();
  }

  static async delete(storagePath: string): Promise<ApiResponse> {
    const response = await fetch(
      `${API_BASE_URL}/file?storagePath=${encodeURIComponent(storagePath)}`,
      {
        method: 'DELETE',
        headers: getHeaders(),
      }
    );
    return response.json();
  }

  static async cleanup(): Promise<ApiResponse> {
    const response = await fetch(`${API_BASE_URL}/file/cleanup`, {
      method: 'POST',
      headers: getHeaders(),
    });
    return response.json();
  }

  static getDownloadUrl(storagePath: string): string {
    return `${API_BASE_URL}/file/download?storagePath=${encodeURIComponent(storagePath)}`;
  }
}