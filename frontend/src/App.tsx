import { Database, Home, TableIcon } from "lucide-react";
import { useState, useEffect } from "react";
import { DatabaseApi } from "./api/databaseApi";
import { DatabasePage } from "./pages/DatabasePage";
import { HomePage } from "./pages/HomePage";
import { TablesPage } from "./pages/TablesPage";
import { TableViewPage } from "./pages/TableViewPage";

export default function App() {
  const [currentPage, setCurrentPage] = useState('home');
  const [databaseInfo, setDatabaseInfo] = useState<any>(null);
  const [tables, setTables] = useState<string[]>([]);
  const [selectedTable, setSelectedTable] = useState<string | null>(null);

  const loadDatabaseInfo = async () => {
    try {
      const info = await DatabaseApi.getInfo();
      if (!info.error) {
        setDatabaseInfo(info);
      }
    } catch (error) {
      setDatabaseInfo(null);
    }
  };

  const loadTables = async () => {
    const response : any = await DatabaseApi.getTables();
    if (response.tables) {
      setTables(response.tables);
    }
  };

  useEffect(() => {
    loadDatabaseInfo();
    loadTables();
  }, []);

  const renderPage = () => {
    switch (currentPage) {
      case 'home':
        return <HomePage 
          databaseInfo={databaseInfo} 
          onRefresh={loadDatabaseInfo}
          onNavigate={setCurrentPage}
        />;
      case 'database':
        return <DatabasePage 
          onDatabaseChange={() => {
            loadDatabaseInfo();
            loadTables();
          }}
        />;
      case 'tables':
        return <TablesPage 
          tables={tables}
          onTablesChange={loadTables}
          onSelectTable={(table : any) => {
            setSelectedTable(table);
            setCurrentPage('table-view');
          }}
        />;
      case 'table-view':
        return selectedTable ? (
          <TableViewPage 
            tableName={selectedTable}
            onBack={() => setCurrentPage('tables')}
          />
        ) : null;
      default:
        return <HomePage 
          databaseInfo={databaseInfo} 
          onRefresh={loadDatabaseInfo}
          onNavigate={setCurrentPage}
        />;
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-blue-600 text-gray-900 p-4 shadow-lg">
        <div className="w-full flex items-center justify-between">
          <h2 className="text-2xl text-white font-bold flex items-center gap-2">
            <Database size={28} />
            Database Manager
          </h2>
          <div className="flex gap-2">
            <button
              onClick={() => setCurrentPage('home')}
              className={`px-4 py-2 rounded flex items-center gap-2 ${currentPage === 'home' ? 'bg-blue-700' : 'hover:bg-blue-500'}`}
            >
              <Home size={18} />
              Home
            </button>
            <button
              onClick={() => setCurrentPage('database')}
              className={`px-4 py-2 rounded flex items-center gap-2 ${currentPage === 'database' ? 'bg-blue-700' : 'hover:bg-blue-500'}`}
            >
              <Database size={18} />
              Database
            </button>
            <button
              onClick={() => setCurrentPage('tables')}
              className={`px-4 py-2 rounded flex items-center gap-2 ${currentPage === 'tables' ? 'bg-blue-700' : 'hover:bg-blue-500'}`}
            >
              <TableIcon size={18} />
              Tables
            </button>
          </div>
        </div>
      </nav>
      <main className="w-screen p-6">
        {renderPage()}
      </main>
    </div>
  );
}