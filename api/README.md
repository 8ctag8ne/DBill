# Database Management API

REST API для управління табличними базами даних з підтримкою типів: Integer, Real, Char, String, TextFile, IntegerInterval.

## Запуск проекту

```bash
dotnet run
```

API буде доступне за адресою: `https://localhost:5001` (або `http://localhost:5000`)

Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Database Operations

#### Створити базу даних
```http
POST /api/database/create
Content-Type: application/json

{
  "name": "MyDatabase"
}
```

#### Завантажити базу даних
```http
POST /api/database/load
Content-Type: multipart/form-data

file: [database.json file]
```

#### Зберегти базу даних
```http
POST /api/database/save
Content-Type: application/json

{
  "filePath": "database.json"
}
```

#### Отримати інформацію про базу даних
```http
GET /api/database/info
```

#### Отримати список таблиць
```http
GET /api/database/tables
```

#### Отримати статистику
```http
GET /api/database/statistics
```

#### Валідувати базу даних
```http
GET /api/database/validate
```

#### Закрити базу даних
```http
POST /api/database/close
```

---

### Table Operations

#### Отримати інформацію про таблицю
```http
GET /api/table/{tableName}
```

#### Створити таблицю
```http
POST /api/table/create
Content-Type: application/json

{
  "tableName": "Students",
  "columns": [
    {
      "name": "Name",
      "type": 3
    },
    {
      "name": "Age",
      "type": 0
    },
    {
      "name": "Grade",
      "type": 1
    }
  ]
}
```

**Типи даних (DataType enum):**
- `0` - Integer
- `1` - Real
- `2` - Char
- `3` - String
- `4` - TextFile
- `5` - IntegerInterval

#### Видалити таблицю
```http
DELETE /api/table/{tableName}
```

#### Отримати всі рядки
```http
GET /api/table/{tableName}/rows
```

#### Отримати рядок за індексом
```http
GET /api/table/{tableName}/rows/{rowIndex}
```

#### Додати рядок
```http
POST /api/table/{tableName}/rows
Content-Type: multipart/form-data

Data: {
  "Name": "John Doe",
  "Age": "20",
  "Grade": "4.5"
}
Files: [file1, file2] // для колонок типу TextFile
```

**Приклад для IntegerInterval:**
```json
{
  "Name": "John Doe",
  "ScoreRange": {
    "Min": "80",
    "Max": "95"
  }
}
```

#### Оновити рядок
```http
PUT /api/table/{tableName}/rows/{rowIndex}
Content-Type: multipart/form-data

Data: { ... }
Files: [ ... ]
```

#### Видалити рядок
```http
DELETE /api/table/{tableName}/rows/{rowIndex}
```

#### Отримати список колонок
```http
GET /api/table/{tableName}/columns
```

#### Перейменувати колонку
```http
PUT /api/table/{tableName}/columns/rename
Content-Type: application/json

{
  "oldName": "OldColumnName",
  "newName": "NewColumnName"
}
```

#### Переставити колонки
```http
PUT /api/table/{tableName}/columns/reorder
Content-Type: application/json

{
  "newOrder": ["Column3", "Column1", "Column2"]
}
```

---

### File Operations

#### Завантажити файл (upload)
```http
POST /api/file/upload
Content-Type: multipart/form-data

file: [binary data]
```

**Response:**
```json
{
  "message": "File uploaded successfully",
  "storagePath": "uploads/guid_filename.txt",
  "fileName": "filename.txt",
  "size": 1024
}
```

#### Скачати файл
```http
GET /api/file/download?storagePath=uploads/guid_filename.txt
```

#### Видалити файл
```http
DELETE /api/file?storagePath=uploads/guid_filename.txt
```

#### Очистити всі файли
```http
POST /api/file/cleanup
```

---

## Приклади використання

### Створення таблиці з різними типами

```javascript
const createTable = async () => {
  const response = await fetch('http://localhost:5000/api/table/create', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      tableName: 'Products',
      columns: [
        { name: 'Name', type: 3 },           // String
        { name: 'Price', type: 1 },          // Real
        { name: 'Quantity', type: 0 },       // Integer
        { name: 'Category', type: 2 },       // Char
        { name: 'Manual', type: 4 },         // TextFile
        { name: 'StockRange', type: 5 }      // IntegerInterval
      ]
    })
  });
  
  return await response.json();
};
```

### Додавання рядка з файлом

```javascript
const addRow = async () => {
  const formData = new FormData();
  
  // Звичайні дані
  const data = {
    Name: 'Laptop',
    Price: '999.99',
    Quantity: '10',
    Category: 'A',
    StockRange: {
      Min: '5',
      Max: '50'
    }
  };
  
  formData.append('Data', JSON.stringify(data));
  
  // Файл для колонки Manual
  const file = document.getElementById('fileInput').files[0];
  formData.append('Manual', file);
  
  const response = await fetch('http://localhost:5000/api/table/Products/rows', {
    method: 'POST',
    body: formData
  });
  
  return await response.json();
};
```

### Завантаження та збереження бази даних

```javascript
// Створити нову базу
await fetch('http://localhost:5000/api/database/create', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ name: 'MyDatabase' })
});

// Зберегти базу
await fetch('http://localhost:5000/api/database/save', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ filePath: 'mydb.json' })
});

// Завантажити базу
await fetch('http://localhost:5000/api/database/load', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ filePath: 'mydb.json' })
});
```

---

## Обробка помилок

API повертає стандартні HTTP коди статусу:

- `200 OK` - Успішна операція
- `400 Bad Request` - Невалідні дані або помилка валідації
- `404 Not Found` - Ресурс не знайдено
- `500 Internal Server Error` - Серверна помилка

Формат відповіді з помилкою:
```json
{
  "error": "Опис помилки",
  "errors": ["Помилка 1", "Помилка 2"]
}
```

---

## Примітки

1. **Файли:** Всі файли зберігаються у директорії `uploads/` з унікальними іменами у форматі `{GUID}_{originalName}`

2. **Тимчасові файли:** При завантаженні бази даних використовується тимчасова директорія `tempFiles/` для ізоляції операцій

3. **Розмір файлів:** Максимальний розмір файлу - 50 MB

4. **Валідація:** Всі операції з даними проходять валідацію перед виконанням

5. **IntegerInterval:** При додаванні/оновленні рядків для IntegerInterval потрібно передавати об'єкт з полями `Min` та `Max`

6. **Імена колонок:** Не можуть містити символи: пробіл, кома, крапка з комою, двокрапка