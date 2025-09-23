import React from "react";
import { Route, Routes } from "react-router-dom";
import RegisterPage from "../pages/RegisterPage";
import LoginPage from "../pages/LoginPage";
import WordsPage from "../pages/WordsPage";
import PublicRoute from "./PublicRoute";
import PrivateRoute from "./PrivateRoute";
import NotFound from "../pages/NotFound";

const AppRoutes: React.FC = () => {
  return (
    <div>
      <Routes>
        {/* 404 Route */}
        <Route path="*" element={<NotFound />} />

        {/* Public Routes */}
        <Route
          path="/login"
          element={
            <PublicRoute>
              <LoginPage />
            </PublicRoute>
          }
        />
        <Route
          path="/register"
          element={
            <PublicRoute>
              <RegisterPage />
            </PublicRoute>
          }
        />

        {/* Private Routes */}
        <Route
          path="/words"
          element={
            <PrivateRoute>
              <WordsPage />
            </PrivateRoute>
          }
        />
      </Routes>
    </div>
  );
};

export default AppRoutes;
