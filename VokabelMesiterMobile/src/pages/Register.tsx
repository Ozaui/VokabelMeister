import React, { useState } from "react";
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  ScrollView,
  Alert,
  StyleSheet,
  Modal,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { Formik, FormikHelpers } from "formik";
import { useNavigation } from "@react-navigation/native";
import { registerSchema } from "../schemas/RegisterSchema";
import { RegisterFormValues } from "../types/RegisterTypes";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import { registerUser } from "../store/slices/userThunk";
import { RegisterScreenNavigationProp } from "../types/NavigationTypes";

const levels = ["A1", "A2", "B1", "B2", "C1", "C2"];

const Register = () => {
  const navigation = useNavigation<RegisterScreenNavigationProp>();
  const dispatch = useAppDispatch();
  const { loading } = useAppSelector((state) => state.user);

  const initialValues: RegisterFormValues = {
    name: "",
    surname: "",
    email: "",
    password: "",
    confirmPassword: "",
    level: "A1",
  };

  const [modalVisible, setModalVisible] = useState(false);

  const handleSubmit = async (
    values: RegisterFormValues,
    { resetForm, setFieldError }: FormikHelpers<RegisterFormValues>
  ) => {
    try {
      // Confirm password'ü çıkarıyoruz çünkü backend'e gönderilmemeli
      const registerData = {
        name: values.name,
        surname: values.surname,
        email: values.email,
        password: values.password,
        level: values.level,
      };

      const result = await dispatch(registerUser(registerData));

      if (registerUser.fulfilled.match(result)) {
        resetForm();
        navigation.navigate("Login");
      } else {
        // Hata durumu
        const errorMessage =
          (result.payload as string) || "Registration failed";
        if (errorMessage.toLowerCase().includes("email")) {
          setFieldError("email", "This email is already registered");
        } else {
          Alert.alert("Registration Failed", errorMessage);
        }
      }
    } catch {
      Alert.alert("Registration Failed", "Network error occurred");
    }
  };

  return (
    <SafeAreaView style={styles.safeArea} edges={["top"]}>
      <ScrollView style={styles.container}>
        <View style={styles.content}>
          <View style={styles.header}>
            <Text style={styles.title}>VokabelMeister</Text>
            <Text style={styles.subtitle}>Create your account</Text>
            <Text style={styles.warningMessage}>
              Please select A1 level, because the other levels do not have any
              words.
            </Text>
          </View>

          <Formik<RegisterFormValues>
            initialValues={initialValues}
            validationSchema={registerSchema}
            onSubmit={handleSubmit}
          >
            {({
              handleChange,
              handleBlur,
              handleSubmit,
              values,
              errors,
              touched,
              setFieldValue,
              isSubmitting,
            }) => (
              <View style={styles.formCard}>
                {/* Name Field */}
                <View style={styles.inputGroup}>
                  <Text style={styles.label}>First Name</Text>
                  <TextInput
                    style={styles.input}
                    value={values.name}
                    onChangeText={handleChange("name")}
                    onBlur={handleBlur("name")}
                    placeholder="Enter your first name"
                    placeholderTextColor="#94a3b8"
                  />
                  {errors.name && touched.name && (
                    <Text style={styles.errorText}>{errors.name}</Text>
                  )}
                </View>

                {/* Surname Field */}
                <View style={styles.inputGroup}>
                  <Text style={styles.label}>Last Name</Text>
                  <TextInput
                    style={styles.input}
                    value={values.surname}
                    onChangeText={handleChange("surname")}
                    onBlur={handleBlur("surname")}
                    placeholder="Enter your last name"
                    placeholderTextColor="#94a3b8"
                  />
                  {errors.surname && touched.surname && (
                    <Text style={styles.errorText}>{errors.surname}</Text>
                  )}
                </View>

                {/* Email Field */}
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

                {/* Password Field */}
                <View style={styles.inputGroup}>
                  <Text style={styles.label}>Password</Text>
                  <TextInput
                    style={styles.input}
                    value={values.password}
                    onChangeText={handleChange("password")}
                    onBlur={handleBlur("password")}
                    placeholder="At least 6 characters"
                    secureTextEntry
                    placeholderTextColor="#94a3b8"
                  />
                  {errors.password && touched.password && (
                    <Text style={styles.errorText}>{errors.password}</Text>
                  )}
                </View>

                {/* Confirm Password Field */}
                <View style={styles.inputGroup}>
                  <Text style={styles.label}>Confirm Password</Text>
                  <TextInput
                    style={styles.input}
                    value={values.confirmPassword}
                    onChangeText={handleChange("confirmPassword")}
                    onBlur={handleBlur("confirmPassword")}
                    placeholder="Re-enter your password"
                    secureTextEntry
                    placeholderTextColor="#94a3b8"
                  />
                  {errors.confirmPassword && touched.confirmPassword && (
                    <Text style={styles.errorText}>
                      {errors.confirmPassword}
                    </Text>
                  )}
                </View>

                {/* Level Selector */}
                <View style={styles.inputGroup}>
                  <Text style={styles.label}>Language Level</Text>
                  <TouchableOpacity
                    style={styles.levelSelector}
                    onPress={() => setModalVisible(true)}
                  >
                    <Text style={styles.levelSelectorText}>
                      {values.level ? `Level ${values.level}` : "Select Level"}
                    </Text>
                    <Text style={styles.levelSelectorArrow}>▼</Text>
                  </TouchableOpacity>
                  {errors.level && touched.level && (
                    <Text style={styles.errorText}>{errors.level}</Text>
                  )}
                </View>

                {/* Level Selection Modal */}
                <Modal
                  animationType="slide"
                  transparent={true}
                  visible={modalVisible}
                  onRequestClose={() => setModalVisible(false)}
                >
                  <View style={styles.modalOverlay}>
                    <View style={styles.modalContent}>
                      <View style={styles.modalHeader}>
                        <Text style={styles.modalTitle}>
                          Select Language Level
                        </Text>
                        <TouchableOpacity
                          onPress={() => setModalVisible(false)}
                        >
                          <Text style={styles.modalClose}>✕</Text>
                        </TouchableOpacity>
                      </View>
                      <ScrollView style={styles.levelList}>
                        {levels.map((lvl) => (
                          <TouchableOpacity
                            key={lvl}
                            style={[
                              styles.levelOption,
                              values.level === lvl &&
                                styles.levelOptionSelected,
                            ]}
                            onPress={() => {
                              setFieldValue("level", lvl);
                              setModalVisible(false);
                            }}
                          >
                            <Text
                              style={[
                                styles.levelOptionText,
                                values.level === lvl &&
                                  styles.levelOptionTextSelected,
                              ]}
                            >
                              Level {lvl}
                            </Text>
                            {values.level === lvl && (
                              <Text style={styles.checkmark}>✓</Text>
                            )}
                          </TouchableOpacity>
                        ))}
                      </ScrollView>
                    </View>
                  </View>
                </Modal>

                {/* Submit Button */}
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
                    {isSubmitting || loading ? "Creating..." : "Create Account"}
                  </Text>
                </TouchableOpacity>

                {/* Footer Link */}
                <View style={styles.footer}>
                  <Text style={styles.footerText}>
                    Already have an account?
                    <Text
                      style={styles.footerLink}
                      onPress={() => navigation.navigate("Login")}
                    >
                      {" "}
                      Sign In
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
    color: "#ff6900",
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 18,
    fontWeight: "600",
    color: "black",
  },
  warningMessage: { fontSize: 14, fontWeight: "500", color: "red" },
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
  levelSelector: {
    backgroundColor: "#f1f5f9",
    borderWidth: 2,
    borderColor: "#ff6900",
    borderRadius: 12,
    paddingHorizontal: 16,
    paddingVertical: 14,
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
  },
  levelSelectorText: {
    fontSize: 16,
    color: "#1e293b",
  },
  levelSelectorArrow: {
    fontSize: 12,
    color: "#64748b",
  },
  modalOverlay: {
    flex: 1,
    backgroundColor: "rgba(0, 0, 0, 0.5)",
    justifyContent: "flex-end",
  },
  modalContent: {
    backgroundColor: "#ffffff",
    borderTopLeftRadius: 24,
    borderTopRightRadius: 24,
    paddingBottom: 40,
    maxHeight: "70%",
  },
  modalHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    padding: 20,
    borderBottomWidth: 1,
    borderBottomColor: "#ff6900",
  },
  modalTitle: {
    fontSize: 20,
    fontWeight: "700",
    color: "#1e293b",
  },
  modalClose: {
    fontSize: 24,
    color: "#64748b",
    fontWeight: "600",
  },
  levelList: {
    padding: 16,
  },
  levelOption: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    padding: 16,
    borderRadius: 12,
    marginBottom: 8,
    backgroundColor: "#f8fafc",
    borderWidth: 2,
    borderColor: "#e2e8f0",
  },
  levelOptionSelected: {
    backgroundColor: "#eef2ff",
    borderColor: "#ff6900",
  },
  levelOptionText: {
    fontSize: 16,
    color: "#1e293b",
    fontWeight: "500",
  },
  levelOptionTextSelected: {
    color: "#ff6900",
    fontWeight: "700",
  },
  checkmark: {
    fontSize: 20,
    color: "#ff6900",
    fontWeight: "700",
  },
  errorText: {
    color: "#ef4444",
    fontSize: 12,
    marginTop: 4,
    marginLeft: 4,
  },
  button: {
    backgroundColor: "#ff6900",
    borderRadius: 12,
    paddingVertical: 16,
    alignItems: "center",
    shadowColor: "#4f46e5",
    shadowOffset: {
      width: 0,
      height: 4,
    },
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 6,
    marginTop: 8,
  },
  buttonDisabled: {
    backgroundColor: "",
  },
  buttonText: {
    color: "#ffffff",
    fontSize: 18,
    fontWeight: "700",
  },
  footer: {
    marginTop: 24,
    alignItems: "center",
  },
  footerText: {
    color: "#64748b",
    fontSize: 14,
  },
  footerLink: {
    color: "#ff6900",
    fontWeight: "600",
  },
});

export default Register;
