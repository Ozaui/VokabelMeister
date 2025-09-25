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

  useEffect(() => {
    if (user) dispatch(fetchWordsThunk());
  }, [user, dispatch]);

  const handleLogout = () => {
    dispatch(logoutUser());
    navigate("/login");
  };

  const [addNewWord, setAddNewWord] = useState(false);
  const handleNewWord = () => {
    setAddNewWord(true);
  };

  if (fetchLoading || addLoading) return <h1>loading...</h1>;
  if (error) return <h1>Error: {error}</h1>;
  return (
    <div>
      <h1>{user?.level} Level Words</h1>
      <h2>Default Words</h2>
      <ul>
        {defaultWords.map((word) => (
          <li key={word._id}>
            <strong>
              {word.german} = {word.turkish}
            </strong>
            {word.sampleSentence && <p>{word.sampleSentence}</p>}
            <button
              onClick={() => dispatch(markWordAsLearnedThunk(word._id))}
            ></button>
          </li>
        ))}
      </ul>

      <h2>My Words</h2>
      <ul>
        {userWords.map((word) => (
          <li key={word._id}>
            <strong>
              {word.german} = {word.turkish}
            </strong>
            {word.sampleSentence && <p>{word.sampleSentence}</p>}
            <button
              onClick={() => dispatch(markWordAsLearnedThunk(word._id))}
            ></button>
          </li>
        ))}
      </ul>

      <h2>Learned Words</h2>
      <ul>
        {learnedWords.map((word) => (
          <li key={word._id}>
            <strong>
              {word.german} = {word.turkish}
            </strong>
            {word.sampleSentence && <p>{word.sampleSentence}</p>}
          </li>
        ))}
      </ul>

      {!addNewWord ? (
        <button onClick={handleNewWord}>Add New Word</button>
      ) : (
        <AddWordComponent />
      )}

      <button onClick={handleLogout}>Log Out</button>
    </div>
  );
};

export default WordsPage;
