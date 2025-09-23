import express from "express";
import mongoose from "mongoose";
import dotenv from "dotenv";
import cors from "cors";
import wordsRoutes from "./routes/wordsRoutes.js";
import userRoutes from "./routes/userRoutes.js";

dotenv.config(); // Config dosyalarının kullanılabilmesi için
const app = express(); // Expressi kullanmak için

app.use(cors()); // Cors ayarları
app.use(express.json()); // Gelen JSON verilerini okuyabilmek için

app.use("/api/words", wordsRoutes);
app.use("/api/users", userRoutes);

//MongoDB bağlantısı
mongoose
  .connect(process.env.MONGO_URI)
  .then(() => console.log("MongoDb connection successful "))
  .catch((err) => {
    console.log("MongoDB connection failed ", err.message);
  });

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => console.log(`Server is running on port ${PORT}`));
