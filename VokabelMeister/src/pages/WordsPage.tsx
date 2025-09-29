import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import type { AppDispatch, RootState } from "../store/store";
import { useNavigate } from "react-router-dom";
import { logoutUser } from "../features/auth/authSlice";
import {
  fetchWordsThunk,
  markWordAsLearnedThunk,
} from "../features/word/wordThunk";
import AddWordComponent from "../Components/AddWordComponent";
import {
  FaBookReader,
  FaCheckCircle,
  FaPlus,
  FaSignOutAlt,
  FaTimesCircle,
  FaEye,
  FaSearch,
} from "react-icons/fa";

const WordsPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const {
    defaultWords,
    userWords,
    learnedWords,
    fetchLoading,
    addLoading,
    error,
  } = useSelector((state: RootState) => state.words);

  const user = useSelector((state: RootState) => state.auth.user);

  const [addNewWord, setAddNewWord] = useState(false);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [showTranslation, setShowTranslation] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");

  useEffect(() => {
    if (user) dispatch(fetchWordsThunk());
  }, [user, dispatch]);

  const allWords = [...defaultWords, ...userWords].filter(
    (word) => !learnedWords.some((learned) => learned._id === word._id)
  );

  useEffect(() => {
    if (currentIndex >= allWords.length) {
      setCurrentIndex(0);
    }
  }, [allWords, currentIndex]);

  useEffect(() => {
    setShowTranslation(false);
  }, [currentIndex]);

  const handleLogout = () => {
    dispatch(logoutUser());
    navigate("/login");
  };

  const handleNextWord = () => {
    setCurrentIndex((prev) => (prev + 1 < allWords.length ? prev + 1 : 0));
  };

  const handleLearned = (wordId: string) => {
    dispatch(markWordAsLearnedThunk(wordId));
    setShowTranslation(false);
  };

  const handleSearch = () => {
    const index = allWords.findIndex(
      (word) =>
        word.german.toLowerCase() === searchTerm.toLowerCase() ||
        word.turkish.toLowerCase() === searchTerm.toLowerCase()
    );

    if (index !== -1) {
      setCurrentIndex(index);
      setShowTranslation(true);
    } else {
      alert("Kelime bulunamadı!");
    }
  };

  if (fetchLoading || addLoading || !user)
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50">
        <h1 className="text-3xl font-semibold text-orange-500">
          Loading words...
        </h1>
      </div>
    );

  if (error)
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50">
        <h1 className="text-3xl font-semibold text-red-600">Error: {error}</h1>
      </div>
    );

  if (allWords.length === 0) {
    return (
      <div className="flex min-h-screen flex-col items-center justify-center bg-gray-50 p-6 text-center">
        {learnedWords.length > 0 ? (
          <FaCheckCircle className="text-green-500 w-16 h-16 mb-4" />
        ) : (
          <FaBookReader className="text-orange-500 w-16 h-16 mb-4" />
        )}
        <h1 className="text-3xl font-bold text-gray-800 mb-4">
          {learnedWords.length > 0
            ? "Congratulations! You've learned all available words!"
            : `Welcome, ${
                user?.name || "User"
              }! Start your vocabulary journey.`}
        </h1>
        {learnedWords.length > 0 && (
          <p className="text-gray-600 mb-8">
            Ready to add new vocabulary or review your learned words?
          </p>
        )}

        {!addNewWord ? (
          <button
            onClick={() => setAddNewWord(true)}
            className="bg-orange-500 text-white font-bold py-3 px-6 rounded-full hover:bg-orange-600 transition duration-300 flex items-center shadow-lg mb-4"
          >
            <FaPlus className="mr-2" />
            {learnedWords.length > 0 ? "Add New Word" : "Add Your First Word"}
          </button>
        ) : (
          <div className="max-w-lg w-full mb-4">
            <AddWordComponent onCancel={() => setAddNewWord(false)} />
          </div>
        )}

        <button
          onClick={handleLogout}
          className="mt-4 text-gray-600 hover:text-orange-500 transition-colors flex items-center"
        >
          <FaSignOutAlt className="mr-2" /> Log Out
        </button>
      </div>
    );
  }

  const currentWord = allWords[currentIndex];

  return (
    <div className="min-h-screen bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl font-extrabold text-gray-900">
            <span className="text-orange-500">{user?.level}</span> Level
            Vocabulary
          </h1>

          <div className="flex space-x-2">
            <input
              type="text"
              placeholder="Search..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="border p-2 rounded"
            />
            <button
              onClick={handleSearch}
              className="bg-orange-500 text-white px-4 py-2 rounded flex items-center"
            >
              <FaSearch className="mr-2" /> Ara
            </button>
            <button
              onClick={handleLogout}
              className="text-gray-600 hover:text-orange-500 font-semibold transition-colors flex items-center ml-4"
            >
              <FaSignOutAlt className="mr-2" /> Log Out
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-2xl p-8 border-t-8 border-orange-500 flex flex-col items-center text-center">
            <h2 className="text-xl font-semibold text-gray-600 mb-6">
              Word {currentIndex + 1} of {allWords.length}
            </h2>

            <strong className="text-4xl sm:text-6xl font-extrabold text-gray-800 mb-4 break-words text-center">
              {currentWord.german}
            </strong>

            {!showTranslation ? (
              <button
                onClick={() => setShowTranslation(true)}
                className="bg-orange-100 text-orange-700 font-bold py-3 px-8 rounded-full hover:bg-orange-200 transition-colors duration-300 flex items-center shadow-md mb-8"
              >
                <FaEye className="mr-2" /> Show Translation
              </button>
            ) : (
              <p
                className={`text-3xl text-orange-500 font-medium mb-8 transition-all duration-500 ${
                  showTranslation ? "filter-none" : "filter blur-md"
                }`}
              >
                = {currentWord.turkish}
              </p>
            )}

            {currentWord.sampleSentence && showTranslation && (
              <p className="text-lg italic text-gray-700 max-w-lg mb-10 p-4 bg-gray-50 rounded-lg border-l-4 border-gray-200">
                "{currentWord.sampleSentence}"
              </p>
            )}

            {showTranslation && (
              <div className="flex space-x-6">
                <button
                  onClick={() => handleLearned(currentWord._id)}
                  className="bg-green-500 text-white font-bold py-3 px-8 rounded-full hover:bg-green-600 transition-colors duration-300 flex items-center shadow-md"
                >
                  <FaCheckCircle className="mr-2" /> Learned It
                </button>
                <button
                  onClick={handleNextWord}
                  className="bg-gray-400 text-white font-bold py-3 px-8 rounded-full hover:bg-gray-500 transition-colors duration-300 flex items-center shadow-md"
                >
                  <FaTimesCircle className="mr-2" /> Not Yet
                </button>
              </div>
            )}

            {!showTranslation && (
              <button
                onClick={handleNextWord}
                className="bg-gray-400 text-white font-bold py-3 px-8 rounded-full hover:bg-gray-500 transition-colors duration-300 flex items-center shadow-md"
              >
                <FaTimesCircle className="mr-2" /> Skip Word
              </button>
            )}
          </div>

          <div className="lg:col-span-1 space-y-8">
            <div className="bg-white rounded-xl shadow-lg p-6">
              <h3 className="text-2xl font-bold text-gray-800 mb-4">
                {addNewWord ? "Add Custom Word" : "Your Vocabulary Tools"}
              </h3>

              {!addNewWord ? (
                <button
                  onClick={() => setAddNewWord(true)}
                  className="w-full bg-orange-500 text-white font-bold py-3 px-6 rounded-full hover:bg-orange-600 transition duration-300 flex items-center justify-center shadow-md"
                >
                  <FaPlus className="mr-2" /> Add New Word
                </button>
              ) : (
                <AddWordComponent onCancel={() => setAddNewWord(false)} />
              )}
            </div>

            <div className="bg-white rounded-xl shadow-lg p-6 max-h-96 overflow-y-auto border-t-4 border-green-500">
              <h3 className="text-2xl font-bold text-gray-800 mb-4 flex items-center">
                <FaCheckCircle className="text-green-500 mr-2" /> Learned Words
                ({learnedWords.length})
              </h3>
              {learnedWords.length === 0 ? (
                <p className="text-gray-500 italic">
                  No words learned yet. Keep practicing!
                </p>
              ) : (
                <ul className="space-y-3">
                  {learnedWords.map((word) => (
                    <li
                      key={word._id}
                      className="border-b pb-2 last:border-b-0"
                    >
                      <strong className="text-gray-800">{word.german}</strong>
                      <span className="text-orange-500 text-sm ml-2">
                        = {word.turkish}
                      </span>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default WordsPage;
