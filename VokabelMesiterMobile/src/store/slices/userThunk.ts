import { createAsyncThunk } from "@reduxjs/toolkit";
import axios from "axios";
import { API_URL } from "@env";

// Axios instance with default config
const api = axios.create({
  baseURL: API_URL,
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});

// Register User Thunk
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
      if (error && typeof error === "object" && "response" in error) {
        // API responded with error status
        const axiosError = error as {
          response: { data?: { error?: string; message?: string } };
        };
        const errorMessage =
          axiosError.response.data?.error ||
          axiosError.response.data?.message ||
          "Registration failed";
        return rejectWithValue(errorMessage);
      } else if (error && typeof error === "object" && "request" in error) {
        // Request made but no response received
        return rejectWithValue("No response from server");
      } else {
        // Something else happened
        const message =
          error && typeof error === "object" && "message" in error
            ? (error as { message: string }).message
            : "Network error occurred";
        return rejectWithValue(message);
      }
    }
  }
);

// Login User Thunk
export const loginUser = createAsyncThunk(
  "auth/login",
  async (
    loginData: {
      email: string;
      password: string;
    },
    { rejectWithValue }
  ) => {
    try {
      const response = await api.post("/users/login", loginData);
      return response.data; // Expected: { token, user }
    } catch (error: unknown) {
      if (error && typeof error === "object" && "response" in error) {
        // API responded with error status
        const axiosError = error as {
          response: { data?: { error?: string; message?: string } };
        };
        const errorMessage =
          axiosError.response.data?.error ||
          axiosError.response.data?.message ||
          "Login failed";
        return rejectWithValue(errorMessage);
      } else if (error && typeof error === "object" && "request" in error) {
        // Request made but no response received
        return rejectWithValue("No response from server");
      } else {
        // Something else happened
        const message =
          error && typeof error === "object" && "message" in error
            ? (error as { message: string }).message
            : "Network error occurred";
        return rejectWithValue(message);
      }
    }
  }
);
