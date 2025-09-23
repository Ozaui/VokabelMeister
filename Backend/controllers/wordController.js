import Word from "../models/Word.js";

export const getWordsByLevel = async (req, res) => {
  try {
    const userLevel = req.user.level;
    const userId = req.user.userId;

    const words = await Word.find({
      level: userLevel,
      $or: [{ userId: userId }, { userId: null }],
    });

    res.json(words);
  } catch (error) {
    res.status(500).json({ message: error.message });
  }
};

export const addWord = async (req, res) => {
  const { german, turkish, sampleSentence, category, level, isDefault } =
    req.body;

  const word = new Word({
    german,
    turkish,
    sampleSentence,
    category,
    level,
    userId: isDefault ? null : req.user.userId,
  });

  try {
    const savedWord = await word.save();
    res.status(201).json(savedWord);
  } catch (err) {
    res.status(400).json({ message: err.message });
  }
};
