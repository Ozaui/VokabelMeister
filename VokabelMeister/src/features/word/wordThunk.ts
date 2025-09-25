import { createAsyncThunk } from "@reduxjs/toolkit";
import type {
  Word,
  WordsResponse,
  AddWordPayload,
} from "../../Types/wordTypes";
import {
  fetchWords,
  addWordApi,
  markWordAsLearnedApi,
} from "../../API/word/wordApi";

export const fetchWordsThunk = createAsyncThunk<WordsResponse>(
  "words/fetchWords",
  async () => {
    return await fetchWords();
  }
);

export const addWordThunk = createAsyncThunk<Word, AddWordPayload>(
  "words/addWord",
  async (wordData, { rejectWithValue }) => {
    try {
      return await addWordApi(wordData);
    } catch (error: unknown) {
      if (error instanceof Error) return rejectWithValue(error.message);
      return rejectWithValue("Unknown error occurred");
    }
  }
);

export const markWordAsLearnedThunk = createAsyncThunk<
  Word,
  string,
  { rejectValue: string }
>("words/markAsLearned", async (wordId, { rejectWithValue }) => {
  try {
    return await markWordAsLearnedApi(wordId);
  } catch (error: unknown) {
    if (error instanceof Error) return rejectWithValue(error.message);
    return rejectWithValue("Unknown error occurred");
  }
});
