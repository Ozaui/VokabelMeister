import React from "react";
import { useNavigate } from "react-router-dom";

const Header: React.FC = () => {
  const navigate = useNavigate();
  return (
    <header className="flex items-center justify-between bg-white py-4 px-6 md:px-12">
      <button
        onClick={() => navigate("/")}
        className="flex items-center cursor-pointer"
      >
        <a className="text-xl font-bold text-orange-500">Vokabel Meister</a>
      </button>

      <nav className="hidden md:flex space-x-8">
        <a
          onClick={() => navigate("/courses")}
          className="text-gray-600 transition-colors duration-300 hover:text-orange-500 cursor-pointer"
        >
          Courses
        </a>
        <a
          onClick={() => navigate("/about")}
          className="text-gray-600 transition-colors duration-300 hover:text-orange-500 cursor-pointer"
        >
          About
        </a>
      </nav>

      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate("/login")}
          className="text-gray-600 transition-colors duration-300 hover:text-orange-500 cursor-pointer"
        >
          Log In
        </button>
        <button
          onClick={() => navigate("/register")}
          className="rounded-full bg-orange-500 px-6 py-2 font-bold text-white transition-colors duration-300 hover:bg-orange-600 cursor-pointer"
        >
          Sign Up
        </button>
      </div>
    </header>
  );
};

export default Header;
