
'use client';

import React, { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import API from '@/api/axios';
import { renderGoogleButton } from '@/lib/googleAuth';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  const [fieldErrors, setFieldErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [googleLoading, setGoogleLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const [successMessage, setSuccessMessage] = useState('');

  const router = useRouter();
  const googleButtonRef = useRef(null);

  const handleLogin = async (e) => {
    e.preventDefault();
    setErrorMessage('');
    setSuccessMessage('');

    // Inline Validation for empty fields
    const errors = {};
    if (!email.trim()) errors.email = 'Email Address is required.';
    if (!password) errors.password = 'Password is required.';

    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      setErrorMessage('Please fill in all required fields marked in red.');
      return;
    }

    setFieldErrors({});
    setLoading(true);

    try {
      const response = await API.post('/Account/Login', {
        email,
        password,
      });

      const accessToken = response.data?.accessToken || response.data?.token;
      if (accessToken) {
        localStorage.setItem('token', accessToken);
      }
      if (response.data?.refreshToken) {
        localStorage.setItem('refreshToken', response.data.refreshToken);
      }

      const loggedInName = response.data?.userName || response.data?.name;
      if (loggedInName) {
        localStorage.setItem('userName', loggedInName);
      }

      setSuccessMessage('Logged in successfully! Redirecting...');

      setTimeout(() => {
        router.push('/home');
      }, 1500);

    } catch (error) {
      console.error("Full Login Error:", error);

      if (!error.response) {
        setErrorMessage('Unable to connect to the server. Please check your network or server CORS settings.');
        return;
      }

      const apiData = error.response.data;
      let detailedErrors = [];

      if (apiData?.errors && typeof apiData.errors === 'object') {
        Object.entries(apiData.errors).forEach(([field, msgs]) => {
          const messageStr = Array.isArray(msgs) ? msgs.join(', ') : msgs;
          detailedErrors.push(`${field}: ${messageStr}`);
        });
      } else if (Array.isArray(apiData)) {
        apiData.forEach(err => {
          if (err.description) detailedErrors.push(err.description);
          else if (typeof err === 'string') detailedErrors.push(err);
        });
      } else if (apiData?.message) {
        detailedErrors.push(apiData.message);
      } else if (apiData?.title) {
        detailedErrors.push(apiData.title);
      }

      if (detailedErrors.length > 0) {
        setErrorMessage(detailedErrors.join(' | '));
      } else {
        setErrorMessage('Invalid email or password. Please try again.');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleGoogleToken = async (idToken) => {
    setErrorMessage('');
    setSuccessMessage('');
    setGoogleLoading(true);

    try {
      const response = await API.post('/Account/GoogleSignIn', { idToken });

      const accessToken = response.data?.accessToken;
      if (accessToken) localStorage.setItem('token', accessToken);
      if (response.data?.refreshToken) {
        localStorage.setItem('refreshToken', response.data.refreshToken);
      }

      setSuccessMessage('Logged in successfully! Redirecting...');
      setTimeout(() => {
        router.push('/home');
      }, 1500);
    } catch (error) {
      console.error("Google Sign-In Error:", error);

      if (error.response) {
        const apiData = error.response.data;
        setErrorMessage(apiData?.message || apiData?.title || 'Google sign-in failed. Please try again.');
      } else {
        setErrorMessage(error.message || 'Google sign-in failed.');
      }
    } finally {
      setGoogleLoading(false);
    }
  };

  useEffect(() => {
    const container = googleButtonRef.current;
    if (!container) return;
    renderGoogleButton(container, handleGoogleToken, container.offsetWidth || 300).catch((err) => {
      console.error('Failed to render Google button:', err);
    });
  }, []);

  return (
    <div className="min-h-screen flex items-center justify-center bg-sky-50/50 p-4 font-sans">
      <div className="max-w-6xl w-full bg-white rounded-3xl shadow-xl overflow-hidden flex flex-col md:flex-row">

        {/* Left Side - Logo, Info & Illustration */}
        <div className="hidden md:flex w-5/12 bg-sky-50/70 p-8 flex-col justify-between border-r border-sky-100">
          <div>
            {/* MedLink Image Logo */}
            <div className="mb-6 h-12 flex items-center">
              <img 
                src="/WhatsApp Image .jpeg" 
                alt="MedLink Logo" 
                className="h-10 object-contain"
              />
            </div>

            <h2 className="text-xl font-bold text-gray-900 mb-2 leading-snug">
              Welcome Back!
            </h2>
            <p className="text-[11px] text-gray-500 mb-6 leading-relaxed">
              Log in to manage your healthcare appointments, records, and access personalized AI recommendations.
            </p>

            {/* Features List */}
            <div className="space-y-3 mb-6">
              <div className="flex items-start gap-3">
                <div className="p-1.5 bg-white rounded-xl shadow-sm text-teal-600 text-sm">🧠</div>
                <div>
                  <h3 className="text-xs font-bold text-gray-800">AI-Powered Search</h3>
                  <p className="text-[10px] text-gray-400">Tell us what you need, and we'll find</p>
                </div>
              </div>

              <div className="flex items-start gap-3">
                <div className="p-1.5 bg-white rounded-xl shadow-sm text-teal-600 text-sm">📍</div>
                <div>
                  <h3 className="text-xs font-bold text-gray-800">Location Based</h3>
                  <p className="text-[10px] text-gray-400">Find nearby providers and services.</p>
                </div>
              </div>

              <div className="flex items-start gap-3">
                <div className="p-1.5 bg-white rounded-xl shadow-sm text-teal-600 text-sm">📅</div>
                <div>
                  <h3 className="text-xs font-bold text-gray-800">Book Appointments</h3>
                  <p className="text-[10px] text-gray-400">Skip the calls, book directly through the app.</p>
                </div>
              </div>

              <div className="flex items-start gap-3">
                <div className="p-1.5 bg-white rounded-xl shadow-sm text-teal-600 text-sm">📁</div>
                <div>
                  <h3 className="text-xs font-bold text-gray-800">Your Health History</h3>
                  <p className="text-[10px] text-gray-400">Keep all your medical information organized in one place.</p>
                </div>
              </div>
            </div>
          </div>

          {/* Bottom Healthcare Illustration */}
          <div className="mt-4 flex flex-col items-center">
            <div className="w-full relative h-40">
              <img 
                src="/WhatsApp Image 2026-10-19 at 17.50.37.jpeg" 
                alt="Healthcare Illustration" 
                className="w-full h-full object-contain"
              />
            </div>
            <div className="text-center text-[10px] text-gray-400 font-medium mt-2">
              Better access. Healthier tomorrows.
            </div>
          </div>
        </div>

        {/* Right Side - Login Form */}
        <div className="w-full md:w-7/12 p-8 sm:p-10 bg-white flex flex-col justify-center">
          <h2 className="text-2xl font-bold text-gray-900 mb-1">Welcome Back</h2>
          <p className="text-xs text-gray-500 mb-6">Enter your credentials to access your account</p>

          {errorMessage && (
            <div className="mb-4 p-3.5 bg-red-50 text-red-600 text-xs rounded-xl border border-red-100 font-medium leading-relaxed">
              ⚠️ {errorMessage}
            </div>
          )}
          {successMessage && (
            <div className="mb-4 p-3.5 bg-green-50 text-green-600 text-xs rounded-xl border border-green-100 font-medium leading-relaxed">
              ✅ {successMessage}
            </div>
          )}

          <form onSubmit={handleLogin} className="space-y-4" noValidate autoComplete="off">
            {/* Email Address */}
            <div>
              <label className="block text-xs font-bold text-gray-800 mb-1.5">Email Address *</label>
              <input
                type="email"
                name="no-autofill-email"
                autoComplete="off"
                value={email}
                onChange={(e) => {
                  setEmail(e.target.value);
                  if (fieldErrors.email) setFieldErrors({ ...fieldErrors, email: '' });
                }}
                placeholder="you@example.com"
                className={`w-full px-3.5 py-3 rounded-xl border text-xs focus:outline-none bg-gray-50/30 text-gray-800 ${
                  fieldErrors.email ? 'border-red-500 bg-red-50/10' : 'border-gray-200 focus:border-teal-500'
                }`}
              />
              {fieldErrors.email && (
                <p className="text-[11px] text-red-500 mt-1 font-medium">{fieldErrors.email}</p>
              )}
            </div>

            {/* Password */}
            <div>
              <div className="flex justify-between items-center mb-1.5">
                <label className="block text-xs font-bold text-gray-800">Password *</label>
                <Link href="/forgot-password" className="text-[11px] text-teal-600 font-semibold hover:underline">
                  Forgot Password?
                </Link>
              </div>
              <div className="relative">
                <input
                  type={showPassword ? 'text' : 'password'}
                  name="no-autofill-password"
                  autoComplete="new-password"
                  value={password}
                  onChange={(e) => {
                    setPassword(e.target.value);
                    if (fieldErrors.password) setFieldErrors({ ...fieldErrors, password: '' });
                  }}
                  placeholder="Enter your password"
                  className={`w-full pl-3.5 pr-10 py-3 rounded-xl border text-xs focus:outline-none bg-gray-50/30 text-gray-800 ${
                    fieldErrors.password ? 'border-red-500 bg-red-50/10' : 'border-gray-200 focus:border-teal-500'
                  }`}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600 focus:outline-none"
                >
                  {showPassword ? (
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-7 0-11-8-11-8a18.45 18.45 0 015.06-5.94M9.9 4.24A9.12 9.12 0 0112 4c7 0 11 8 11 8a18.5 18.5 0 01-2.16 3.19m-6.72-1.07a3 3 0 11-4.24-4.24" />
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M3 3l18 18" />
                    </svg>
                  ) : (
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                    </svg>
                  )}
                </button>
              </div>
              {fieldErrors.password && (
                <p className="text-[11px] text-red-500 mt-1 font-medium">{fieldErrors.password}</p>
              )}
            </div>

            {/* Login Button */}
            <button
              type="submit"
              disabled={loading}
              className="w-full py-3.5 bg-sky-900 hover:bg-sky-950 text-white rounded-xl text-xs font-bold transition duration-200 shadow-md shadow-sky-900/10 mt-2"
            >
              {loading ? 'Logging In...' : 'Log In'}
            </button>
          </form>

          {/* OR Divider */}
          <div className="relative flex py-4 items-center">
            <div className="flex-grow border-t border-gray-100"></div>
            <span className="flex-shrink mx-4 text-gray-400 text-[10px] uppercase font-semibold">OR</span>
            <div className="flex-grow border-t border-gray-100"></div>
          </div>

          {/* Google Button */}
          <div className="relative w-full">
            <button
              type="button"
              aria-hidden="true"
              tabIndex={-1}
              className="w-full py-3 border border-gray-200 rounded-xl text-xs font-semibold text-gray-700 flex items-center justify-center gap-2 pointer-events-none"
            >
              <span>🌐</span> {googleLoading ? 'Connecting...' : 'Continue with Google'}
            </button>
            <div ref={googleButtonRef} className="absolute inset-0 z-10 overflow-hidden opacity-0" />
          </div>

          {/* Register Link */}
          <p className="text-center text-xs text-gray-500 mt-6">
            Don't have an account?{' '}
            <Link href="/register" className="text-teal-600 font-bold hover:underline">
              Sign Up
            </Link>
          </p>
        </div>

      </div>
    </div>
  );
}