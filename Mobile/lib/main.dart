import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Core/Widgets/auth_scaffold.dart';
import 'package:medlink/Features/Login/View/login_screen.dart';

void main() {
  runApp(const MainApp());
}

class MainApp extends StatelessWidget {
  const MainApp({super.key});

  @override
  Widget build(BuildContext context) {
    return ScreenUtilInit(
      designSize: Size(390, 844),
      child: MaterialApp(
        theme: ThemeData(
          scaffoldBackgroundColor: ColorsManager.primaryBackgroundColor,
          textSelectionTheme: TextSelectionThemeData(
            cursorColor: ColorsManager.primaryColor,
            selectionColor: ColorsManager.primaryColor.withValues(alpha: 0.25),
            selectionHandleColor: ColorsManager.primaryColor,
          ),
        ),
        home: LoginScreen(),
      ),
    );
  }
}
