
'use client';

import React, { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import API from '@/api/axios';
import { renderGoogleButton } from '@/lib/googleAuth';

const translations = {
  en: {
    backHome: "Back Home",
    welcomeTitle: "Welcome Back!",
    welcomeSubtitle: "Log in to manage your healthcare appointments, records, and access personalized AI recommendations.",
    emailLabel: "Email Address *",
    emailPlaceholder: "you@example.com",
    passwordLabel: "Password *",
    passwordPlaceholder: "Enter your password",
    forgotPass: "Forgot Password?",
    loginBtn: "Log In",
    loggingIn: "Logging In...",
    orText: "OR",
    googleBtn: "Continue with Google",
    noAccount: "Don't have an account?",
    signUp: "Sign Up",
    aiSearch: "AI-Powered Search",
    aiSearchDesc: "Tell us what you need, and we'll find",
    locationBased: "Location Based",
    locationDesc: "Find nearby providers and services.",
    bookAppt: "Book Appointments",
    bookDesc: "Skip the calls, book directly through the app.",
    history: "Your Health History",
    historyDesc: "Keep all your medical information organized in one place."
  },
  ar: {
    backHome: "الرئيسية",
    welcomeTitle: "أهلاً بك مجدداً!",
    welcomeSubtitle: "سجل الدخول لإدارة مواعيد الرعاية الصحية الخاصة بك، السجلات، والوصول إلى توصيات الذكاء الاصطناعي.",
    emailLabel: "البريد الإلكتروني *",
    emailPlaceholder: "you@example.com",
    passwordLabel: "كلمة المرور *",
    passwordPlaceholder: "أدخل كلمة المرور",
    forgotPass: "نسيت كلمة المرور؟",
    loginBtn: "تسجيل الدخول",
    loggingIn: "جاري تسجيل الدخول...",
    orText: "أو",
    googleBtn: "المتابعة باستخدام جوجل",
    noAccount: "ليس لديك حساب؟",
    signUp: "إنشاء حساب",
    aiSearch: "بحث بالذكاء الاصطناعي",
    aiSearchDesc: "أخبرنا بما تحتاجه، وسنساعدك في إيجاده",
    locationBased: "بناءً على الموقع",
    locationDesc: "اعثر على مقدمي الخدمات والخدمات القريبة منك.",
    bookAppt: "حجز المواعيد",
    bookDesc: "تجاوز المكالمات، واحجز مباشرة عبر التطبيق.",
    history: "تاريخك الصحي",
    historyDesc: "احتفظ بجميع معلوماتك الطبية منظمة في مكان واحد."
  }
};

export default function LoginPage() {
  const [lang, setLang] = useState('en');
  const t = translations[lang];

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

  const handleLanguageChange = (e) => {
    const selectedLang = e.target.value;
    setLang(selectedLang);
    
    if (selectedLang === 'ar') {
      document.documentElement.dir = 'rtl';
      document.documentElement.lang = 'ar';
    } else {
      document.documentElement.dir = 'ltr';
      document.documentElement.lang = 'en';
    }
  };

  const handleLogin = async (e) => {
    e.preventDefault();
    setErrorMessage('');
    setSuccessMessage('');

    // Inline Validation for empty fields
    const errors = {};
    if (!email.trim()) errors.email = lang === 'ar' ? 'البريد الإلكتروني مطلوب.' : 'Email Address is required.';
    if (!password) errors.password = lang === 'ar' ? 'كلمة المرور مطلوبة.' : 'Password is required.';

    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      setErrorMessage(lang === 'ar' ? 'يرجى ملء جميع الحقول المطلوبة المحددة باللون الأحمر.' : 'Please fill in all required fields marked in red.');
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

      setSuccessMessage(lang === 'ar' ? 'تم تسجيل الدخول بنجاح! جاري التحويل...' : 'Logged in successfully! Redirecting...');

      setTimeout(() => {
        router.push('/home');
      }, 1500);

    } catch (error) {
      console.error("Full Login Error:", error);

      if (!error.response) {
        setErrorMessage(lang === 'ar' ? 'تعذر الاتصال بالخادم. يرجى التحقق من الشبكة.' : 'Unable to connect to the server. Please check your network or server CORS settings.');
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
        setErrorMessage(lang === 'ar' ? 'البريد الإلكتروني أو كلمة المرور غير صحيحة. يرجى المحاولة مرة أخرى.' : 'Invalid email or password. Please try again.');
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
    <div className="min-h-screen flex flex-col bg-white font-sans">
      
      {/* --- Navbar العلوي بلون خلفية #F7F7F7 (يوجه إلى /landing) --- */}
      <header className="w-full py-4 px-8 border-b border-gray-100 flex justify-between items-center sticky top-0 z-50" style={{ backgroundColor: '#F7F7F7' }}>
        <div className="flex items-center gap-4">
          <Link href="/landing" className="flex items-center">
            <img 
              src="/WhatsApp Image .jpeg" 
              alt="MedLink Logo" 
              className="h-10 w-auto object-contain cursor-pointer"
            />
          </Link>
          <Link 
            href="/landing" 
            className="px-3.5 py-1.5 rounded-lg border border-gray-200 text-xs font-semibold text-gray-700 bg-white hover:bg-gray-50 transition shadow-sm"
          >
            {t.backHome}
          </Link>
        </div>

        {/* اختيار اللغة الحقيقي والتفاعلي */}
        <div className="flex items-center">
          <select 
            onChange={handleLanguageChange}
            value={lang}
            className="px-3 py-1.5 rounded-lg border border-gray-200 text-xs font-semibold text-gray-700 bg-white focus:outline-none cursor-pointer"
          >
            <option value="en">EN 🌐</option>
            <option value="ar">AR 🌐</option>
          </select>
        </div>
      </header>

      {/* --- محتوى الصفحة الرئيسي --- */}
      <div className="flex-grow flex items-center justify-center bg-sky-50/50 p-4 py-8">
        <div className="max-w-6xl w-full bg-white rounded-3xl shadow-xl overflow-hidden flex flex-col md:flex-row">

          {/* Left Side - Logo, Info & Illustration */}
          <div className="hidden md:flex w-5/12 bg-sky-50/70 p-8 flex-col justify-between border-r border-sky-100">
            <div>
              <div className="mb-6 h-12 flex items-center">
                <img 
                  src="/WhatsApp Image .jpeg" 
                  alt="MedLink Logo" 
                  className="h-10 object-contain"
                />
              </div>

              <h2 className="text-xl font-bold text-gray-900 mb-2 leading-snug">
                {t.welcomeTitle}
              </h2>
              <p className="text-[11px] text-gray-500 mb-6 leading-relaxed">
                {t.welcomeSubtitle}
              </p>

              {/* Features List */}
              <div className="space-y-3 mb-6">
                <div className="flex items-start gap-3">
                  <div className="p-1.5 bg-white rounded-xl shadow-sm text-teal-600 text-sm">🧠</div>
                  <div>
                    <h3 className="text-xs font-bold text-gray-800">{t.aiSearch}</h3>
                    <p className="text-[10px] text-gray-400">{t.aiSearchDesc}</p>
                  </div>
                </div>

                <div className="flex items-start gap-3">
                  <div className="p-1.5 bg-white rounded-xl shadow-sm text-teal-600 text-sm">📍</div>
                  <div>
                    <h3 className="text-xs font-bold text-gray-800">{t.locationBased}</h3>
                    <p className="text-[10px] text-gray-400">{t.locationDesc}</p>
                  </div>
                </div>

                <div className="flex items-start gap-3">
                  <div className="p-1.5 bg-white rounded-xl shadow-sm text-teal-600 text-sm">📅</div>
                  <div>
                    <h3 className="text-xs font-bold text-gray-800">{t.bookAppt}</h3>
                    <p className="text-[10px] text-gray-400">{t.bookDesc}</p>
                  </div>
                </div>

                <div className="flex items-start gap-3">
                  <div className="p-1.5 bg-white rounded-xl shadow-sm text-teal-600 text-sm">📁</div>
                  <div>
                    <h3 className="text-xs font-bold text-gray-800">{t.history}</h3>
                    <p className="text-[10px] text-gray-400">{t.historyDesc}</p>
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
            <h2 className="text-2xl font-bold text-gray-900 mb-1">{t.welcomeTitle}</h2>
            <p className="text-xs text-gray-500 mb-6">{t.welcomeSubtitle}</p>

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
                <label className="block text-xs font-bold text-gray-800 mb-1.5">{t.emailLabel}</label>
                <input
                  type="email"
                  name="no-autofill-email"
                  autoComplete="off"
                  value={email}
                  onChange={(e) => {
                    setEmail(e.target.value);
                    if (fieldErrors.email) setFieldErrors({ ...fieldErrors, email: '' });
                  }}
                  placeholder={t.emailPlaceholder}
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
                  <label className="block text-xs font-bold text-gray-800">{t.passwordLabel}</label>
                  <Link href="/forgot-password" className="text-[11px] text-teal-600 font-semibold hover:underline">
                    {t.forgotPass}
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
                    placeholder={t.passwordPlaceholder}
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
                {loading ? t.loggingIn : t.loginBtn}
              </button>
            </form>

            {/* OR Divider */}
            <div className="relative flex py-4 items-center">
              <div className="flex-grow border-t border-gray-100"></div>
              <span className="flex-shrink mx-4 text-gray-400 text-[10px] uppercase font-semibold">{t.orText}</span>
              <div className="flex-grow border-t border-gray-100"></div>
            </div>

            {/* Google Button */}
            <button
              type="button"
              className="w-full py-3 border border-gray-200 rounded-xl text-xs font-semibold text-gray-700 hover:bg-gray-50 flex items-center justify-center gap-2 transition duration-200"
            >
              <span>🌐</span> {t.googleBtn}
            </button>

            {/* Register Link */}
            <p className="text-center text-xs text-gray-500 mt-6">
              {t.noAccount}{' '}
              <Link href="/register" className="text-teal-600 font-bold hover:underline">
                {t.signUp}
              </Link>
            </p>
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

      {/* --- Footer السفلي الاحترافي والمتعدد الأعمدة --- */}
      <footer className="w-full border-t border-gray-100 bg-white py-10 px-8 mt-auto">
        <div className="max-w-6xl mx-auto grid grid-cols-1 md:grid-cols-4 gap-8 mb-8">
          
          <div className="md:col-span-1 space-y-3">
            <div className="flex items-center gap-2">
              <img 
                src="/WhatsApp Image .jpeg" 
                alt="MedLink Logo" 
                className="h-7 w-auto object-contain"
              />
            </div>
            <p className="text-[11px] text-gray-500 leading-relaxed">
              AI-powered healthcare platform connecting patients with top providers and keeping health history seamlessly organized.
            </p>
          </div>

          <div>
            <h4 className="text-xs font-bold text-gray-900 uppercase tracking-wider mb-3">Quick Links</h4>
            <ul className="space-y-2 text-xs text-gray-500">
              <li><Link href="/landing" className="hover:text-teal-600 transition">Home</Link></li>
              <li><Link href="/login" className="hover:text-teal-600 transition">Log In</Link></li>
              <li><Link href="/register" className="hover:text-teal-600 transition">Sign Up</Link></li>
            </ul>
          </div>

          <div>
            <h4 className="text-xs font-bold text-gray-900 uppercase tracking-wider mb-3">Support</h4>
            <ul className="space-y-2 text-xs text-gray-500">
              <li><Link href="/help" className="hover:text-teal-600 transition">Help Center</Link></li>
              <li><Link href="/contact" className="hover:text-teal-600 transition">Contact Support</Link></li>
            </ul>
          </div>

          <div>
            <h4 className="text-xs font-bold text-gray-900 uppercase tracking-wider mb-3">Legal</h4>
            <ul className="space-y-2 text-xs text-gray-500">
              <li><Link href="/privacy" className="hover:text-teal-600 transition">Privacy Policy</Link></li>
              <li><Link href="/terms" className="hover:text-teal-600 transition">Terms of Service</Link></li>
            </ul>
          </div>

        </div>

        <div className="max-w-6xl mx-auto pt-6 border-t border-gray-100 flex flex-col sm:flex-row items-center justify-between gap-4 text-xs text-gray-400">
          <p>© 2026 <span className="font-bold text-sky-900">MedLink</span>. All rights reserved.</p>
          <p className="text-[11px]">Better access. Healthier tomorrows.</p>
        </div>
      </footer>

    </div>
  );
}