export interface User {
  id?: string;
  name: string;
  surname: string;
  email: string;
  level: string;
}

export interface UserState {
  isAuthenticated: boolean;
  user: User | null;
  token: string | null;
  loading: boolean;
  error: string | null;
}
