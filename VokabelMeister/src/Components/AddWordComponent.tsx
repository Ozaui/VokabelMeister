import React from "react";
import { useDispatch, useSelector } from "react-redux";
import { type AppDispatch, type RootState } from "../store/store";
import type { AddWordPayload } from "../Types/wordTypes";
import { addWordThunk } from "../features/word/wordThunk";
import { newWordSchema } from "../schemas/addWordSchema";
import { ErrorMessage, Field, Form, Formik, type FormikHelpers } from "formik";

interface AddWordComponentProps {
  onCancel?: () => void; // parent iptal butonu isterse
}

const AddWordComponent: React.FC<AddWordComponentProps> = ({ onCancel }) => {
  const dispatch = useDispatch<AppDispatch>();
  const { addLoading, error } = useSelector((state: RootState) => state.words);

  const initialValues: AddWordPayload = {
    german: "",
    turkish: "",
    sampleSentence: "",
    category: "İsim",
  };

  const handleSubmit = async (
    values: AddWordPayload,
    formikHelpers: FormikHelpers<AddWordPayload>
  ) => {
    const resultAction = await dispatch(addWordThunk(values));
    if (addWordThunk.fulfilled.match(resultAction)) {
      formikHelpers.resetForm();
      if (onCancel) onCancel(); // ekleme bitince kapat
    }
  };

  return (
    <div className="bg-white shadow-lg rounded-xl p-6 border-t-4 border-orange-500">
      <h1 className="text-2xl font-bold text-gray-800 mb-4">Add New Word</h1>
      <Formik
        initialValues={initialValues}
        validationSchema={newWordSchema}
        onSubmit={handleSubmit}
      >
        {({ isSubmitting }) => (
          <Form className="space-y-4">
            <div>
              <label className="block text-gray-700 font-medium mb-1">
                German
              </label>
              <Field
                type="text"
                name="german"
                className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-orange-400"
                placeholder="z.B. Haus"
              />
              <ErrorMessage
                name="german"
                component="div"
                className="text-red-500 text-sm mt-1"
              />
            </div>

            <div>
              <label className="block text-gray-700 font-medium mb-1">
                Turkish
              </label>
              <Field
                type="text"
                name="turkish"
                className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-orange-400"
                placeholder="ör: Ev"
              />
              <ErrorMessage
                name="turkish"
                component="div"
                className="text-red-500 text-sm mt-1"
              />
            </div>

            <div>
              <label className="block text-gray-700 font-medium mb-1">
                Sample Sentence
              </label>
              <Field
                type="text"
                name="sampleSentence"
                className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-orange-400"
                placeholder="Das Haus ist groß."
              />
              <ErrorMessage
                name="sampleSentence"
                component="div"
                className="text-red-500 text-sm mt-1"
              />
            </div>

            <div>
              <label className="block text-gray-700 font-medium mb-1">
                Category
              </label>
              <Field
                as="select"
                name="category"
                className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-orange-400"
              >
                <option value="İsim">İsim</option>
                <option value="Fiil">Fiil</option>
                <option value="Sıfat">Sıfat</option>
                <option value="Zarf">Zarf</option>
                <option value="Zamir">Zamir</option>
                <option value="Edat">Edat</option>
                <option value="Bağlaç">Bağlaç</option>
                <option value="Ünlem">Ünlem</option>
              </Field>
              <ErrorMessage
                name="category"
                component="div"
                className="text-red-500 text-sm mt-1"
              />
            </div>

            <div className="flex space-x-4">
              <button
                type="submit"
                disabled={isSubmitting || addLoading}
                className="flex-1 bg-orange-500 text-white py-2 rounded-lg hover:bg-orange-600 transition disabled:opacity-50"
              >
                {addLoading ? "Adding..." : "Add Word"}
              </button>
              {onCancel && (
                <button
                  type="button"
                  onClick={onCancel}
                  className="flex-1 bg-gray-300 text-gray-800 py-2 rounded-lg hover:bg-gray-400 transition"
                >
                  Cancel
                </button>
              )}
            </div>

            {error && (
              <div className="text-red-600 font-medium mt-2 text-center">
                {error}
              </div>
            )}
          </Form>
        )}
      </Formik>
    </div>
  );
};

export default AddWordComponent;
