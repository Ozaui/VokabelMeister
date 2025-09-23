import type {
  LoginFormValues,
  RegisterFormValues,
  User,
} from "../../Types/authTypes";
import axios from "axios";

const API_BASE_URL = import.meta.env.VITE_BASE_URL as string;

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers!.Authorization = `Bearer ${token}`;
  }
  return config;
});

//Register API
export const registerUserApi = async (
  formData: RegisterFormValues
): Promise<{ message: string }> => {
  const response = await api.post<{ message: string }>("/users/register", {
    name: formData.name,
    surname: formData.surname,
    email: formData.email,
    password: formData.password,
    level: formData.level,
  });
  return response.data;
};

// Login API
export const loginUserApi = async (
  formData: LoginFormValues
): Promise<User> => {
  const response = await api.post<User>("/users/login", {
    email: formData.email,
    password: formData.password,
  });

  const data = response.data;

  if (data.token) {
    localStorage.setItem("token", data.token);
  }

  return data;
};
