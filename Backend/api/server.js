import express from "express";
import mongoose from "mongoose";
import dotenv from "dotenv";
import cors from "cors";
// Route dosya yollarını güncelledik: 'routes' klasörü artık bir üst seviyede
import wordsRoutes from "../routes/wordsRoutes.js";
import userRoutes from "../routes/userRoutes.js";

dotenv.config(); // Ortam değişkenlerini yükle
const app = express(); // Express uygulaması

// MongoDB bağlantı durumunu Vercel'in Cold Start'ını yönetmek için tutalım
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
    // Hata durumunda uygulamanın çökmesini önlemek için burada throw etmiyoruz
  }
};

// Middleware'ler
app.use(cors()); // Cors ayarları
app.use(express.json()); // JSON verilerini okuma

// Gelen her API isteğinde DB bağlantısını kontrol et ve bağlan (veya bağlıysa geç)
app.use(async (req, res, next) => {
  await connectDB();
  next();
});

// Route Tanımlamaları
app.use("/api/words", wordsRoutes);
app.use("/api/users", userRoutes);

// Kök yol (root path) için test endpoint'i
app.get("/", (req, res) => {
  res.send("WordApp API is running successfully!");
});

// Vercel için Express uygulamasını dışa aktarın
export default app;
