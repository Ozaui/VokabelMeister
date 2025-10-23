import React, { useEffect } from "react";
import { View, Text, TouchableOpacity, StyleSheet } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import { fetchWords } from "../store/slices/wordThunk";
import Card from "../components/Card";

const Dashboard: React.FC = () => {
  const dispatch = useAppDispatch();
  const { token } = useAppSelector((state) => state.user);

  useEffect(() => {
    if (token) {
      dispatch(fetchWords(token));
    }
  }, [dispatch, token]);

  const { defaultWords, userWords } = useAppSelector((state) => state.words);
  const allWords = [...defaultWords, ...userWords];
  const [currentIndex, setCurrentIndex] = React.useState(0);

  return (
    <SafeAreaView style={styles.safeArea} edges={["top"]}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.title}>VokabelMeister</Text>
          <Text style={styles.subtitle}>Welcome to your dashboard!</Text>
        </View>

        <View style={styles.content}>
          <View style={styles.cardContainer}>
            {allWords && allWords.length > 0 && (
              <Card
                german={defaultWords[currentIndex]?.german}
                turkish={defaultWords[currentIndex]?.turkish}
              />
            )}
            {allWords && allWords.length > 1 && (
              <TouchableOpacity
                style={styles.nextButton}
                onPress={() =>
                  setCurrentIndex((prev) => (prev + 1) % defaultWords.length)
                }
                activeOpacity={0.8}
              >
                <Text style={styles.nextButtonText}>İleri</Text>
              </TouchableOpacity>
            )}
          </View>
        </View>
      </View>
    </SafeAreaView>
  );
};

export default Dashboard;

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: "#f8fafc",
  },
  container: {
    flex: 1,
    padding: 24,
  },
  header: {
    marginBottom: 16,
    alignItems: "center",
    paddingTop: 8,
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
  content: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    paddingBottom: 80,
  },
  cardContainer: {
    alignItems: "center",
    justifyContent: "center",
    flex: 1,
  },
  nextButton: {
    backgroundColor: "#ff6900",
    paddingVertical: 12,
    paddingHorizontal: 32,
    borderRadius: 12,
    marginTop: 16,
  },
  nextButtonText: {
    color: "#fff",
    fontWeight: "700",
    fontSize: 16,
  },
  welcomeCard: {
    backgroundColor: "#ffffff",
    borderRadius: 24,
    padding: 32,
    alignItems: "center",
    shadowColor: "#000",
    shadowOffset: {
      width: 0,
      height: 4,
    },
    shadowOpacity: 0.1,
    shadowRadius: 12,
    elevation: 8,
    marginBottom: 32,
  },
  welcomeText: {
    fontSize: 24,
    fontWeight: "700",
    color: "#10b981",
    marginBottom: 16,
    textAlign: "center",
  },
  userInfo: {
    fontSize: 16,
    color: "#64748b",
    textAlign: "center",
    marginBottom: 16,
  },
  tokenInfo: {
    fontSize: 12,
    color: "#94a3b8",
    textAlign: "center",
    fontFamily: "monospace",
  },
  logoutButton: {
    backgroundColor: "#ef4444",
    paddingVertical: 16,
    paddingHorizontal: 32,
    borderRadius: 12,
    alignItems: "center",
    shadowColor: "#ef4444",
    shadowOffset: {
      width: 0,
      height: 4,
    },
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 6,
  },
  logoutButtonText: {
    color: "#ffffff",
    fontSize: 16,
    fontWeight: "700",
  },
});
