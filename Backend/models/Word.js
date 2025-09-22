import mongoose from "mongoose";

const wordSchema = new mongoose.Schema(
  {
    german: { type: String, required: true },
    turkish: { type: String, required: true },
    sampleSentence: { type: String },
    category: { type: String, required: true }, // İsim, fiil, sıfat vb.
    difficulty: { type: String, required: true }, // easy, medium, hard
    learned: { type: Boolean, default: false },
  },
  { timestamps: true }
);

export default mongoose.model("Word", wordSchema);
