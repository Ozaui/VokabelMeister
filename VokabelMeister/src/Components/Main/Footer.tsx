import React from "react";
import { FaGithub, FaInstagram, FaLinkedin } from "react-icons/fa";

const Footer: React.FC = () => {
  return (
    <footer className="bg-gray-100 py-12 px-6">
      <div className="container mx-auto max-w-7xl">
        {/* Footer Top */}
        <div className="flex flex-col md:flex-row justify-between items-center md:items-start mb-12 border-b border-gray-300 pb-8">
          {/* Logo */}
          <div className="mb-8 md:mb-0">
            <span className="text-2xl font-bold text-orange-500">
              Vokabel Meister
            </span>
            <p className="mt-2 text-gray-600 max-w-sm">
              A German vocabulary learning app built by a junior full-stack
              developer to improve skills and grow as a developer.
            </p>
          </div>

          {/* Quick Links */}
          <div className="flex flex-col sm:flex-row gap-8 sm:gap-16">
            <div>
              <h4 className="text-lg font-semibold text-gray-800 mb-4">
                Quick Links
              </h4>
              <ul className="space-y-2">
                <li>
                  <a
                    href="#"
                    className="text-gray-600 hover:text-orange-500 transition-colors duration-300"
                  >
                    Courses
                  </a>
                </li>
                <li>
                  <a
                    href="#"
                    className="text-gray-600 hover:text-orange-500 transition-colors duration-300"
                  >
                    About Us
                  </a>
                </li>
                <li>
                  <a
                    href="#"
                    className="text-gray-600 hover:text-orange-500 transition-colors duration-300"
                  >
                    Contact
                  </a>
                </li>
                <li>
                  <a
                    href="#"
                    className="text-gray-600 hover:text-orange-500 transition-colors duration-300"
                  >
                    Pricing
                  </a>
                </li>
              </ul>
            </div>
          </div>
        </div>

        {/* Footer Bottom */}
        <div className="flex flex-col md:flex-row justify-between items-center text-center md:text-left">
          {/* Copyright */}
          <p className="text-gray-500 text-sm mb-4 md:mb-0">
            &copy; {new Date().getFullYear()} Vokabel Meister. All Rights
            Reserved.
          </p>

          {/* Social Icons */}
          <div className="flex space-x-4">
            <a
              href="https://github.com/Ozaui"
              aria-label="GitHub"
              className="text-gray-600 hover:text-orange-500 transition-colors duration-300"
            >
              <FaGithub size={24} />
            </a>
            <a
              href="https://www.instagram.com/melihisdevil"
              aria-label="Instagram"
              className="text-gray-600 hover:text-orange-500 transition-colors duration-300"
            >
              <FaInstagram size={24} />
            </a>
            <a
              href="https://www.linkedin.com/in/ozay-melih-yildiz"
              aria-label="LinkedIn"
              className="text-gray-600 hover:text-orange-500 transition-colors duration-300"
            >
              <FaLinkedin size={24} />
            </a>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
