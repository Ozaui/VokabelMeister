export interface Word {
  _id: string;
  german: string;
  turkish: string;
  sampleSentence?: string;
  category?: string;
  level: string;
  isLearned?: boolean;
  userId?: string | null;
  learnedBy?: string[];
}

export interface WordsState {
  defaultWords: Word[];
  userWords: Word[];
  learnedWords: Word[];
  loading: boolean;
  error: string | null;
}
