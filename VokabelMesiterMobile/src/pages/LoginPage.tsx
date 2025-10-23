import React from "react";
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  ScrollView,
  Alert,
  StyleSheet,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { Formik, FormikHelpers } from "formik";
import { useNavigation } from "@react-navigation/native";
import { loginSchema } from "../schemas/LoginSchema";
import { LoginFormValues } from "../types/LoginTypes";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import { loginUser } from "../store/slices/userThunk";
import { LoginScreenNavigationProp } from "../types/NavigationTypes";

const initialValues: LoginFormValues = {
  email: "",
  password: "",
};

const LoginPage = () => {
  const dispatch = useAppDispatch();
  const navigation = useNavigation<LoginScreenNavigationProp>();
  const { loading } = useAppSelector((state) => state.user);

  const handleSubmit = async (
    values: LoginFormValues,
    { resetForm, setFieldError }: FormikHelpers<LoginFormValues>
  ) => {
    try {
      const result = await dispatch(loginUser(values));

      if (loginUser.fulfilled.match(result)) {
        resetForm();
        // Navigation to Dashboard will happen automatically via Redux state change
      } else {
        const errorMessage = (result.payload as string) || "Login failed";
        if (errorMessage.toLowerCase().includes("not found")) {
          setFieldError("email", "User not found");
        } else if (errorMessage.toLowerCase().includes("password")) {
          setFieldError("password", "Wrong password");
        } else {
          Alert.alert("Login Failed", errorMessage);
        }
      }
    } catch {
      Alert.alert("Login Failed", "Network error occurred");
    }
  };

  return (
    <SafeAreaView style={styles.safeArea} edges={["top"]}>
      <ScrollView style={styles.container}>
        <View style={styles.content}>
          <View style={styles.header}>
            <Text style={styles.title}>VokabelMeister</Text>
            <Text style={styles.subtitle}>Welcome back!</Text>
          </View>

          <Formik<LoginFormValues>
            initialValues={initialValues}
            validationSchema={loginSchema}
            onSubmit={handleSubmit}
          >
            {({
              values,
              errors,
              touched,
              handleChange,
              handleBlur,
              handleSubmit,
              isSubmitting,
            }) => (
              <View style={styles.formCard}>
                <View style={styles.inputGroup}>
                  <Text style={styles.label}>Email</Text>
                  <TextInput
                    style={styles.input}
                    value={values.email}
                    onChangeText={handleChange("email")}
                    onBlur={handleBlur("email")}
                    placeholder="example@email.com"
                    keyboardType="email-address"
                    autoCapitalize="none"
                    placeholderTextColor="#94a3b8"
                  />
                  {errors.email && touched.email && (
                    <Text style={styles.errorText}>{errors.email}</Text>
                  )}
                </View>

                <View style={styles.inputGroup}>
                  <Text style={styles.label}>Password</Text>
                  <TextInput
                    style={styles.input}
                    value={values.password}
                    onChangeText={handleChange("password")}
                    onBlur={handleBlur("password")}
                    placeholder="Enter your password"
                    secureTextEntry
                    placeholderTextColor="#94a3b8"
                  />
                  {errors.password && touched.password && (
                    <Text style={styles.errorText}>{errors.password}</Text>
                  )}
                </View>

                <TouchableOpacity style={styles.forgotPassword}>
                  <Text style={styles.forgotPasswordText}>
                    Forgot Password?
                  </Text>
                </TouchableOpacity>

                <TouchableOpacity
                  activeOpacity={0.8}
                  style={[
                    styles.button,
                    (isSubmitting || loading) && styles.buttonDisabled,
                  ]}
                  onPress={() => handleSubmit()}
                  disabled={isSubmitting || loading}
                >
                  <Text style={styles.buttonText}>
                    {isSubmitting || loading ? "Signing In..." : "Sign In"}
                  </Text>
                </TouchableOpacity>

                <View style={styles.footer}>
                  <Text style={styles.footerText}>
                    Don't have an account?
                    <Text
                      style={styles.footerLink}
                      onPress={() => navigation.navigate("Register")}
                    >
                      Sign Up
                    </Text>
                  </Text>
                </View>
              </View>
            )}
          </Formik>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
};

export default LoginPage;

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: "#f8fafc",
  },
  container: {
    flex: 1,
  },
  content: {
    padding: 24,
  },
  header: {
    marginBottom: 32,
  },
  title: {
    fontSize: 42,
    fontWeight: "800",
    textAlign: "center",
    color: "#ff6900",
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 18,
    fontWeight: "600",
    textAlign: "center",
    color: "#64748b",
  },
  formCard: {
    backgroundColor: "#ffffff",
    borderRadius: 24,
    padding: 24,
    shadowColor: "#000",
    shadowOffset: {
      width: 0,
      height: 4,
    },
    shadowOpacity: 0.1,
    shadowRadius: 12,
    elevation: 8,
  },
  inputGroup: {
    marginBottom: 20,
  },
  label: {
    fontSize: 14,
    fontWeight: "600",
    color: "#334155",
    marginBottom: 8,
  },
  input: {
    backgroundColor: "#f1f5f9",
    borderWidth: 2,
    borderColor: "#e2e8f0",
    borderRadius: 12,
    paddingHorizontal: 16,
    paddingVertical: 14,
    fontSize: 16,
    color: "#1e293b",
  },
  errorText: {
    color: "#ef4444",
    fontSize: 12,
    marginTop: 4,
    marginLeft: 4,
  },
  forgotPassword: {
    alignSelf: "flex-end",
    marginBottom: 24,
  },
  forgotPasswordText: {
    color: "#ff6900",
    fontSize: 14,
    fontWeight: "600",
  },
  button: {
    backgroundColor: "#ff6900",
    paddingVertical: 16,
    borderRadius: 12,
    alignItems: "center",
    shadowColor: "#4f46e5",
    shadowOffset: {
      width: 0,
      height: 4,
    },
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 6,
  },
  buttonDisabled: {
    backgroundColor: "#94a3b8",
    shadowOpacity: 0.1,
  },
  buttonText: {
    color: "#ffffff",
    fontSize: 16,
    fontWeight: "700",
  },
  footer: {
    marginTop: 24,
    alignItems: "center",
  },
  footerText: {
    fontSize: 14,
    color: "#64748b",
  },
  footerLink: {
    color: "#ff6900",
    fontWeight: "700",
  },
});
