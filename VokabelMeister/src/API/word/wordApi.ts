import axios from "axios";
import type {
  AddWordPayload,
  Word,
  WordsResponse,
} from "../../Types/wordTypes";

const API_BASE_URL = import.meta.env.VITE_BASE_URL as string;

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { "Content-Type": "application/json" },
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
      return config;
    }

    // Token geçerli, Authorization header ekle
    config.headers.Authorization = `Bearer ${token}`;
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

      // Login sayfasına yönlendir
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

export const fetchWords = async (): Promise<WordsResponse> => {
  const response = await api.get("/words");
  return response.data;
};

export const addWordApi = async (wordData: AddWordPayload): Promise<Word> => {
  const response = await api.post("/words", wordData);
  return response.data;
};

export const markWordAsLearnedApi = async (wordId: string): Promise<Word> => {
  const response = await api.post("/words/learned", { wordId });
  return response.data;
};
