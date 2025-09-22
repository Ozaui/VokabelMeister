import Word from "../models/Word.js";

export const getWords = async (req, res) => {
  try {
    const words = await Word.find();
    res.json(words);
  } catch (err) {
    res.status(500).json({ message: err.message });
  }
};

export const addWord = async (req, res) => {
  const { german, turkish, sampleSentence, category, difficulty } = req.body;
  const word = new Word({
    german,
    turkish,
    sampleSentence,
    category,
    difficulty,
  });

  try {
    const savedWord = await word.save();
    res.status(201).json(savedWord);
  } catch (err) {
    res.status(400).json({ message: err.message });
  }
};
