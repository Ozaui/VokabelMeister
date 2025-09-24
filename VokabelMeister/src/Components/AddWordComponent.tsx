import React from "react";
import { useDispatch, useSelector } from "react-redux";
import { type AppDispatch, type RootState } from "../store/store";
import type { AddWordPayload } from "../Types/wordTypes";
import { addWordThunk } from "../features/word/wordThunk";
import { newWordSchema } from "../schemas/addWordSchema";
import { ErrorMessage, Field, Form, Formik, type FormikHelpers } from "formik";

const AddWordComponent: React.FC = () => {
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
    }
  };

  return (
    <div>
      <h1>Add New Word</h1>
      <Formik
        initialValues={initialValues}
        validationSchema={newWordSchema}
        onSubmit={handleSubmit}
      >
        {({ isSubmitting }) => (
          <Form>
            <div>
              <label>German</label>
              <Field type="text" name="german" />
              <ErrorMessage name="german" component="div" />
            </div>

            <div>
              <label>Turkish</label>
              <Field type="text" name="turkish" />
              <ErrorMessage name="turkish" component="div" />
            </div>

            <div>
              <label>Sample Sentence</label>
              <Field type="text" name="sampleSentence" />
              <ErrorMessage name="sampleSentence" component="div" />
            </div>

            <div>
              <label>Category</label>
              <Field as="select" name="category">
                <option value="İsim">İsim</option>
                <option value="Fiil">Fiil</option>
                <option value="Sıfat">Sıfat</option>
                <option value="Zarf">Zarf</option>
                <option value="Zamir">Zamir</option>
                <option value="Edat">Edat</option>
                <option value="Bağlaç">Bağlaç</option>
                <option value="Ünlem">Ünlem</option>
              </Field>
              <ErrorMessage name="category" component="div" />
            </div>

            <button type="submit" disabled={isSubmitting || addLoading}>
              {addLoading ? "Adding..." : "Add Word"}
            </button>
            {error && <h1>{error}</h1>}
          </Form>
        )}
      </Formik>
    </div>
  );
};

export default AddWordComponent;
