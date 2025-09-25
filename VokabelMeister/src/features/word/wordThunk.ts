import { createAsyncThunk } from "@reduxjs/toolkit";
import type {
  AddWordPayload,
  Word,
  WordsResponse,
} from "../../Types/wordTypes";
import { addWordApi, fetchWords } from "../../API/word.ts/wordApi";

export const fetchWordsThunk = createAsyncThunk<WordsResponse>(
  "words/fetchWords",
  async () => {
    const data = await fetchWords();
    return data;
  }
);

export const addWordThunk = createAsyncThunk<Word, AddWordPayload>(
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
