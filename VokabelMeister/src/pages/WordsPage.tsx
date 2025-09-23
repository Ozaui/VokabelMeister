import React from "react";
import { useDispatch } from "react-redux";
import type { AppDispatch } from "../store/store";
import { useNavigate } from "react-router-dom";
import { logoutUser } from "../features/auth/authSlice";

const WordsPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();

  const handleLogout = () => {
    dispatch(logoutUser());
    navigate("/login");
  };
  return (
    <div>
      <button onClick={handleLogout}>Log Out</button>
    </div>
  );
};

export default WordsPage;
