import { StyleSheet, Text, View } from "react-native";
import React from "react";

interface CardProps {
  german: string;
  turkish: string;
}

const Card: React.FC<CardProps> = ({ german, turkish }) => {
  return (
    <View style={styles.card}>
      <Text style={styles.german}>{german}</Text>
      <Text style={styles.turkish}>{turkish}</Text>
    </View>
  );
};

export default Card;

const styles = StyleSheet.create({
  card: {
    width: 300,
    minHeight: 180,
    backgroundColor: "#fff",
    borderRadius: 20,
    alignItems: "center",
    justifyContent: "center",
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.15,
    shadowRadius: 12,
    elevation: 8,
    marginVertical: 24,
    padding: 24,
  },
  german: {
    fontSize: 28,
    fontWeight: "bold",
    color: "#ff6900",
    marginBottom: 12,
    textAlign: "center",
  },
  turkish: {
    fontSize: 22,
    color: "#334155",
    textAlign: "center",
  },
});
