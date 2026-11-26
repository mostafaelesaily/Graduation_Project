import 'package:flutter/material.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/Auth/Widgets/verification_scaffold.dart';

class AdminReview extends StatelessWidget {
  const new({super.key});

  @override
  Widget build(BuildContext context) {
    return VerificationScaffold(stepNumber: 3, child: _buildReviewCard());
  }
}

Widget _buildReviewCard() {
  return Container(
    width: double.infinity,
    padding: const EdgeInsets.all(20),
    decoration: BoxDecoration(
      color: Colors.white,
      borderRadius: BorderRadius.circular(25),
      border: Border.all(color: ColorsManager.cardBorder),
      boxShadow: [
        BoxShadow(
          color: ColorsManager.primaryColor.withOpacity(0.06),
          blurRadius: 20,
          offset: const Offset(0, 5),
        ),
      ],
    ),
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Step 3 of 3',
          style: TextStyle(
            color: ColorsManager.primaryColor,
            fontSize: 14,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 8),

        Row(
          mainAxisAlignment: .center,
          children: [
            Container(
              height: 72,
              width: 72,
              decoration: BoxDecoration(
                color: ColorsManager.primaryColor.withOpacity(0.1),
                shape: BoxShape.circle,
              ),
              child: const Icon(
                Icons.access_time,
                color: ColorsManager.primaryColor,
                size: 36,
              ),
            ),
          ],
        ),
        SizedBox(height: 12),

        const Text(
          'Your request is under review',
          style: TextStyle(
            fontSize: 22,
            fontWeight: FontWeight.bold,
            color: ColorsManager.textDark,
          ),
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 8),

        const Text(
          'Our Admin team is reviewing your documents. We will email you as soon as the review is complete.',
          style: TextStyle(
            fontSize: 14,
            color: ColorsManager.fadedText,
            height: 1.5,
          ),
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 25),
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
          decoration: BoxDecoration(
            color: ColorsManager.primaryBackgroundColor, // Light blue/grey tint
            borderRadius: BorderRadius.circular(8),
          ),
          child: Row(
            children: [
              const Icon(
                Icons.mail_outline,
                color: ColorsManager.primaryColor,
                size: 20,
              ),
              const SizedBox(width: 12),
              Expanded(
                child: RichText(
                  text: const TextSpan(
                    style: TextStyle(
                      fontSize: 13,
                      color: ColorsManager.textDark,
                      height: 1.4,
                    ),
                    children: [
                      TextSpan(text: 'Confirmation will be sent to '),
                      TextSpan(
                        text: 'your registered email',
                        style: TextStyle(fontWeight: FontWeight.bold),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
        // const SizedBox(height: 18),

        // const SizedBox(height: 18),

        // const SizedBox(height: 18),

        // const SizedBox(height: 18),
      ],
    ),
  );
}
