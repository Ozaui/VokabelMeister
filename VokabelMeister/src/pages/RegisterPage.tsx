import React from "react";
import { useDispatch, useSelector } from "react-redux";
import type { AppDispatch, RootState } from "../store/store";
import type { RegisterFormValues } from "../Types/authTypes";
import { registerUserThunk } from "../features/auth/authThunk";
import { Formik, Form, Field, ErrorMessage } from "formik";
import { registerSchema } from "../schemas/registerSchemas";
import { useNavigate } from "react-router-dom";
import Footer from "../Components/Footer";
import HeaderForLogin from "../Components/HeaderForLogin";

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

  const inputClass =
    "w-full rounded-lg border border-gray-300 p-3 shadow-sm focus:border-orange-500 focus:ring-orange-500 transition duration-150";
  const labelClass = "block text-sm font-medium text-gray-700 mb-1";
  const errorClass = "mt-1 text-sm text-red-600";

  return (
    <div>
      <HeaderForLogin />
      <div className="flex min-h-screen items-center justify-center bg-gray-50 p-4">
        <div className="w-full max-w-lg rounded-xl bg-white p-8 shadow-2xl">
          <h1 className="text-center text-3xl font-bold text-gray-800 mb-4">
            Join <span className="text-orange-500">Vokabel Meister</span>
          </h1>
          <p className="text-center text-gray-600 mb-8">
            Start your language journey in a fun and effective way!
          </p>

          <Formik
            initialValues={initialValues}
            validationSchema={registerSchema}
            onSubmit={handleSubmit}
          >
            {({ isSubmitting }) => (
              <Form className="space-y-5">
                {/* İsim ve Soyisim Gruplaması */}
                <div className="flex gap-4">
                  <div className="w-1/2">
                    <label htmlFor="name" className={labelClass}>
                      Name
                    </label>
                    <Field
                      type="text"
                      name="name"
                      id="name"
                      className={inputClass}
                      placeholder="First Name"
                    />
                    <ErrorMessage
                      name="name"
                      component="div"
                      className={errorClass}
                    />
                  </div>
                  <div className="w-1/2">
                    <label htmlFor="surname" className={labelClass}>
                      Surname
                    </label>
                    <Field
                      type="text"
                      name="surname"
                      id="surname"
                      className={inputClass}
                      placeholder="Last Name"
                    />
                    <ErrorMessage
                      name="surname"
                      component="div"
                      className={errorClass}
                    />
                  </div>
                </div>

                {/* Email Alanı */}
                <div>
                  <label htmlFor="email" className={labelClass}>
                    Email Address
                  </label>
                  <Field
                    type="email"
                    name="email"
                    id="email"
                    className={inputClass}
                    placeholder="you@example.com"
                  />
                  <ErrorMessage
                    name="email"
                    component="div"
                    className={errorClass}
                  />
                </div>

                {/* Şifre Alanları Gruplaması */}
                <div className="flex gap-4">
                  <div className="w-1/2">
                    <label htmlFor="password" className={labelClass}>
                      Password
                    </label>
                    <Field
                      type="password"
                      name="password"
                      id="password"
                      className={inputClass}
                      placeholder="••••••••"
                    />
                    <ErrorMessage
                      name="password"
                      component="div"
                      className={errorClass}
                    />
                  </div>
                  <div className="w-1/2">
                    <label htmlFor="confirmPassword" className={labelClass}>
                      Confirm Password
                    </label>
                    <Field
                      type="password"
                      name="confirmPassword"
                      id="confirmPassword"
                      className={inputClass}
                      placeholder="••••••••"
                    />
                    <ErrorMessage
                      name="confirmPassword"
                      component="div"
                      className={errorClass}
                    />
                  </div>
                </div>

                {/* Dil Seviyesi Alanı */}
                <div>
                  <label htmlFor="level" className={labelClass}>
                    Select your current level
                  </label>
                  <Field
                    as="select"
                    name="level"
                    id="level"
                    className={inputClass}
                  >
                    <option value="A1">A1 - Beginner</option>
                    <option value="A2">A2 - Elementary</option>
                    <option value="B1">B1 - Intermediate</option>
                    <option value="B2">B2 - Upper Intermediate</option>
                    <option value="C1">C1 - Advanced</option>
                    <option value="C2">C2 - Proficiency</option>
                  </Field>
                  <ErrorMessage
                    name="level"
                    component="div"
                    className={errorClass}
                  />
                </div>

                {/* Kayıt Butonu */}
                <button
                  type="submit"
                  disabled={isSubmitting || loading}
                  className={`w-full py-3 px-4 rounded-full font-bold text-white transition-colors duration-300 shadow-lg mt-6 ${
                    isSubmitting || loading
                      ? "bg-gray-400 cursor-not-allowed"
                      : "bg-orange-500 hover:bg-orange-600"
                  }`}
                >
                  {loading ? "Registering..." : "Create Account"}
                </button>

                {/* Hata Mesajı */}
                {error && (
                  <div className="mt-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded-lg text-center">
                    {error}
                  </div>
                )}

                {/* Giriş Yapma Linki */}
                <p className="mt-4 text-center text-sm text-gray-600">
                  Already have an account?
                  <button
                    type="button"
                    onClick={() => navigate("/login")}
                    className="font-semibold text-orange-500 hover:text-orange-600 ml-1"
                  >
                    Log In
                  </button>
                </p>
              </Form>
            )}
          </Formik>
        </div>
      </div>
      <Footer />
    </div>
  );
};

export default RegisterPage;
