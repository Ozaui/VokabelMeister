import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { Word, WordsState } from "../../Types/wordTypes";
import { fetchWordsThunk } from "./wordThunk";

const initialState: WordsState = {
  words: [],
  loading: false,
  error: null,
};
const wordsSlice = createSlice({
  name: "words",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchWordsThunk.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(
        fetchWordsThunk.fulfilled,
        (state, action: PayloadAction<Word[]>) => {
          state.loading = false;
          state.words = action.payload;
        }
      )
      .addCase(fetchWordsThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || "Failed to fetch words";
      });
  },
});

export default wordsSlice.reducer;
