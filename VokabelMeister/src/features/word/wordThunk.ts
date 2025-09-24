import { createAsyncThunk } from "@reduxjs/toolkit";
import type { Word } from "../../Types/wordTypes";
import { fetchWords } from "../../API/word.ts/wordApi";

export const fetchWordsThunk = createAsyncThunk<Word[]>(
  "words/fetchWords",
  async () => {
    const data = await fetchWords();
    return data;
  }
);
