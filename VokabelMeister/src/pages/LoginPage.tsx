import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import type { AppDispatch, RootState } from "../store/store";
import { useNavigate } from "react-router-dom";
import type { LoginFormValues } from "../Types/authTypes";
import { loginUserThunk } from "../features/auth/authThunk";
import { ErrorMessage, Field, Form, Formik } from "formik";
import { loginSchema } from "../schemas/loginSchema";
import { checkTokenExpiration } from "../API/auth/authApi";

import HeaderForLogin from "../Components/HeaderForLogin";
import Footer from "../Components/Footer";

const LoginPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { loading, error } = useSelector((state: RootState) => state.auth);

  // Sayfa yüklendiğinde token kontrolü yap
  useEffect(() => {
    checkTokenExpiration();
  }, []);

  const initialValues: LoginFormValues = {
    email: "",
    password: "",
  };

  const handleSubmit = async (values: LoginFormValues) => {
    const resultAction = await dispatch(loginUserThunk(values));
    if (loginUserThunk.fulfilled.match(resultAction)) {
      navigate("/words");
    }
  };

  return (
    <div>
      <HeaderForLogin />
      <div className="flex min-h-screen items-center justify-center bg-gray-50 p-4">
        <div className="w-full max-w-md rounded-xl bg-white p-8 shadow-2xl">
          {/* Başlık */}
          <h1 className="text-center text-3xl font-bold text-gray-800 mb-8">
            Welcome Back to{" "}
            <span className="text-orange-500">Vokabel Meister</span>
          </h1>

          <Formik
            initialValues={initialValues}
            validationSchema={loginSchema}
            onSubmit={handleSubmit}
          >
            {({ isSubmitting }) => (
              <Form className="space-y-6">
                <div>
                  <label
                    htmlFor="email"
                    className="block text-sm font-medium text-gray-700 mb-1"
                  >
                    Email Address
                  </label>
                  <Field
                    type="email"
                    name="email"
                    id="email"
                    className="w-full rounded-lg border border-gray-300 p-3 shadow-sm focus:border-orange-500 focus:ring-orange-500 transition duration-150"
                    placeholder="you@example.com"
                  />
                  <ErrorMessage
                    name="email"
                    component="div"
                    className="mt-1 text-sm text-red-600"
                  />
                </div>

                <div>
                  <label
                    htmlFor="password"
                    className="block text-sm font-medium text-gray-700 mb-1"
                  >
                    Password
                  </label>
                  <Field
                    type="password"
                    name="password"
                    id="password"
                    className="w-full rounded-lg border border-gray-300 p-3 shadow-sm focus:border-orange-500 focus:ring-orange-500 transition duration-150"
                    placeholder="••••••••"
                  />
                  <ErrorMessage
                    name="password"
                    component="div"
                    className="mt-1 text-sm text-red-600"
                  />
                </div>

                <button
                  type="submit"
                  disabled={isSubmitting || loading}
                  className={`w-full py-3 px-4 rounded-full font-bold text-white transition-colors duration-300 shadow-lg ${
                    isSubmitting || loading
                      ? "bg-gray-400 cursor-not-allowed"
                      : "bg-orange-500 hover:bg-orange-600"
                  }`}
                >
                  {loading ? "Logging in..." : "Login"}
                </button>

                {error && (
                  <div className="mt-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded-lg text-center">
                    {error}
                  </div>
                )}

                <p className="mt-4 text-center text-sm text-gray-600">
                  Don't have an account?
                  <button
                    type="button"
                    onClick={() => navigate("/register")}
                    className="font-semibold text-orange-500 hover:text-orange-600 ml-1"
                  >
                    Sign Up
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

export default LoginPage;
