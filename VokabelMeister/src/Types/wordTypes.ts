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

export type AddWordPayload = {
  german: string;
  turkish: string;
  sampleSentence?: string;
  category: string;
};

export type WordsState = {
  defaultWords: Word[];
  userWords: Word[];
  fetchLoading: boolean;
  addLoading: boolean;
  error: string | null;
};

export type WordsResponse = {
  defaultWords: Word[];
  userWords: Word[];
};
