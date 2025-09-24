export type Word = {
  _id: string;
  german: string;
  turkish: string;
  sampleSentence?: string;
  category: string;
  level: "A1" | "A2" | "B1" | "B2" | "C1" | "C2";
  learned: boolean;
  userId: string | null;
};

export type WordsState = {
  words: Word[];
  loading: boolean;
  error: string | null;
};
