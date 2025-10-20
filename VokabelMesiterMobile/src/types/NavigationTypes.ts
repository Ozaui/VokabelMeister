import { NativeStackNavigationProp } from "@react-navigation/native-stack";

// Auth Stack - Login ve Register için
export type AuthStackParamList = {
  Login: undefined;
  Register: undefined;
};

// Main Stack - Dashboard ve diğer authenticated sayfalar
export type MainStackParamList = {
  Dashboard: undefined;
  // Buraya diğer sayfaları ekleyebilirsiniz
};

// Root Stack - Auth ve Main stack'leri içerir
export type RootStackParamList = {
  AuthStack: undefined;
  MainStack: undefined;
};

// Navigation prop types
export type LoginScreenNavigationProp = NativeStackNavigationProp<
  AuthStackParamList,
  "Login"
>;

export type RegisterScreenNavigationProp = NativeStackNavigationProp<
  AuthStackParamList,
  "Register"
>;

export type DashboardScreenNavigationProp = NativeStackNavigationProp<
  MainStackParamList,
  "Dashboard"
>;
