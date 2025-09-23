import React, { type JSX } from "react";
import { useSelector } from "react-redux";
import type { RootState } from "../store/store";
import { Navigate } from "react-router-dom";

type PublicRouteProps = { children: JSX.Element };

const PublicRoute: React.FC<PublicRouteProps> = ({ children }) => {
  const { user } = useSelector((state: RootState) => state.auth);
  if (user) return <Navigate to="/words" replace />;
  return children;
};

export default PublicRoute;
