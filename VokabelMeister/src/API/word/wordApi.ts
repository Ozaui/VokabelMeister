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

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

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
