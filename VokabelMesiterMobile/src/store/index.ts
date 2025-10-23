import { configureStore } from "@reduxjs/toolkit";
import userReducer from "./slices/userSlice";
import wordsReducer from "./slices/wordSlice";

export const store = configureStore({
  reducer: {
    user: userReducer,
    words: wordsReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
