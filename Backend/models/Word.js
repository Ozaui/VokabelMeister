import mongoose from "mongoose";

const wordSchema = new mongoose.Schema(
  {
    german: { type: String, required: true },
    turkish: { type: String, required: true },
    sampleSentence: { type: String },
    category: { type: String, default: true },
    level: {
      type: String,
      required: true,
      enum: ["A1", "A2", "B1", "B2", "C1", "C2"],
    },
    learnedBy: [{ type: mongoose.Schema.Types.ObjectId, ref: "User" }],
    userId: {
      type: mongoose.Schema.Types.ObjectId,
      ref: "User",
      default: null,
    },
  },
  { timestamps: true }
);

export default mongoose.model("Word", wordSchema);
