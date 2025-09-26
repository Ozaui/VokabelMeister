import React from "react";
import Header from "../Components/Header";
import Footer from "../Components/Footer";

const AboutPage: React.FC = () => {
  // Backend teknolojileri (dependencies'den seçilmiş ana teknolojiler)
  const backendSkills = [
    "Node.js",
    "Express.js",
    "MongoDB",
    "Mongoose",
    "bcrypt",
    "JWT",
  ];

  // Frontend teknolojileri (dependencies'den seçilmiş ana teknolojiler)
  const frontendSkills = [
    "React",
    "TypeScript",
    "Tailwind CSS",
    "Redux Toolkit ",
    "React Router DOM",
    "Formik",
    "Yup",
    "Axios",
  ];

  return (
    <div>
      <Header />
      <div className="flex min-h-[85vh] items-center justify-center bg-gray-50 p-6">
        <div className="w-full max-w-4xl rounded-xl bg-white p-8 md:p-12 shadow-2xl">
          <h1 className="text-center text-4xl font-bold text-gray-800 mb-4">
            Hi there! I'm the Developer Özay Melih Yıldız
          </h1>
          <h2 className="text-center text-xl text-orange-500 font-semibold mb-10">
            Let me tell you a bit about <strong>Vokabel Meister</strong> and
            myself.
          </h2>

          <div className="text-lg text-gray-700 space-y-6">
            <p>
              Welcome! I'm the person who built this entire platform, and
              honestly, <strong>Vokabel Meister</strong> started as my own
              little passion project. Like you, I was diving deep into learning
              <strong>German</strong>, and I quickly realized I needed a better,
              faster, and smarter way to nail down all that vocabulary. So, I
              decided to build it myself!
            </p>

            <div className="p-4 bg-orange-50 border-l-4 border-orange-500 rounded-lg">
              <h3 className="text-2xl font-semibold text-gray-800 mb-2">
                Born from a Need, Built with Code
              </h3>
              <p>
                This entire app is proof that sometimes the best solutions come
                from personal struggles. While I was focused on memorizing
                German nouns and articles, I was also pushing myself to grow as
                a developer. This project became my ultimate playground,
                allowing me to significantly level up my
                <strong> Full-stack Development</strong> game in a very
                practical way. It was a win-win!
              </p>
            </div>

            <h3 className="text-2xl font-semibold text-gray-800 mt-8 mb-4">
              My Day Job: Full-Stack Developer
            </h3>
            <p>
              I'm a <strong>Computer Engineering</strong> graduate, which
              basically means I love building things and solving puzzles with
              code. I'm a<strong> Full-Stack Developer</strong>, and I genuinely
              enjoy making sure everything—from the button you click to the
              database storing your progress—works flawlessly and fast.
            </p>

            <h3 className="text-2xl font-semibold text-gray-800 mt-8 mb-4">
              The Tools I Used: Frontend
            </h3>
            <p>
              For the user interface, I used my favorite modern tools to create
              a smooth, responsive, and type-safe experience:
            </p>

            <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
              {frontendSkills.map((skill) => (
                <span
                  key={skill}
                  className="inline-block bg-orange-500/10 text-orange-700 px-4 py-2 rounded-full text-center font-medium hover:bg-orange-600/20 transition duration-300 border border-orange-200"
                >
                  {skill}
                </span>
              ))}
            </div>

            <h3 className="text-2xl font-semibold text-gray-800 mt-8 mb-4">
              The Tools I Used: Backend
            </h3>
            <p>
              The application's engine is built with a reliable and fast
              JavaScript stack, ensuring secure user authentication and
              efficient data handling:
            </p>

            <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
              {backendSkills.map((skill) => (
                <span
                  key={skill}
                  className="inline-block bg-gray-600/10 text-gray-800 px-4 py-2 rounded-full text-center font-medium hover:bg-gray-700/20 transition duration-300 border border-gray-300"
                >
                  {skill}
                </span>
              ))}
            </div>

            {/* Closing */}
            <p className="pt-6 text-xl font-medium text-gray-800">
              I'm constantly working on new features to make our learning
              experience even better. Thanks for being here, and happy learning!
              Let's conquer that vocabulary together!
            </p>
          </div>
        </div>
      </div>
      <Footer />
    </div>
  );
};

export default AboutPage;
