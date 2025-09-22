import express from "express";
import { getWords, addWord } from "../controllers/wordController.js";

const router = express.Router();

router.get("/", getWords);
router.post("/", addWord);

export default router;
