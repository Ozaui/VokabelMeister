import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { AuthState, User } from "../../Types/authTypes";
import { loginUserThunk, registerUserThunk } from "./authThunk";

const savedUser = localStorage.getItem("user");
const initialState: AuthState = {
  user: savedUser ? JSON.parse(savedUser) : null,
  loading: false,
  error: null,
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    logoutUser: (state) => {
      state.user = null;
      state.error = null;
      localStorage.removeItem("token");
      localStorage.removeItem("token_expiration");
      localStorage.removeItem("user");
    },
  },
  extraReducers: (builder) => {
    builder
      // Register
      .addCase(registerUserThunk.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(registerUserThunk.fulfilled, (state) => {
        state.loading = false;
      })
      .addCase(registerUserThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Login
      .addCase(loginUserThunk.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(
        loginUserThunk.fulfilled,
        (state, action: PayloadAction<User>) => {
          state.loading = false;
          state.user = action.payload;

          localStorage.setItem("user", JSON.stringify(action.payload));
          localStorage.setItem("token", action.payload.token);
        }
      )
      .addCase(loginUserThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload || "Login failed";
      });
  },
});

export const { logoutUser } = authSlice.actions;
export default authSlice.reducer;
