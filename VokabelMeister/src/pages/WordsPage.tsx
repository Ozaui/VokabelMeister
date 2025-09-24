import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import type { AppDispatch, RootState } from "../store/store";
import { useNavigate } from "react-router-dom";
import { logoutUser } from "../features/auth/authSlice";
import { fetchWordsThunk } from "../features/word/wordThunk";

const WordsPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { words, loading, error } = useSelector(
    (state: RootState) => state.words
  );
  const user = useSelector((state: RootState) => state.auth.user);

  useEffect(() => {
    if (user) dispatch(fetchWordsThunk());
  }, [user, dispatch]);

  const handleLogout = () => {
    dispatch(logoutUser());
    navigate("/login");
  };

  if (loading) return <h1>loading...</h1>;
  if (error) return <h1>Error: {error}</h1>;
  return (
    <div>
      <h1>{user?.level} Level Words</h1>
      <ul>
        {words.map((word) => (
          <li key={word._id}>
            <strong>
              {word.german} = {word.turkish}
            </strong>
            <p>{word.sampleSentence && word.sampleSentence}</p>
          </li>
        ))}
      </ul>

      <button onClick={handleLogout}>Log Out</button>
    </div>
  );
};

export default WordsPage;
