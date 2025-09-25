import React, { useEffect } from "react";
import AppRoutes from "./routes/AppRoutes";
import { checkTokenExpiration } from "./API/auth/authApi";

const App: React.FC = () => {
  useEffect(() => {
    checkTokenExpiration(); // sayfa açılır açılmaz kontrol
  }, []);
  return (
    <div>
      <AppRoutes />
    </div>
  );
};

export default App;
