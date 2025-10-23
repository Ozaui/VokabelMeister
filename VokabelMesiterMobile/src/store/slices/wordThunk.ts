import { createAsyncThunk } from "@reduxjs/toolkit";
import axios from "axios";
import { API_URL } from "@env";

const api = axios.create({
  baseURL: API_URL,
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});

// words/fetch thunk
export const fetchWords = createAsyncThunk(
  "words/fetch",
  async (token: string, { rejectWithValue }) => {
    try {
      const response = await api.get("/words", {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
      return response.data;
    } catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        return rejectWithValue(
          error.response?.data?.message || "Failed to fetch words"
        );
      }
      return rejectWithValue("Failed to fetch words");
    }
  }
);
