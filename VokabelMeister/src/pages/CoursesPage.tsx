import React from "react";
import Header from "../Components/Header";
import Footer from "../Components/Footer";
import { FaGraduationCap, FaBookOpen } from "react-icons/fa";
import { useNavigate } from "react-router-dom";

const courseLevels = [
  {
    level: "A1",
    title: "Beginner: First Steps in German",
    description:
      "Learn fundamental vocabulary for basic communication, introductions, and everyday life. Perfect for absolute beginners.",
    color: "bg-green-100",
    textColor: "text-green-700",
  },
  {
    level: "A2",
    title: "Elementary: Expanding Horizons",
    description:
      "Build on your foundation with vocabulary for describing experiences, environments, and routine tasks. You'll gain simple conversational fluency.",
    color: "bg-blue-100",
    textColor: "text-blue-700",
  },
  {
    level: "B1",
    title: "Intermediate: Independent Language Use",
    description:
      "Acquire vocabulary necessary to handle most situations encountered while traveling and express opinions on various topics clearly.",
    color: "bg-yellow-100",
    textColor: "text-yellow-700",
  },
  {
    level: "B2",
    title: "Upper Intermediate: Fluent and Detailed",
    description:
      "Master complex vocabulary to understand the main ideas of complex texts and participate in technical discussions in your field of specialization.",
    color: "bg-purple-100",
    textColor: "text-purple-700",
  },
  {
    level: "C1",
    title: "Advanced: Highly Proficient",
    description:
      "Attain a broad vocabulary spectrum to express yourself fluently and spontaneously, recognizing implied meaning in demanding texts.",
    color: "bg-red-100",
    textColor: "text-red-700",
  },
  {
    level: "C2",
    title: "Proficiency: Near-Native Mastery",
    description:
      "Develop vocabulary for almost every situation, understanding virtually everything heard or read with nuanced and sophisticated expression.",
    color: "bg-indigo-100",
    textColor: "text-indigo-700",
  },
];

const CoursesPage: React.FC = () => {
  const navigate = useNavigate();

  const handleStartCourse = () => {
    navigate("/register");
  };

  return (
    <div>
      <Header />

      <div className="min-h-screen bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-16">
            <h1 className="text-5xl font-extrabold text-gray-900 mb-4">
              Our <span className="text-orange-500">Vocabulary Courses</span>
            </h1>
            <p className="text-xl text-gray-600 max-w-3xl mx-auto">
              Master German vocabulary from beginner (A1) to near-native fluency
              (C2), structured according to official European frameworks.
            </p>
          </div>

          <div className="bg-white rounded-xl shadow-xl p-8 mb-12 border-t-4 border-orange-500 flex flex-col md:flex-row items-center justify-center space-y-4 md:space-y-0 md:space-x-8">
            <FaBookOpen className="text-orange-500 w-10 h-10 flex-shrink-0" />
            <div className="text-center md:text-left">
              <h3 className="text-2xl font-bold text-gray-800">
                Official and Reliable Source
              </h3>
              <p className="mt-1 text-lg text-gray-600">
                All vocabulary sets are meticulously sourced from the core
                textbooks and materials recommended by the{" "}
                <strong> Goethe-Institut</strong>. This guarantees that you
                learn the exact words required for official exams and real-world
                competence.
              </p>
            </div>
            <FaGraduationCap className="text-orange-500 w-10 h-10 flex-shrink-0 hidden md:block" />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {courseLevels.map((course) => (
              <div
                key={course.level}
                className="bg-white rounded-xl shadow-lg hover:shadow-2xl transition duration-300 overflow-hidden"
              >
                <div className={`${course.color} p-4`}>
                  <p
                    className={`text-sm font-semibold uppercase ${course.textColor}`}
                  >
                    CEFR Level
                  </p>
                  <h2
                    className={`text-4xl font-extrabold ${course.textColor} mt-1`}
                  >
                    {course.level}
                  </h2>
                </div>

                <div className="p-6">
                  <h3 className="text-xl font-bold text-gray-800 mb-3">
                    {course.title}
                  </h3>
                  <p className="text-gray-600 min-h-[72px]">
                    {course.description}
                  </p>

                  <button
                    onClick={handleStartCourse}
                    className="mt-5 w-full py-2 rounded-full border border-orange-500 text-orange-500 font-semibold hover:bg-orange-50 transition-colors duration-200 cursor-pointer"
                  >
                    Start {course.level} Course
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      <Footer />
    </div>
  );
};

export default CoursesPage;
