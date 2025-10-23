import { createAsyncThunk } from "@reduxjs/toolkit";
import axios, { isAxiosError } from "axios";
import { API_URL } from "@env";

const api = axios.create({
  baseURL: API_URL,
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});

export const registerUser = createAsyncThunk(
  "auth/register",
  async (
    userData: {
      name: string;
      surname: string;
      email: string;
      password: string;
      level: string;
    },
    { rejectWithValue }
  ) => {
    try {
      const response = await api.post("/users/register", userData);
      return response.data;
    } catch (error: unknown) {
      // Axios hatası mı kontrol et
      if (isAxiosError(error)) {
        return rejectWithValue(
          error.response?.data?.error ||
            error.response?.data?.message ||
            "Registration failed"
        );
      }

      // Diğer hatalar
      return rejectWithValue(
        error instanceof Error ? error.message : "Network error occurred"
      );
    }
  }
);

// Login User Thunk
export const loginUser = createAsyncThunk(
  "auth/login",
  async (
    loginData: { email: string; password: string },
    { rejectWithValue }
  ) => {
    try {
      const response = await api.post("/users/login", loginData);
      return response.data; // Expected: { token, user }
    } catch (error: unknown) {
      // Axios hatası mı kontrol et
      if (axios.isAxiosError(error)) {
        const errorMessage =
          error.response?.data?.error ||
          error.response?.data?.message ||
          "Login failed";
        return rejectWithValue(errorMessage);
      }

      // Diğer hatalar
      const message =
        error instanceof Error ? error.message : "Network error occurred";
      return rejectWithValue(message);
    }
  }
);
