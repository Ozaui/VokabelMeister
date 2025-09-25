import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { Word, WordsState, WordsResponse } from "../../Types/wordTypes";
import {
  fetchWordsThunk,
  addWordThunk,
  markWordAsLearnedThunk,
} from "./wordThunk";

const initialState: WordsState = {
  defaultWords: [],
  userWords: [],
  learnedWords: [],
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
        (state, action: PayloadAction<WordsResponse>) => {
          state.fetchLoading = false;
          state.defaultWords = action.payload.defaultWords;
          state.userWords = action.payload.userWords;
          state.learnedWords = action.payload.learnedWords;
        }
      )
      .addCase(fetchWordsThunk.rejected, (state, action) => {
        state.fetchLoading = false;
        state.error = action.error.message || "Failed to fetch words";
      });

    // Add Word
    builder
      .addCase(addWordThunk.pending, (state) => {
        state.addLoading = true;
        state.error = null;
      })
      .addCase(addWordThunk.fulfilled, (state, action: PayloadAction<Word>) => {
        state.addLoading = false;
        state.userWords.unshift(action.payload);
      })
      .addCase(addWordThunk.rejected, (state, action) => {
        state.addLoading = false;
        state.error = action.payload as string;
      });

    // Mark Word as Learned
    builder.addCase(
      markWordAsLearnedThunk.fulfilled,
      (state, action: PayloadAction<Word>) => {
        state.userWords = state.userWords.filter(
          (w) => w._id !== action.payload._id
        );
        state.defaultWords = state.defaultWords.filter(
          (w) => w._id !== action.payload._id
        );
        if (!state.learnedWords.find((w) => w._id === action.payload._id)) {
          state.learnedWords.push(action.payload);
        }
      }
    );
  },
});

export default wordsSlice.reducer;
