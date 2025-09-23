import React from "react";
import { useDispatch, useSelector } from "react-redux";
import type { AppDispatch, RootState } from "../store/store";
import type { RegisterFormValues } from "../Types/authTypes";
import { registerUserThunk } from "../features/auth/authThunk";
import { Formik, Form, Field, ErrorMessage } from "formik";
import { registerSchema } from "../schemas/registerSchemas";
import { useNavigate } from "react-router-dom";

const RegisterPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { loading, error } = useSelector((state: RootState) => state.auth);
  const navigate = useNavigate();

  const initialValues: RegisterFormValues = {
    name: "",
    surname: "",
    email: "",
    password: "",
    confirmPassword: "",
    level: "A1",
  };

  const handleSubmit = async (values: RegisterFormValues) => {
    try {
      const resultAction = await dispatch(registerUserThunk(values));
      if (registerUserThunk.fulfilled.match(resultAction)) {
        navigate("/login");
      }
    } catch (error) {
      console.log("Register error: ", error);
    }
  };

  return (
    <div>
      <h1>Register</h1>

      <Formik
        initialValues={initialValues}
        validationSchema={registerSchema}
        onSubmit={handleSubmit}
      >
        {({ isSubmitting }) => (
          <Form>
            <div>
              <label>Name</label>
              <Field type="text" name="name" />
              <ErrorMessage name="name" component="div" />
            </div>

            <div>
              <label>Surname</label>
              <Field type="text" name="surname" />
              <ErrorMessage name="surname" component="div" />
            </div>

            <div>
              <label>Email</label>
              <Field type="email" name="email" />
              <ErrorMessage name="email" component="div" />
            </div>

            <div>
              <label>Password</label>
              <Field type="password" name="password" />
              <ErrorMessage name="password" component="div" />
            </div>

            <div>
              <label>Confirm Password</label>
              <Field type="password" name="confirmPassword" />
              <ErrorMessage name="confirmPassword" component="div" />
            </div>

            <div>
              <label>Select your level</label>
              <Field as="select" name="level">
                <option value="A1">A1</option>
                <option value="A2">A2</option>
                <option value="B1">B1</option>
                <option value="B2">B2</option>
                <option value="C1">C1</option>
                <option value="C2">C2</option>
              </Field>
              <ErrorMessage name="level" component="div" />
            </div>

            <button type="submit" disabled={isSubmitting || loading}>
              {loading ? "Registering..." : "Register"}
            </button>

            {error && <div style={{ color: "red" }}>{error}</div>}
          </Form>
        )}
      </Formik>
    </div>
  );
};

export default RegisterPage;
