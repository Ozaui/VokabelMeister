import { createSlice } from "@reduxjs/toolkit";

import { fetchWords } from "./wordThunk";
import type { WordsState } from "../../types/WordTypes";

const initialState: WordsState = {
  defaultWords: [],
  userWords: [],
  learnedWords: [],
  loading: false,
  error: null,
};

const wordSlice = createSlice({
  name: "words",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchWords.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchWords.fulfilled, (state, action) => {
        state.loading = false;
        state.error = null;
        state.defaultWords = action.payload.defaultWords || [];
        state.userWords = action.payload.userWords || [];
        state.learnedWords = action.payload.learnedWords || [];
      })
      .addCase(fetchWords.rejected, (state, action) => {
        state.loading = false;
        state.error = (action.payload as string) || "Failed to fetch words";
      });
  },
});

export default wordSlice.reducer;
