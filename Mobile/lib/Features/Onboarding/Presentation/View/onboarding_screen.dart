import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:go_router/go_router.dart';
import 'package:medlink/Core/Shared/auth_button.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Core/Style/text_styles.dart';
import 'package:medlink/Features/Onboarding/Presentation/Models/onboarding_item.dart';
import 'package:medlink/Features/Onboarding/Presentation/View/widgets/onboarding_page.dart';

class OnboardingScreen extends StatefulWidget {
  const OnboardingScreen({super.key});

  @override
  State<OnboardingScreen> createState() => _OnboardingScreenState();
}

class _OnboardingScreenState extends State<OnboardingScreen> {
  late final PageController _controller;
  int _currentPage = 0;

  bool get _isLast => _currentPage == onboardingItems.length - 1;

  @override
  void initState() {
    super.initState();
    _controller = PageController();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _onNext() {
    if (_isLast) {
      // "Get Started" -> create an account.
      context.goNamed('signup');
    } else {
      _controller.nextPage(
        duration: const Duration(milliseconds: 350),
        curve: Curves.easeInOut,
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Column(
          children: [
            _Header(onSkip: () => context.goNamed('login')),
            Expanded(
              child: PageView.builder(
                controller: _controller,
                itemCount: onboardingItems.length,
                onPageChanged: (index) => setState(() => _currentPage = index),
                itemBuilder: (context, index) => OnboardingPage(
                  item: onboardingItems[index],
                  controller: _controller,
                  count: onboardingItems.length,
                ),
              ),
            ),
            Padding(
              padding: EdgeInsets.fromLTRB(16.w, 8.w, 16.w, 8.w),
              child: AuthButton(
                onPressed: _onNext,
                icon: Row(
                  mainAxisSize: MainAxisSize.min,
                  spacing: 6.w,
                  children: [
                    Text(
                      _isLast ? 'Get Started' : 'Continue',
                      style: TextStyles.buttonText,
                    ),
                    Icon(Icons.arrow_forward, size: 16.w, color: Colors.white),
                  ],
                ),
              ),
            ),
            Padding(
              padding: EdgeInsets.only(bottom: 12.w),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Text(
                    'Already have an account? ',
                    style: TextStyle(
                      fontSize: 12,
                      color: ColorsManager.fadedText,
                    ),
                  ),
                  TextButton(
                    style: TextButton.styleFrom(
                      minimumSize: const Size(0, 16),
                      padding: EdgeInsets.zero,
                      tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                      visualDensity: VisualDensity.compact,
                    ),
                    onPressed: () => context.goNamed('login'),
                    child: const Text(
                      'Log In',
                      style: TextStyle(
                        fontSize: 12,
                        fontWeight: FontWeight.bold,
                        color: ColorsManager.primaryColor,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Logo + tagline on the left, "Skip" on the right.
class _Header extends StatelessWidget {
  final VoidCallback onSkip;

  const _Header({required this.onSkip});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: EdgeInsets.fromLTRB(16.w, 12.w, 16.w, 4.w),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              SizedBox(
                height: 34.w,
                child: Image.asset('assets/logo/logo.png', fit: BoxFit.contain),
              ),
              Text(
                'Your Health. Our Connection.',
                style: TextStyles.fadedText.copyWith(fontSize: 10),
              ),
            ],
          ),
          TextButton(
            onPressed: onSkip,
            child: const Text(
              'Skip',
              style: TextStyle(
                fontSize: 14,
                fontWeight: FontWeight.w600,
                color: ColorsManager.primaryColor,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
