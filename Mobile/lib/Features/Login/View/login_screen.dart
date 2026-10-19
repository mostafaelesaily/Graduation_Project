import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Shared/auth_button.dart';
import 'package:medlink/Core/Shared/google_button.dart';
import 'package:medlink/Core/Shared/input_field.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Core/Style/text_styles.dart';
import 'package:medlink/Core/Widgets/auth_scaffold.dart';

class LoginScreen extends StatefulWidget {
  new({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  late final GlobalKey<FormState> _formKey;
  late final TextEditingController _emailController;
  late final TextEditingController _passwordController;
  bool isChecked = false;

  @override
  void initState() {
    super.initState();
    _formKey = GlobalKey<FormState>();
    _emailController = TextEditingController();
    _passwordController = TextEditingController();
  }

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AuthScaffold(
      isCentered: true,
      logo: 'assets/logo/logo.png',
      title: 'Welcome Back',
      titleDescription: 'Log in to manage your health appointments and records',
      child: Form(
        key: _formKey,
        child: Column(
          spacing: 14.w,
          children: [
            InputField(
              controller: _emailController,
              validator: (value) {
                if (value == null || value == '') {
                  return 'Email is required';
                }
                return null;
              },
              isRequired: true,
              labelText: 'Email Address',
              hintText: 'Enter your Email',
              icon: Icons.email_outlined,
            ),
            InputField(
              controller: _passwordController,
              validator: (value) {
                if (value == null || value == '') {
                  return 'Password is required';
                }
                return null;
              },
              isRequired: true,
              labelText: 'Password',
              hintText: 'Enter your Password',
              isObsecure: true,
              icon: Icons.lock_outline,
            ),
            Row(
              mainAxisAlignment: .spaceBetween,
              children: [
                Row(
                  mainAxisAlignment: .start,
                  spacing: 8.w,
                  children: [
                    GestureDetector(
                      onTap: () {
                        setState(() {
                          isChecked = !isChecked;
                        });
                      },
                      child: Container(
                        width: 16,
                        height: 16,
                        decoration: BoxDecoration(
                          color: isChecked
                              ? ColorsManager.primaryColor
                              : Colors.white,
                          border: Border.all(color: ColorsManager.primaryColor),
                          borderRadius: BorderRadius.circular(4),
                        ),
                        child: isChecked
                            ? const Icon(
                                Icons.check,
                                size: 11,
                                color: Colors.white,
                              )
                            : null,
                      ),
                    ),
                    Text('Remember me', style: TextStyle(fontSize: 11)),
                  ],
                ),
                TextButton(
                  style: TextButton.styleFrom(
                    minimumSize: Size(0, 16),
                    maximumSize: Size(double.infinity, 16),
                    padding: EdgeInsets.zero,
                    tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                    visualDensity: VisualDensity.compact,
                  ),
                  onPressed: () {},
                  child: Text(
                    'Forgot Password?',
                    style: TextStyle(
                      fontSize: 11,
                      fontWeight: .bold,
                      color: ColorsManager.primaryColor,
                    ),
                  ),
                ),
              ],
            ),
            AuthButton(
              onPressed: () {
                if (_formKey.currentState!.validate()) {
                  _emailController.clear();
                  _passwordController.clear();
                  _formKey.currentState!.reset();
                }
              },
              text: 'Log in',
            ),
            Stack(
              alignment: .center,
              children: [
                Divider(color: Color(0xffddecfa)),
                Container(
                  color: ColorsManager.primaryBackgroundColor,
                  padding: EdgeInsets.symmetric(horizontal: 12),
                  child: Text(
                    'OR',
                    style: TextStyles.fadedText.copyWith(
                      fontWeight: .bold,
                      fontSize: 10,
                    ),
                  ),
                ),
              ],
            ),
            GoogleButton(onPressed: () {}),

            Row(
              mainAxisAlignment: .center,
              children: [
                Text(
                  'Don\'t have an account? ',
                  style: TextStyle(
                    fontSize: 12,
                    color: ColorsManager.fadedText,
                  ),
                ),
                TextButton(
                  style: TextButton.styleFrom(
                    minimumSize: Size(0, 16),
                    maximumSize: Size(double.infinity, 16),
                    padding: EdgeInsets.zero,
                    tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                    visualDensity: VisualDensity.compact,
                  ),
                  onPressed: () {},
                  child: Text(
                    'Sign Up',
                    style: TextStyle(
                      fontSize: 12,
                      fontWeight: .bold,
                      color: ColorsManager.primaryColor,
                    ),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
