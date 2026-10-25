import 'package:go_router/go_router.dart';
import 'package:medlink/Features/Auth/Presentation/Login/View/login_screen.dart';
import 'package:medlink/Features/Auth/Presentation/Signup/View/signup_screen.dart';

class AppRouter {
  static final GoRouter router = GoRouter(
    initialLocation: '/login',
    routes: [
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
    ],
  );
}
