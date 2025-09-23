import express from "express";
import { getWordsByLevel, addWord } from "../controllers/wordController.js";
import authMiddleware from "../middleware/authMiddleware.js";

const router = express.Router();

router.get("/", authMiddleware, getWordsByLevel);
router.post("/", authMiddleware, addWord);

export default router;
