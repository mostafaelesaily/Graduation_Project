import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import 'package:medlink/Core/DI/dependecies.dart';
import 'package:medlink/Core/Router/app_router.dart';
import 'package:medlink/Core/Style/colors_manager.dart';

import 'package:medlink/Features/Auth/Domain/Repos/auth_repository.dart';
import 'package:medlink/Features/Auth/Presentation/Login/View/login_screen.dart';
import 'package:medlink/Features/Auth/Presentation/Login/cubit/login_cubit.dart';
import 'package:medlink/Features/Auth/Presentation/Signup/Cubit/signup_cubit.dart';
import 'package:medlink/Features/Auth/Presentation/Signup/View/signup_screen.dart';

void main() {
  final dependencies = Dependencies();

  runApp(MainApp(dependencies: dependencies));
}

class MainApp extends StatelessWidget {
  final Dependencies dependencies;

  const MainApp({super.key, required this.dependencies});

  @override
  Widget build(BuildContext context) {
    return ScreenUtilInit(
      designSize: const Size(390, 844),
      child: RepositoryProvider<AuthRepository>.value(
        value: dependencies.authRepository,
        child: MultiBlocProvider(
          providers: [
            BlocProvider(
              create: (context) =>
                  LoginCubit(authRepository: context.read<AuthRepository>()),
            ),
            BlocProvider(
              create: (context) =>
                  SignupCubit(authRepository: context.read<AuthRepository>()),
            ),
          ],
          child: MaterialApp.router(
            debugShowCheckedModeBanner: false,
            routerConfig: AppRouter.router,
            theme: ThemeData(
              scaffoldBackgroundColor: ColorsManager.primaryBackgroundColor,
              textSelectionTheme: TextSelectionThemeData(
                cursorColor: ColorsManager.primaryColor,
                selectionColor: ColorsManager.primaryColor.withValues(
                  alpha: 0.25,
                ),
                selectionHandleColor: ColorsManager.primaryColor,
              ),
              fontFamily: 'LiberationSans',
            ),
          ),
        ),
      ),
    );
  }
}
