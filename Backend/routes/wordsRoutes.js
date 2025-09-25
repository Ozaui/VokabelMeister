import express from "express";
import {
  getWordsByLevel,
  addWord,
  markWordAsLearned,
} from "../controllers/wordController.js";
import authMiddleware from "../middleware/authMiddleware.js";

const router = express.Router();

router.get("/", authMiddleware, getWordsByLevel);
router.post("/", authMiddleware, addWord);
router.post("/learned", authMiddleware, markWordAsLearned);

export default router;
