import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { Word, WordsState } from "../../Types/wordTypes";
import { addWordThunk, fetchWordsThunk } from "./wordThunk";

const initialState: WordsState = {
  words: [],
  fetchLoading: false,
  addLoading: false,
  error: null,
};
const wordsSlice = createSlice({
  name: "words",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    // Fetch Words
    builder
      .addCase(fetchWordsThunk.pending, (state) => {
        state.fetchLoading = true;
        state.error = null;
      })
      .addCase(
        fetchWordsThunk.fulfilled,
        (state, action: PayloadAction<Word[]>) => {
          state.fetchLoading = false;
          state.words = action.payload;
        }
      )
      .addCase(fetchWordsThunk.rejected, (state, action) => {
        state.fetchLoading = false;
        state.error = action.error.message || "Failed to fetch words";
      })
      // Add new word
      .addCase(addWordThunk.pending, (state) => {
        state.addLoading = true;
        state.error = null;
      })
      .addCase(addWordThunk.fulfilled, (state, action: PayloadAction<Word>) => {
        state.addLoading = false;
        state.words.push(action.payload);
      })
      .addCase(addWordThunk.rejected, (state, action) => {
        state.addLoading = false;
        state.error = action.payload as string;
      });
  },
});

export default wordsSlice.reducer;
