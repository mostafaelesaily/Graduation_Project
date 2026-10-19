import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Core/Style/text_styles.dart';

class AuthButton extends StatelessWidget {
  final String? text;
  final void Function()? onPressed;
  const new({super.key, required this.onPressed, this.text});

  @override
  Widget build(BuildContext context) {
    return Ink(
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: ColorsManager.primaryColor.withValues(alpha: 0.24),
            offset: Offset(0, 4),
            blurRadius: 14,
          ),
        ],
      ),
      child: ElevatedButton(
        onPressed: onPressed,
        style: ElevatedButton.styleFrom(
          backgroundColor: ColorsManager.primaryColor,
          minimumSize: Size(double.infinity, 44.w),
          elevation: 0,
        ),

        child: Text(text ?? '', style: TextStyles.buttonText),
      ),
    );
  }
}
