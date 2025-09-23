import mongoose from "mongoose";

const userSchema = new mongoose.Schema({
  name: { type: String, required: true },
  surname: { type: String, required: true },
  email: { type: String, required: true, unique: true },
  password: { type: String, required: true },
  level: { type: String, enum: ["A1", "A2", "B1", "B2", "C1", "C2"] },
});

export default mongoose.model("User", userSchema);
