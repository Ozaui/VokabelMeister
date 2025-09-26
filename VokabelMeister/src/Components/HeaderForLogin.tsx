import React from "react";
import { useNavigate } from "react-router-dom";

const HeaderForLogin: React.FC = () => {
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
    </header>
  );
};

export default HeaderForLogin;
