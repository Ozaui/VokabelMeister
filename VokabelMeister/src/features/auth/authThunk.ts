import { createAsyncThunk } from "@reduxjs/toolkit";
import type {
  LoginFormValues,
  RegisterFormValues,
  User,
} from "../../Types/authTypes";
import { loginUserApi, registerUserApi } from "../../API/auth/authApi";
import axios from "axios";

export const registerUserThunk = createAsyncThunk<
  { message: string },
  RegisterFormValues,
  { rejectValue: string }
>("auth/registerUser", async (formData, { rejectWithValue }) => {
  try {
    const response = await registerUserApi(formData);
    return response;
  } catch (error: unknown) {
    if (axios.isAxiosError(error)) {
      return rejectWithValue(
        error.response?.data?.message || "Registration failed"
      );
    }

    return rejectWithValue("Registration failed");
  }
});

export const loginUserThunk = createAsyncThunk<
  User,
  LoginFormValues,
  { rejectValue: string }
>("auth/loginUser", async (formData, { rejectWithValue }) => {
  try {
    const user = await loginUserApi(formData);
    return user;
  } catch (error: unknown) {
    if (axios.isAxiosError(error)) {
      return rejectWithValue(error.response?.data?.message || "Login failed");
    }
    return rejectWithValue("Login failed");
  }
});
