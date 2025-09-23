export type User = {
  name?: string;
  surname?: string;
  email: string;
  level?: string;
  token: string;
};

export type AuthState = {
  user: User | null;
  loading: boolean;
  error: string | null;
};

// For register
export type RegisterFormValues = {
  name: string;
  surname: string;
  email: string;
  password: string;
  confirmPassword: string;
  level: "A1" | "A2" | "B1" | "B2" | "C1" | "C2";
};

// For login
export type LoginFormValues = {
  email: string;
  password: string;
};
