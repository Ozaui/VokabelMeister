import * as Yup from "yup";

export const newWordSchema = Yup.object({
  german: Yup.string().required("German word is required"),
  turkish: Yup.string().required("Turkish word is required"),
  sampleSentence: Yup.string(),
  category: Yup.string().oneOf([
    "İsim",
    "Fiil",
    "Sıfat",
    "Zarf",
    "Zamir",
    "Edat",
    "Bağlaç",
    "Ünlem",
  ]),
});
