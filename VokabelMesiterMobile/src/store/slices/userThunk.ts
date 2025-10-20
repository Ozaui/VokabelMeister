import { createAsyncThunk } from "@reduxjs/toolkit";
import { API_URL } from "@env";

// Register Thunk
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
      const response = await fetch(`${API_URL}/users/register`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(userData),
      });

      const data = await response.json();

      if (!response.ok) {
        return rejectWithValue(data.error || "Registration failed");
      }

      return data;
    } catch {
      return rejectWithValue("Network error occurred");
    }
  }
);

// Login Thunk
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
      const response = await fetch(`${API_URL}/users/login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(loginData),
      });

      const data = await response.json();

      if (!response.ok) {
        return rejectWithValue(data.message || "Login failed");
      }

      return data; // { token, level }
    } catch {
      return rejectWithValue("Network error occurred");
    }
  }
);
