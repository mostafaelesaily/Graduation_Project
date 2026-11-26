import 'package:go_router/go_router.dart';
import 'package:medlink/Features/Auth/Data/models/register_request_model.dart';
import 'package:medlink/Features/Auth/Presentation/Admin%20Review/View/admin_review.dart';
import 'package:medlink/Features/Auth/Presentation/Login/View/login_screen.dart';
import 'package:medlink/Features/Auth/Presentation/Provider%20Signup/View/provider_signup_screen.dart';
import 'package:medlink/Features/Auth/Presentation/Signup/View/signup_screen.dart';
import 'package:medlink/Features/Home/Presentation/View/home_screen.dart';
import 'package:medlink/Features/Onboarding/Presentation/View/onboarding_screen.dart';

class AppRouter {
  static final GoRouter router = GoRouter(
    initialLocation: '/onboarding',
    routes: [
      GoRoute(
        path: '/onboarding',
        name: 'onboarding',
        builder: (context, state) => const OnboardingScreen(),
      ),
      GoRoute(
        path: '/login',
        name: 'login',
        builder: (context, state) => LoginScreen(),
      ),
      GoRoute(
        path: '/signup',
        name: 'signup',
        builder: (context, state) => SignUpScreen(),
      ),
      GoRoute(
        path: '/signupProvider',
        name: 'signupProvider',
        builder: (context, state) {
          final model = state.extra as RegisterRequestModel;

          return ProviderSignup(model: model);
        },
      ),
      GoRoute(
        path: '/adminReview',
        name: 'adminReview',
        builder: (context, state) => AdminReview(),
      ),
      GoRoute(
        path: '/home',
        name: 'home',
        builder: (context, state) => const HomeScreen(),
      ),
    ],
  );
}
