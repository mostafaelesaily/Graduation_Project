import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:medlink/Features/Appointments/Domain/Repos/appointments_repository.dart';
import 'package:medlink/Features/Appointments/Presentation/Cubit/appointments_cubit.dart';
import 'package:medlink/Features/Auth/Data/models/register_request_model.dart';
import 'package:medlink/Features/Booking/Presentation/Cubit/booking_cubit.dart';
import 'package:medlink/Features/Booking/Presentation/View/booking_screen.dart';
import 'package:medlink/Features/Auth/Presentation/Admin%20Review/View/admin_review.dart';
import 'package:medlink/Features/Auth/Presentation/Login/View/login_screen.dart';
import 'package:medlink/Features/Auth/Presentation/Provider%20Signup/View/provider_signup_screen.dart';
import 'package:medlink/Features/Auth/Presentation/Signup/View/signup_screen.dart';
import 'package:medlink/Features/Home/Presentation/View/home_screen.dart';
import 'package:medlink/Features/Onboarding/Presentation/View/onboarding_screen.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Entites/provider_profile.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Repos/provider_repository.dart';
import 'package:medlink/Features/ProviderProfile/Presentation/View/provider_profile_screen.dart';

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
        builder: (context, state) => BlocProvider(
          create: (context) => AppointmentsCubit(
            appointmentsRepository: context.read<AppointmentsRepository>(),
            providerRepository: context.read<ProviderRepository>(),
          )..load(),
          child: const HomeScreen(),
        ),
      ),
      GoRoute(
        path: '/provider',
        name: 'providerProfile',
        redirect: (context, state) =>
            state.extra is ProviderProfile ? null : '/home',
        builder: (context, state) {
          final profile = state.extra as ProviderProfile;

          return ProviderProfileScreen(profile: profile);
        },
      ),
      GoRoute(
        path: '/book',
        name: 'booking',
        redirect: (context, state) =>
            state.extra is ProviderProfile ? null : '/home',
        builder: (context, state) {
          final profile = state.extra as ProviderProfile;

          return BlocProvider(
            create: (context) => BookingCubit(
              providerRepository: context.read<ProviderRepository>(),
              appointmentsRepository: context.read<AppointmentsRepository>(),
              providerId: profile.id,
            )..loadSlots(),
            child: BookingScreen(profile: profile),
          );
        },
      ),
    ],
  );
}
