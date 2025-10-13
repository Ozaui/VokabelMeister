import { createAsyncThunk } from "@reduxjs/toolkit";
import type {
  LoginFormValues,
  RegisterFormValues,
  User,
} from "../../Types/authTypes";
import { loginUserApi, registerUserApi } from "../../API/auth/authApi";
import axios from "axios";
import type { AppDispatch } from "../../store/store";
import { logoutUser } from "./authSlice";

export const registerUserThunk = createAsyncThunk<
  { message: string },
  RegisterFormValues,
  { rejectValue: string; dispatch: AppDispatch }
>("auth/registerUser", async (formData, { rejectWithValue, dispatch }) => {
  try {
    const response = await registerUserApi(formData);
    return response;
  } catch (error: unknown) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 401) {
        dispatch(logoutUser());
      }
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
  { rejectValue: string; dispatch: AppDispatch }
>("auth/loginUser", async (formData, { rejectWithValue, dispatch }) => {
  try {
    const user = await loginUserApi(formData);
    return user;
  } catch (error: unknown) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 401) {
        dispatch(logoutUser());
      }
      return rejectWithValue(error.response?.data?.message || "Login failed");
    }
    return rejectWithValue("Login failed");
  }
});
