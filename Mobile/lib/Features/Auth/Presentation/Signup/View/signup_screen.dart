import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:go_router/go_router.dart';
import 'package:medlink/Core/Dialogs/error_dialog.dart';
import 'package:medlink/Core/Shared/auth_button.dart';
import 'package:medlink/Core/Shared/google_button.dart';
import 'package:medlink/Core/Shared/input_field.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Core/Style/text_styles.dart';
import 'package:medlink/Core/Widgets/auth_scaffold.dart';
import 'package:medlink/Features/Auth/Data/models/register_request_model.dart';
import 'package:medlink/Features/Auth/Presentation/Signup/Cubit/signup_cubit.dart';
import 'package:medlink/Features/Auth/Presentation/Signup/Cubit/signup_state.dart';

enum AccountRole { patient, provider }

class SignUpScreen extends StatefulWidget {
  const SignUpScreen({super.key});

  @override
  State<SignUpScreen> createState() => _SignUpScreenState();
}

class _SignUpScreenState extends State<SignUpScreen> {
  final _formKey = GlobalKey<FormState>();

  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  final _phoneController = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();
  final _serviceTypeContorller = TextEditingController();
  final _locationContorller = TextEditingController();

  AccountRole _selectedRole = AccountRole.patient;
  bool _isPasswordVisible = false;
  bool _isConfirmPasswordVisible = false;

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    _phoneController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    _serviceTypeContorller.dispose();
    _locationContorller.dispose();
    super.dispose();
  }

  String? _requiredValidator(String? value, String field) {
    if (value == null || value.trim().isEmpty) {
      return '$field is required';
    }
    return null;
  }

  String? _validateEmail(String? value) {
    if (value == null || value.trim().isEmpty) {
      return 'Email is required';
    }

    if (!RegExp(r'^[^@\s]+@[^@\s]+\.[^@\s]+$').hasMatch(value.trim())) {
      return 'Enter a valid email address';
    }

    return null;
  }

  String? _validatePassword(String? value) {
    if (value == null || value.isEmpty) {
      return 'Password is required';
    }

    if (value.length < 8) {
      return 'Password must be at least 8 characters';
    }

    return null;
  }

  String? _validateConfirmPassword(String? value) {
    if (value == null || value.isEmpty) {
      return 'Please confirm your password';
    }

    if (value != _passwordController.text) {
      return 'Passwords do not match';
    }

    return null;
  }

  void _signUp() {
    if (!_formKey.currentState!.validate()) return;

    context.read<SignupCubit>().signup(
      RegisterRequestModel(
        accountRole: _selectedRole.name,
        serviceType: _serviceTypeContorller.text,
        location: _locationContorller.text,
        name: _nameController.text,
        email: _emailController.text,
        phone: _phoneController.text,
        password: _passwordController.text,
      ),
    );

    _serviceTypeContorller.clear();
    _locationContorller.clear();
    _nameController.clear();
    _emailController.clear();
    _phoneController.clear();
    _passwordController.clear();
    _confirmPasswordController.clear();
  }

  @override
  Widget build(BuildContext context) {
    return AuthScaffold(
      isCentered: false,
      logo: 'assets/logo/logo.png',
      title: 'Create Your Account',
      titleDescription: 'Choose your role to get started',
      child: Form(
        key: _formKey,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          spacing: 14.h,
          children: [
            // Account role selector
            Row(
              children: [
                Expanded(
                  child: _RoleCard(
                    title: 'Patient / User',
                    description: 'Find and book healthcare services',
                    icon: Icons.person,
                    selected: _selectedRole == AccountRole.patient,
                    onTap: () {
                      setState(() {
                        _selectedRole = AccountRole.patient;
                      });
                    },
                  ),
                ),
                SizedBox(width: 12.w),
                Expanded(
                  child: _RoleCard(
                    title: 'Provider',
                    description: 'Manage your services and reach more patients',
                    icon: Icons.medical_services_outlined,
                    selected: _selectedRole == AccountRole.provider,
                    onTap: () {
                      setState(() {
                        _selectedRole = AccountRole.provider;
                      });
                    },
                  ),
                ),
              ],
            ),

            SizedBox(height: 4.h),

            if (_selectedRole == AccountRole.provider) ...[
              InputField(
                controller: _serviceTypeContorller,
                validator: (value) => _requiredValidator(value, 'Service Type'),
                isRequired: true,
                labelText: 'Service Type',
                hintText: 'e.g. Internal Medicine',
                icon: Icons.medical_services_outlined,
              ),
              InputField(
                controller: _locationContorller,
                validator: (value) => _requiredValidator(value, 'Location'),
                isRequired: true,
                labelText: 'Location',
                hintText: 'e.g. Cairo',
                icon: Icons.location_on_outlined,
              ),
            ],

            InputField(
              controller: _nameController,
              validator: (value) => _requiredValidator(value, 'Full name'),
              isRequired: true,
              labelText: 'Full Name',
              hintText: 'Enter your full name',
              icon: Icons.person_outline,
            ),

            InputField(
              controller: _emailController,
              validator: _validateEmail,
              isRequired: true,
              labelText: 'Email Address',
              hintText: 'you@example.com',
              icon: Icons.email_outlined,
            ),

            InputField(
              controller: _phoneController,
              validator: (value) {
                final error = _requiredValidator(value, 'Phone number');

                if (error != null) return error;

                if (value!.trim().length < 7) {
                  return 'Enter a valid phone number';
                }

                return null;
              },
              isRequired: true,
              labelText: 'Phone Number',
              hintText: '+20 123 456 7890',
              icon: Icons.phone_outlined,
            ),

            InputField(
              controller: _passwordController,
              validator: _validatePassword,
              isRequired: true,
              labelText: 'Password',
              hintText: 'Create a strong password',
              isObsecure: !_isPasswordVisible,
              icon: Icons.lock_outline,
              issuffixShown: true,
            ),

            InputField(
              controller: _confirmPasswordController,
              validator: _validateConfirmPassword,
              isRequired: true,
              labelText: 'Confirm Password',
              hintText: 'Confirm your password',
              isObsecure: !_isConfirmPasswordVisible,
              icon: Icons.lock_outline,
              issuffixShown: true,
            ),

            SizedBox(height: 4.h),

            BlocConsumer<SignupCubit, SignupState>(
              listener: (context, state) {
                if (state.status == SignupStatus.success) {}

                if (state.status == SignupStatus.failure) {
                  showDialog(
                    context: context,
                    builder: (context) {
                      return ErrorDialog(error: state.errorMessage.toString());
                    },
                  );
                }
              },
              builder: (context, state) {
                return AuthButton(
                  onPressed: _signUp,
                  text: state.status == SignupStatus.loading ? null : 'Sign Up',
                  icon: state.status == SignupStatus.loading
                      ? CircularProgressIndicator()
                      : null,
                );
              },
            ),

            Stack(
              alignment: Alignment.center,
              children: [
                const Divider(color: Color(0xffddecfa)),
                Container(
                  color: ColorsManager.primaryBackgroundColor,
                  padding: EdgeInsets.symmetric(horizontal: 12.w),
                  child: Text(
                    'OR',
                    style: TextStyles.fadedText.copyWith(
                      fontWeight: FontWeight.bold,
                      fontSize: 10.sp,
                    ),
                  ),
                ),
              ],
            ),

            GoogleButton(onPressed: () {}),

            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Flexible(
                  child: Text(
                    'Already have an account? ',
                    style: TextStyle(
                      fontSize: 12.sp,
                      color: ColorsManager.fadedText,
                    ),
                  ),
                ),
                TextButton(
                  style: TextButton.styleFrom(
                    minimumSize: Size.zero,
                    padding: EdgeInsets.zero,
                    tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                    visualDensity: VisualDensity.compact,
                  ),
                  onPressed: () {
                    context.goNamed('login');
                  },
                  child: Text(
                    'Log In',
                    style: TextStyle(
                      fontSize: 12.sp,
                      fontWeight: FontWeight.bold,
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

class _RoleCard extends StatelessWidget {
  final String title;
  final String description;
  final IconData icon;
  final bool selected;
  final VoidCallback onTap;

  const _RoleCard({
    required this.title,
    required this.description,
    required this.icon,
    required this.selected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final primary = ColorsManager.primaryColor;

    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(20.r),
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 180),
        padding: EdgeInsets.all(12.w),
        decoration: BoxDecoration(
          color: selected ? primary.withValues(alpha: 0.05) : Colors.white,
          borderRadius: BorderRadius.circular(20.r),
          border: Border.all(
            color: selected ? primary : const Color(0xffe1e8f0),
            width: selected ? 2 : 1,
          ),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Container(
                  width: 38.w,
                  height: 38.w,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    color: selected ? primary : const Color(0xfff6f9fc),
                    border: selected
                        ? null
                        : Border.all(color: const Color(0xffe1e8f0)),
                  ),
                  child: Icon(
                    icon,
                    color: selected ? Colors.white : const Color(0xff183652),
                    size: 22.sp,
                  ),
                ),
                SizedBox(width: 8.w),
                Expanded(
                  child: Text(
                    title,
                    style: TextStyle(
                      fontSize: 13.sp,
                      fontWeight: FontWeight.bold,
                      color: selected ? primary : const Color(0xff183652),
                    ),
                  ),
                ),
              ],
            ),
            SizedBox(height: 10.h),
            Text(
              description,
              style: TextStyle(
                fontSize: 11.sp,
                height: 1.4,
                color: ColorsManager.fadedText,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
