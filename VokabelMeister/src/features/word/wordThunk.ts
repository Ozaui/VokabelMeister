import { createAsyncThunk } from "@reduxjs/toolkit";
import type { addWordPayload, Word } from "../../Types/wordTypes";
import { addWordApi, fetchWords } from "../../API/word.ts/wordApi";

export const fetchWordsThunk = createAsyncThunk<Word[]>(
  "words/fetchWords",
  async () => {
    const data = await fetchWords();
    return data;
  }
);

export const addWordThunk = createAsyncThunk<Word, addWordPayload>(
  "words/addWord",
  async (wordData, { rejectWithValue }) => {
    try {
      const newWord = await addWordApi(wordData);
      return newWord;
    } catch (error: unknown) {
      if (error instanceof Error) {
        return rejectWithValue(error.message);
      }
      return rejectWithValue("As unknown error occurred");
    }
  }
);
