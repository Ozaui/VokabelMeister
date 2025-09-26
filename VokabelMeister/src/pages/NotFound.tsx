import React from "react";
import { useNavigate } from "react-router-dom";

const NotFound: React.FC = () => {
  const navigate = useNavigate();

  return (
    <div className="flex min-h-[80vh] flex-col items-center justify-center bg-gray-50 p-6 text-center">
      <div className="max-w-xl">
        <h1 className="text-9xl font-extrabold text-orange-500 mb-4">404</h1>

        <h2 className="text-4xl font-bold text-gray-800 mb-4">
          Oops! Page Not Found
        </h2>

        <p className="text-lg text-gray-600 mb-8">
          We can't seem to find the page you're looking for. It might have been
          moved or deleted. Don't worry, let's get you back on track!
        </p>

        <button
          onClick={() => navigate("/")}
          className="bg-orange-500 text-white font-bold py-3 px-8 rounded-full hover:bg-orange-600 transition-colors duration-300 shadow-lg"
        >
          Go to Homepage
        </button>

        <button
          onClick={() => navigate("/courses")}
          className="ml-4 text-gray-700 hover:text-orange-500 font-semibold py-3 transition-colors duration-300"
        >
          Explore Courses
        </button>
      </div>
    </div>
  );
};

export default NotFound;
