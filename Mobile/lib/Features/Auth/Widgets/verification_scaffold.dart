import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:go_router/go_router.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Core/Style/text_styles.dart';
import 'package:medlink/Features/Auth/Data/models/register_request_model.dart';

import 'package:medlink/Features/Auth/Presentation/Provider Signup/Cubit/provider_signup_cubit.dart';
import 'package:medlink/Features/Auth/Presentation/Provider Signup/Cubit/provider_signup_state.dart';

class VerificationScaffold extends StatefulWidget {
  VerificationScaffold({
    super.key,
    this.model,
    required this.stepNumber,
    required this.child,
    this.onSubmit,
  });

  Widget child;
  RegisterRequestModel? model;
  int stepNumber;
  final onSubmit;

  @override
  State<VerificationScaffold> createState() => _ProviderSignupState();
}

class _ProviderSignupState extends State<VerificationScaffold> {
  @override
  Widget build(BuildContext context) {
    return BlocListener<ProviderSignupCubit, ProviderSignupState>(
      listener: _handleStateChanges,
      child: Scaffold(
        backgroundColor: ColorsManager.primaryBackgroundColor,
        resizeToAvoidBottomInset: true,
        body: SafeArea(
          child: Column(
            children: [
              Expanded(
                child: SingleChildScrollView(
                  keyboardDismissBehavior:
                      ScrollViewKeyboardDismissBehavior.onDrag,
                  padding: const EdgeInsets.fromLTRB(20, 16, 20, 24),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Image.asset(
                        'assets/logo/logo.png',
                        width: 340.w,
                        height: 80.w,
                      ),
                      const SizedBox(height: 28),
                      _buildIntroduction(),
                      const SizedBox(height: 30),
                      VerificationProgress(stepNumber: widget.stepNumber),
                      const SizedBox(height: 32),

                      widget.child,
                    ],
                  ),
                ),
              ),
              _buildBottomActions(),
            ],
          ),
        ),
      ),
    );
  }

  void _handleStateChanges(BuildContext context, ProviderSignupState state) {
    if (state.status == ProviderSignupStatus.failure &&
        state.errorMessage != null) {
      ScaffoldMessenger.of(context)
        ..hideCurrentSnackBar()
        ..showSnackBar(SnackBar(content: Text(state.errorMessage!)));
    }

    if (state.status == ProviderSignupStatus.success) {
      ScaffoldMessenger.of(context)
        ..hideCurrentSnackBar()
        ..showSnackBar(
          const SnackBar(
            content: Text('Your application was submitted successfully.'),
          ),
        );
    }
  }

  Widget _buildIntroduction() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Verify your practice\nto start receiving patients.',
          style: TextStyle(
            color: ColorsManager.textDark,
            fontSize: 27,
            height: 1.25,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 12),
        const Text(
          'We review every provider before going live so '
          'patients can trust who they book with.',
          style: TextStyle(
            color: ColorsManager.fadedText,
            fontSize: 15,
            height: 1.5,
          ),
        ),
      ],
    );
  }

  Widget _buildBottomActions() {
    return BlocBuilder<ProviderSignupCubit, ProviderSignupState>(
      builder: (context, state) {
        final isBusy = state.isSubmitting || state.isPickingFile;

        return Container(
          padding: const EdgeInsets.fromLTRB(20, 12, 20, 12),
          decoration: BoxDecoration(
            color: Colors.white,
            border: const Border(
              top: BorderSide(color: ColorsManager.cardBorder),
            ),
            boxShadow: [
              BoxShadow(
                color: ColorsManager.textDark.withOpacity(0.05),
                blurRadius: 15,
                offset: const Offset(0, -4),
              ),
            ],
          ),
          child: Row(
            children: [
              Expanded(
                flex: 4,
                child: OutlinedButton(
                  onPressed: () => context.pop(),
                  style: OutlinedButton.styleFrom(
                    foregroundColor: ColorsManager.textDark,
                    side: const BorderSide(color: ColorsManager.cardBorder),
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(30),
                    ),
                  ),
                  child: const Text(
                    'Back',
                    style: TextStyle(fontWeight: FontWeight.bold),
                  ),
                ),
              ),
              const SizedBox(width: 12),

              widget.stepNumber == 2
                  ? Expanded(
                      flex: 6,
                      child: ElevatedButton(
                        onPressed: isBusy
                            ? null
                            : () async {
                                await widget.onSubmit();
                                if (widget.stepNumber == 2) {
                                  context.pushNamed('adminReview');
                                }
                              },
                        style: ElevatedButton.styleFrom(
                          backgroundColor: ColorsManager.primaryColor,
                          foregroundColor: Colors.white,
                          disabledBackgroundColor: ColorsManager.primaryColor
                              .withOpacity(0.5),
                          padding: const EdgeInsets.symmetric(vertical: 16),
                          elevation: 2,
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(30),
                          ),
                        ),
                        child: state.isSubmitting
                            ? const SizedBox(
                                height: 20,
                                width: 20,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                  color: Colors.white,
                                ),
                              )
                            : const Text(
                                'Submit for Review',
                                style: TextStyles.buttonText,
                              ),
                      ),
                    )
                  : SizedBox(),
            ],
          ),
        );
      },
    );
  }
}

class VerificationProgress extends StatelessWidget {
  const VerificationProgress({super.key, required this.stepNumber});

  final int stepNumber;

  @override
  Widget build(BuildContext context) {
    final currentStep = stepNumber.clamp(1, 3);

    return Column(
      children: [
        Row(
          children: [
            _buildCircle(1, currentStep),
            Expanded(child: _buildLine(currentStep >= 2)),
            _buildCircle(2, currentStep),
            Expanded(child: _buildLine(currentStep >= 3)),
            _buildCircle(3, currentStep),
          ],
        ),
        const SizedBox(height: 12),
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _buildLabel(
              'Create account',
              currentStep > 1 ? 'Completed' : 'You are here',
              current: currentStep == 1,
            ),
            _buildLabel(
              'Service & verification',
              currentStep > 2
                  ? 'Completed'
                  : currentStep == 2
                  ? 'You are here'
                  : 'Up next',
              current: currentStep == 2,
            ),
            _buildLabel(
              'Admin review',
              currentStep == 3 ? 'You are here' : 'Up to 48 hours',
              current: currentStep == 3,
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildCircle(int number, int currentStep) {
    final completed = number < currentStep;
    final current = number == currentStep;

    return Container(
      height: 42,
      width: 42,
      alignment: Alignment.center,
      decoration: BoxDecoration(
        color: completed ? ColorsManager.primaryColor : Colors.white,
        shape: BoxShape.circle,
        border: Border.all(
          color: current || completed
              ? ColorsManager.primaryColor
              : ColorsManager.cardBorder,
          width: current ? 2 : 1,
        ),
      ),
      child: Text(
        '$number',
        style: TextStyle(
          color: completed
              ? Colors.white
              : current
              ? ColorsManager.primaryColor
              : ColorsManager.textDark,
          fontSize: 19,
          fontWeight: FontWeight.bold,
        ),
      ),
    );
  }

  Widget _buildLine(bool active) {
    return Container(
      height: 4,
      color: active ? ColorsManager.primaryColor : ColorsManager.cardBorder,
    );
  }

  Widget _buildLabel(String title, String subtitle, {required bool current}) {
    return Expanded(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Text(
            title,
            textAlign: TextAlign.center,
            style: const TextStyle(
              color: ColorsManager.textDark,
              fontSize: 12,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            subtitle,
            textAlign: TextAlign.center,
            style: TextStyle(
              color: current
                  ? ColorsManager.primaryColor
                  : ColorsManager.fadedText,
              fontSize: 11,
            ),
          ),
        ],
      ),
    );
  }
}
