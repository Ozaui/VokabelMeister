import React from "react";
import { useNavigate } from "react-router-dom";

const Header: React.FC = () => {
  const navigate = useNavigate();
  return (
    <header className="flex items-center justify-between bg-white py-4 px-6 md:px-12">
      {/* Logo */}
      <div className="flex items-center">
        <span className="text-xl font-bold text-orange-500">
          Vokabel Meister
        </span>
      </div>

      {/* Navigation */}
      <nav className="hidden md:flex space-x-8">
        <a
          href="#"
          className="text-gray-600 transition-colors duration-300 hover:text-orange-500"
        >
          Courses
        </a>
        <a
          href="#"
          className="text-gray-600 transition-colors duration-300 hover:text-orange-500"
        >
          About
        </a>
      </nav>

      {/* Auth Buttons */}
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate("/login")}
          className="text-gray-600 transition-colors duration-300 hover:text-orange-500"
        >
          Log In
        </button>
        <button className="rounded-full bg-orange-500 px-6 py-2 font-bold text-white transition-colors duration-300 hover:bg-orange-600">
          Sign Up
        </button>
      </div>
    </header>
  );
};

export default Header;
