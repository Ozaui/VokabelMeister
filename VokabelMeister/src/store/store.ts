import { configureStore } from "@reduxjs/toolkit";
import authReducer from "../features/auth/authSlice";
import wordsReducer from "../features/word/wordSlice";

export const store = configureStore({
  reducer: {
    auth: authReducer,
    words: wordsReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
