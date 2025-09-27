import express from "express";
import mongoose from "mongoose";
import dotenv from "dotenv";
import cors from "cors";
import wordsRoutes from "../routes/wordsRoutes.js";
import userRoutes from "../routes/userRoutes.js";

dotenv.config();
const app = express();

let isConnected = false;

const connectDB = async () => {
  if (isConnected) {
    console.log("MongoDB already connected.");
    return;
  }
  try {
    await mongoose.connect(process.env.MONGO_URI);
    isConnected = true;
    console.log("MongoDB connection successful.");
  } catch (err) {
    console.error("MongoDB connection failed: ", err.message);
  }
};

app.use(
  cors({
    origin: "https://vokabel-meister-xkjc.vercel.app",
    credentials: true,
    methods: "GET,HEAD,PUT,PATCH,POST,DELETE,OPTIONS",
    allowedHeaders: "Content-Type,Authorization",
  })
);
app.use(express.json());

app.use(async (req, res, next) => {
  await connectDB();
  next();
});

app.use("/api/words", wordsRoutes);
app.use("/api/users", userRoutes);

app.get("/", (req, res) => {
  res.send("WordApp API is running successfully!");
});

export default app;
