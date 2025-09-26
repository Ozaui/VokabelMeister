import React from "react";
import { useNavigate } from "react-router-dom";

const Body: React.FC = () => {
  const navigate = useNavigate();
  return (
    <main className="flex justify-center items-center py-16 px-4 md:px-0">
      <div
        className="relative w-full max-w-5xl rounded-3xl overflow-hidden shadow-2xl"
        style={{
          backgroundImage: "url(/path/to/your/background-image.jpg)",
          backgroundSize: "cover",
          backgroundPosition: "center",
        }}
      >
        <div className="absolute inset-0 bg-black opacity-35"></div>

        <div className="relative z-10 text-center py-20 px-6 md:py-32">
          <h1 className="text-4xl sm:text-5xl md:text-6xl font-bold text-white mb-4 leading-tight">
            Unlock German Vocabulary with <br className="sm:hidden" />
            <h1 className="text-orange-500"> Vokabel Meister</h1>
          </h1>

          <p className="text-lg md:text-xl text-gray-200 mb-8 max-w-2xl mx-auto">
            Learn German words easily with Vokabel Meister. Expand your
            vocabulary and track your progress. Start learning German words
            today!
          </p>

          <div className="flex flex-col sm:flex-row justify-center items-center space-y-4 sm:space-y-0 sm:space-x-4">
            <button
              onClick={() => navigate("/register")}
              className="bg-orange-500 text-white font-semibold py-3 px-8 rounded-full hover:bg-orange-700 transition-colors duration-300 w-full sm:w-auto cursor-pointer"
            >
              Get Started for Free
            </button>
            <button
              onClick={() => navigate("/courses")}
              className="bg-white text-gray-800 font-semibold py-3 px-8 rounded-full hover:bg-gray-200 transition-colors duration-300 w-full sm:w-auto cursor-pointer"
            >
              Explore Courses
            </button>
          </div>
        </div>
      </div>
    </main>
  );
};

export default Body;
