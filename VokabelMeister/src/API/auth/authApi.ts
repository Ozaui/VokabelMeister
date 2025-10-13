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

// Axios interceptor: token süresini kontrol eder ve header ekler
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  const expiration = localStorage.getItem("token_expiration");

  if (token && expiration) {
    const now = new Date().getTime();
    if (now > Number(expiration)) {
      // Token süresi dolmuş, sil
      localStorage.removeItem("token");
      localStorage.removeItem("token_expiration");
      localStorage.removeItem("user");
      console.log("Token süresi doldu, otomatik silindi.");
      return config; // istersen burada hata fırlatabilirsin
    }

    // Token geçerli, Authorization header ekle
    config.headers!.Authorization = `Bearer ${token}`;
  }

  return config;
});

// Response interceptor: 401 hatalarını yakalar ve token'ı temizler
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // 401 hatası geldiğinde token'ı temizle
      localStorage.removeItem("token");
      localStorage.removeItem("token_expiration");
      localStorage.removeItem("user");
      console.log("401 hatası alındı, token temizlendi.");

      // Eğer login/register sayfasında değilse login sayfasına yönlendir
      if (
        window.location.pathname !== "/login" &&
        window.location.pathname !== "/register"
      ) {
        window.location.href = "/login";
      }
    }
    return Promise.reject(error);
  }
);

// Register API
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
    const expirationTime = new Date().getTime() + 60 * 60 * 1000; // 1 saat
    localStorage.setItem("token", data.token);
    localStorage.setItem("token_expiration", expirationTime.toString());
  }

  return data;
};

export const checkTokenExpiration = () => {
  const token = localStorage.getItem("token");
  const expiration = localStorage.getItem("token_expiration");

  if (token && expiration && new Date().getTime() > Number(expiration)) {
    localStorage.removeItem("token");
    localStorage.removeItem("token_expiration");
    localStorage.removeItem("user");
    console.log("Token süresi dolduğu için silindi");
  }
};

// Test için: Token süresini manuel olarak doldur
export const expireTokenForTesting = () => {
  const expiredTime = new Date().getTime() - 1000; // 1 saniye önce
  localStorage.setItem("token_expiration", expiredTime.toString());
  console.log("Token süresi test için dolduruldu");
};
