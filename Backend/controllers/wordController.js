import Word from "../models/Word.js";

export const getWordsByLevel = async (req, res) => {
  try {
    const userLevel = req.user.level;
    const userId = req.user.userId;

    const defaultWords = await Word.find({
      level: userLevel,
      userId: null,
      learnedBy: { $ne: userId },
    });

    const userWords = await Word.find({
      level: userLevel,
      userId: userId,
      learnedBy: { $ne: userId },
    });

    const learnedWords = await Word.find({
      level: userLevel,
      learnedBy: userId,
    });

    const mapWithIsLearned = (words) =>
      words.map((word) => {
        const obj = word.toObject();
        return {
          ...obj,
          isLearned: obj.learnedBy.includes(userId),
        };
      });
    res.json({
      defaultWords: mapWithIsLearned(defaultWords),
      userWords: mapWithIsLearned(userWords),
      learnedWords: mapWithIsLearned(learnedWords),
    });
  } catch (error) {
    res.status(500).json({ message: error.message });
  }
};

export const addWord = async (req, res) => {
  const { german, turkish, sampleSentence, category } = req.body;

  if (!req.user || !req.user.userId) {
    return res.status(401).json({ message: "unauthorized" });
  }

  const word = new Word({
    german,
    turkish,
    sampleSentence,
    category,
    level: req.user.level,
    userId: req.user.userId,
  });

  try {
    const savedWord = await word.save();
    res.status(201).json(savedWord);
  } catch (err) {
    res.status(400).json({ message: err.message });
  }
};

export const markWordAsLearned = async (req, res) => {
  const { wordId } = req.body;
  const userId = req.user.userId;

  try {
    const updatedWord = await Word.findByIdAndUpdate(
      wordId,
      { $addToSet: { learnedBy: userId } },
      { new: true }
    );

    if (!updatedWord) {
      return res.status(404).json({ message: "Word not found" });
    }

    res.json(updatedWord);
  } catch (error) {
    res.status(500).json({ message: error.message });
  }
};
